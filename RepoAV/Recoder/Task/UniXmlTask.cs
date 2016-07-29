using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using PSNC.RepoAV.Common;
using PSNC.Util;
using PSNC.RepoAV.TaskQueue;
using System.IO;
using System.Threading;
using PSNC.RepoAV.RepDBAccess;

namespace PSNC.RepoAV.Recoder
{
	public class UniXmlTask : BaseTask
	{
		protected string[] m_InputFormatIds;
		protected string[] m_OutputFormatIds;
		protected string[] m_InputFilePaths;//sciezki do pliku po sciagnieciu przez http
		protected bool[] m_DownloadSourceFileFlags;//flsagi decydujące, czy zaciągac format, czy przekazać URLa
		protected string[] m_OutputFilePaths;//sciezki do plików wynikowych 
		protected string[] m_Parameters;
		protected List<Guid> m_Guids;
		protected RecoderSubsystem m_Subsystem;
		protected TaskDefinition m_Definition;
		protected OperationDefinition m_CurrentOperation;		
		protected bool m_WasCancelled;
		protected List<SingleOperationResult> m_OperationsPassed;
		protected Process m_RunningProcessHandle;
		protected long m_RepoTaskId;
		protected bool m_ResultSent;
		protected bool m_LastCall2SNode;

		public string[] InputFormatIds
		{
			get { return m_InputFormatIds; }
			set { m_InputFormatIds = value; }
		}
		public string[] OutputFormatIds
		{
			get { return m_OutputFormatIds; }
			set { m_OutputFormatIds = value; }
		}
		public string[] OutputFilePaths
		{
			get { return m_OutputFilePaths; }
			set { m_OutputFilePaths = value; }
		}
		public bool WasCancelled
		{
			get { return m_WasCancelled; }
			set { m_WasCancelled = value; }
		}
		public TaskDefinition Definition
		{
			get { return m_Definition; }
			set { m_Definition = value; }
		}
		public string[] Parameters
		{
			get { return m_Parameters; }
			set { m_Parameters = value; }
		}
		public SingleOperationResult[] OperationsPassed
		{
			get { return m_OperationsPassed.ToArray(); }
		}
		public SingleOperationResult LastOperation
		{
			get
			{
				if (OperationsPassed.Length > 0)
					return OperationsPassed[OperationsPassed.Length - 1];
				return null;
			}
		}
		public OperationDefinition CurrentOperation
		{
			get { return m_CurrentOperation; }
			set { m_CurrentOperation = value; }
		}
		public long RepoTaskId
		{
			get { return m_RepoTaskId; }
		}

		public override bool IsMainEntryTask { get { return true; } }//okresla, czy jest to zadanie inicjowane bezposrednio z zewnatrz (czyli glowne)
		public override bool CanWaitForOtherTask { get { return true; } }


		public UniXmlTask(long repoTaskId, string[] inputFormatIds, string[] outputFormatIds, string[] parameters, TaskDefinition definition, RecoderSubsystem subsystem, bool[] downloadSourceFiles)
			: this(repoTaskId, inputFormatIds, outputFormatIds, parameters, definition, DateTime.Now, -1, subsystem, downloadSourceFiles)
		{
		}

		public UniXmlTask(long repoTaskId, string[] inputFormatIds, string[] outputFormatIds, string[] parameters, TaskDefinition definition, DateTime startDate, long parentTaskID, RecoderSubsystem subsystem, bool[] downloadSourceFiles)
			: base(startDate, parentTaskID)
		{
			m_OutputFormatIds = outputFormatIds;
			m_CurrentOperation = null;
			m_OperationsPassed = new List<SingleOperationResult>();
			m_RunningProcessHandle = null;
			m_InputFormatIds = inputFormatIds;
			if (m_InputFormatIds != null)
				m_InputFilePaths = new string[m_InputFormatIds.Length];
			m_Guids = new List<Guid>();
			m_Parameters = parameters;
			m_Definition = definition;
			m_WasCancelled = false;
			m_Subsystem = subsystem;
			m_RepoTaskId = repoTaskId;
			m_FinishAfterLastChild = false;
			m_ResultSent = false;
			m_LastCall2SNode = false;
			m_DownloadSourceFileFlags = downloadSourceFiles;
		}


		protected string GetInputFormatId(int idx)
		{
			if (m_InputFormatIds != null && m_InputFormatIds.Length > idx)
				return m_InputFormatIds[idx];
			return null;
		}

		protected string GetOutputFormatId(int idx)
		{
			if (m_OutputFormatIds != null && m_OutputFormatIds.Length > idx)
				return m_OutputFormatIds[idx];
			return null;
		}

		private string GetParameter(int idx)
		{
			if (m_Parameters != null)
				return m_Parameters[idx];
			return null;
		}

		protected SingleOperationResult GetLastOperation()
		{
			lock (m_OperationsPassed)
			{
				if (m_OperationsPassed.Count < 1)
					return null;
				return m_OperationsPassed[m_OperationsPassed.Count - 1];
			}
		}

		protected OperationDefinition GetOperationByName(string name)
		{
			foreach (OperationDefinition od in m_Definition.Operations)
				if (string.Compare(od.Name, name, true) == 0)
					return od;

			return null;
		}

		private bool ShiftToNextOperation()//true, jesli ma sie juz zakonczyc
		{
			if (m_Definition == null)
				return false;

			SingleOperationResult lastOper = LastOperation;
			bool result = true;
			if (m_CurrentOperation == null)//poczatek
			{
				foreach (OperationDefinition od in m_Definition.Operations)
					if (string.Compare(od.Name, m_Definition.StartOperation, true) == 0)
					{
						result = false;//jest jakas operacja - idziemy do przodu
						m_CurrentOperation = od;
						break;
					}
				if (m_CurrentOperation == null && m_Definition.Operations.Length > 0)
					m_CurrentOperation = m_Definition.Operations[0];
			}
			else
			{
				string nextOper = (lastOper == null || lastOper.Success) ? m_CurrentOperation.OnSuccess : m_CurrentOperation.OnFailure;
				if (string.IsNullOrEmpty(nextOper))
				{
					if (lastOper != null && !lastOper.Success)
					{
						AddErrorDesc((int)ErrorType.Runtime, lastOper.Comments, "| ");
					}
					m_CurrentOperation = null;
					return true;//konczymy
				}

				m_CurrentOperation = GetOperationByName(nextOper);
				if (m_CurrentOperation == null)
				{
					lastOper = new SingleOperationResult(nextOper) { Comments = string.Format("Błędna definicja zadania: nie odnaleziono operacji o nazwie '{0}'.", nextOper), Success = false };
					AddErrorDesc((int)ErrorType.Runtime, lastOper.Comments, ";");
					m_OperationsPassed.Add(lastOper);
					return true;//konczymy
				}
				result = false;
			}


			if (m_CurrentOperation != null && m_CurrentOperation.Skip)
			{
				if (m_CurrentOperation.SkipResultSpecified)
				{
					lastOper = new SingleOperationResult(m_CurrentOperation.Name)
					{
						Comments = string.Format("Operacja została pominięta z wynikiem '{0}'.", m_CurrentOperation.SkipResult),
						Success = (m_CurrentOperation.SkipResult == SkipResults.Success || m_CurrentOperation.SkipResult == SkipResults.StopExecutionWithSuccess)
					};
					m_OperationsPassed.Add(lastOper);

					if (m_CurrentOperation.SkipResult == SkipResults.Failure || m_CurrentOperation.SkipResult == SkipResults.Success)
					{
						return ShiftToNextOperation();
					}
					else
					{
						m_CurrentOperation = null;
						result = true;
					}
				}
				else
				{
					m_CurrentOperation = null;
					result = true;
				}
			}

			return result;
		}

		protected string InsertSpecialSigns(string template, string[] outputPathWithOutExts)
		{
			string cmdLine = template;
			cmdLine = cmdLine.Replace("\n", "").Trim();
			cmdLine = cmdLine.Replace("%%tools", m_Subsystem.ToolsFolder);
			cmdLine = cmdLine.Replace("%%t", m_Subsystem.TempFolder);
			cmdLine = cmdLine.Replace(@"%%di", m_Subsystem.TempFolder);
			cmdLine = cmdLine.Replace(@"%%do", m_Subsystem.TempFolder);

			if (m_InputFilePaths != null)
			{
				string[] inputExts = new string[m_InputFilePaths.Length];
				string[] inputPathWithOutExts = new string[m_InputFilePaths.Length];
				for (int x = 0; x < m_InputFilePaths.Length; x++)
					if (!string.IsNullOrEmpty(m_InputFilePaths[x]))
					{
						inputPathWithOutExts[x] = Path.GetFileNameWithoutExtension(m_InputFilePaths[x]);
						inputExts[x] = Path.GetExtension(m_InputFilePaths[x]);
					}
					else
					{
						inputExts[x] = string.Empty;
						inputPathWithOutExts[x] = string.Empty;
					}

				for (int x = 0; x < Math.Max(1000, m_InputFilePaths.Length); x++)
				{
					int x1 = x + 1;
					if (x < m_InputFilePaths.Length && !string.IsNullOrEmpty(m_InputFilePaths[x]) && File.Exists(m_InputFilePaths[x]))
					{
						cmdLine = cmdLine.Replace(@"%%i" + x1.ToString(), m_InputFilePaths[x]);
						cmdLine = cmdLine.Replace(@"%%iw" + x1.ToString(), inputPathWithOutExts[x]);
						cmdLine = cmdLine.Replace(@"%%eI" + x1.ToString(), inputExts[x]);
					}
					else
					{
						cmdLine = cmdLine.Replace(@"%%i" + x1.ToString(), string.Empty);
						cmdLine = cmdLine.Replace(@"%%iw" + x1.ToString(), string.Empty);
						cmdLine = cmdLine.Replace(@"%%eI" + x1.ToString(), string.Empty);
					}
				}

				if (!string.IsNullOrEmpty(m_InputFilePaths[0]) && File.Exists(m_InputFilePaths[0]))
				{
					cmdLine = cmdLine.Replace(@"%%iw", inputPathWithOutExts.Length > 0 ? inputPathWithOutExts[0] : string.Empty);
					cmdLine = cmdLine.Replace(@"%%i", m_InputFilePaths.Length > 0 ? m_InputFilePaths[0] : string.Empty);
					cmdLine = cmdLine.Replace(@"%%eI", inputExts.Length > 0 ? inputExts[0] : string.Empty);
				}
				else
				{
					cmdLine = cmdLine.Replace(@"%%i", string.Empty);
					cmdLine = cmdLine.Replace(@"%%iw", string.Empty);
					cmdLine = cmdLine.Replace(@"%%eI", string.Empty);
				}
			}


			if (m_OutputFilePaths != null)
			{
				string[] outputExts = new string[m_OutputFilePaths.Length];
				for (int x = 0; x < m_OutputFilePaths.Length; x++)
					if (!string.IsNullOrEmpty(m_OutputFilePaths[x]))
						outputExts[x] = Path.GetExtension(m_OutputFilePaths[x]);
					else
						outputExts[x] = string.Empty;

				for (int x = 0; x < Math.Max(1000, m_OutputFilePaths.Length); x++)
				{
					int x1 = x + 1;
					if (x < m_OutputFilePaths.Length && !string.IsNullOrEmpty(m_OutputFilePaths[x]))
					{
						cmdLine = cmdLine.Replace(@"%%o" + x1.ToString(), m_OutputFilePaths[x]);
						cmdLine = cmdLine.Replace(@"%%ow" + x1.ToString(), outputPathWithOutExts[x]);
						cmdLine = cmdLine.Replace(@"%%eO" + x1.ToString(), outputExts[x]);
					}
					else
					{
						cmdLine = cmdLine.Replace(@"%%o" + x1.ToString(), string.Empty);
						cmdLine = cmdLine.Replace(@"%%ow" + x1.ToString(), string.Empty);
						cmdLine = cmdLine.Replace(@"%%eO" + x1.ToString(), string.Empty);
					}
				}


				if (!string.IsNullOrEmpty(m_OutputFilePaths[0]))
				{
					cmdLine = cmdLine.Replace(@"%%ow", outputPathWithOutExts.Length > 0 ? outputPathWithOutExts[0] : string.Empty);
					cmdLine = cmdLine.Replace(@"%%o", m_OutputFilePaths.Length > 0 ? m_OutputFilePaths[0] : string.Empty);
					cmdLine = cmdLine.Replace(@"%%eO", outputExts.Length > 0 ? outputExts[0] : string.Empty);
				}
				else
				{
					cmdLine = cmdLine.Replace(@"%%o", string.Empty);
					cmdLine = cmdLine.Replace(@"%%ow", string.Empty);
					cmdLine = cmdLine.Replace(@"%%eO", string.Empty);
				}
			}


			int i = 1;
			if (m_Parameters != null)
			{
				i = 1;
				foreach (string p in m_Parameters)
				{
					cmdLine = cmdLine.Replace(@"%%" + i.ToString(), p);
					i++;
				}
			}


			i = 1;
			Guid g;
			while (cmdLine.Contains(@"%%g" + i.ToString()))
			{
				if (m_Guids.Count < i)
				{
					g = Guid.NewGuid();
					m_Guids.Add(g);
				}
				else
					g = m_Guids[i - 1];

				cmdLine = cmdLine.Replace(@"%%g" + i.ToString(), g.ToString());
				i++;
			}
			cmdLine = cmdLine.Replace(@"%%u", ID.ToString());

			return cmdLine;
		}

		public void CancelTask()
		{
			m_WasCancelled = true;
			if (m_RunningProcessHandle != null)
			{
				m_RunningProcessHandle.Kill();
				m_Subsystem.AfterProcessEnded(m_RunningProcessHandle.Id, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));				
				m_RunningProcessHandle = null;
			}
		}


		protected override void GetDetailsAfterFinished(StringBuilder sb)
		{
			if (sb == null)
				return;

			base.GetDetailsAfterFinished(sb);
			sb.AppendFormat(", InFormatIds={0}, DownloadInputFlags={1}", m_InputFormatIds == null ? "NULL" : string.Join("|", m_InputFormatIds), m_DownloadSourceFileFlags == null ? "NULL" : string.Join("|", m_DownloadSourceFileFlags));
			sb.AppendFormat(", OutFormatIds={0}", m_OutputFormatIds == null ? "NULL" : string.Join("|", m_OutputFormatIds));
			sb.AppendFormat(", TaskDefinition='{0}', ParametersCount={1}, WasCancelled={2}", m_Definition.Name, m_Parameters == null ? 0 : m_Parameters.Length, m_WasCancelled);
			sb.AppendFormat(", OutFilePath='{0}'", m_OutputFilePaths == null ? "NULL" : string.Join("|", m_OutputFilePaths));
		}

		protected bool AreAnyFormatsCommon(UniXmlTask task)
		{
			if (task is UniXmlTask)
			{
				UniXmlTask ut = (UniXmlTask)task;

				if (ut.InputFormatIds != null && m_InputFormatIds != null && ut.InputFormatIds.FirstOrDefault(inf => m_OutputFormatIds.Contains(inf))  != null)
					return true;
			}

			return false;
		}

		protected override bool ShouldAskingTaskWaitForMe(BaseTask askingTask)
		{
			UniXmlTask ft = askingTask as UniXmlTask;

			if (ft != null && ft != this)
			{
				if (AreAnyFormatsCommon(ft))
				{
					Manager.ShowText(string.Format("Zadanie typu '{2}' o nr {0} powinno czekać na koniec zadania nr {1} typu '{3}' z powodu takich samych formatów we/wy.", askingTask.ID, ID, askingTask.Type, Type), TraceEventType.Information);
					return true;
				}
			}
			return false;
		}

		protected override bool CanBeRunningLonger(double runningTime)//w sekundach
		{
			int time = 3600;
			if (Definition != null)
				time = Definition.ExecutionTimeout;

			Manager.ShowText(string.Format("Zapytanie o możliwość kontynuacji wykonania zadania ze względu na przekroczony czas przetrzymywania wątka wykonawczego. Zadanie wykonuje sie już {0} s, a max to {1}s.", runningTime, time), System.Diagnostics.TraceEventType.Verbose);

			if (runningTime > time)
				return false;
			return true;
		}
		protected override bool CanBeWaitingLonger(double totalWaitingSeconds, int maxPeriodSeconds)
		{
			if (State == TaskState.WaitingForAnswer)
			{
				bool isTaskKnown = false;
				foreach(long childId in m_ChildrenIDs)
				{
					if (m_Subsystem.SNodeSubsystem.IsTaskStillRunning(childId, out isTaskKnown))
					{
						return true;
					}
				}
			}
			return base.CanBeWaitingLonger(totalWaitingSeconds, maxPeriodSeconds);
		}

		protected override void BeforeFinish()
		{
			if (m_RunningProcessHandle != null)
			{
				m_RunningProcessHandle.Close();
				m_Subsystem.AfterProcessEnded(m_RunningProcessHandle.Id, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));
				m_RunningProcessHandle = null;
			}

			AddInfo = Array.ConvertAll(OperationsPassed, so => so.ToString());
			base.BeforeFinish();
		}

		protected string GetFullPathForOutput(string uniqueId)
		{
			Format format = m_Subsystem.RepoDBAccess.GetFormat(uniqueId);
			if (format == null)
				return null;
			string fileExt = m_Subsystem.SNodeSubsystem.GetExtension4MimeType(format.Mime);

			int i = 1;
			string fullPath = Path.Combine(m_Subsystem.TempFolder, uniqueId + fileExt);
			while(File.Exists(fullPath))
				fullPath = Path.Combine(m_Subsystem.TempFolder, uniqueId + "_" + (i++).ToString() + fileExt);

			return fullPath;
		}

		protected override void DoSpecificAction()
		{
			if (m_CurrentOperation == null) // poczatek
			{
				if (m_RepoTaskId > -1)
					m_Subsystem.RepoDBAccess.UpdateTaskLastActivityDate(m_RepoTaskId);

				m_FinishAfterLastChild = false;
				if (m_InputFormatIds == null || m_InputFormatIds.Length < 1 || m_InputFormatIds.Where(s => string.IsNullOrEmpty(s)).Count() > 0)
				{
					FinishTaskWithError((int)ErrorType.InvalidParameter, "Niepoprawny parametr określający identyfikatory formatów wejściowych.");
					return;
				}
				if (m_Definition == null || m_Definition.Operations.Length < 1)
				{
					FinishTaskWithError((int)ErrorType.InvalidParameter, "Niepoprawny parametr - brak poprawnej definicji zadania.");
					return;
				}

				Manager.ShowText(string.Format("Wykonanie zadania rekodowania o nazwie '{0}' dla we='{1}' i wy='{2}'!", m_Definition.Name, m_InputFormatIds == null ? "null" : string.Join("|", m_InputFormatIds), m_OutputFormatIds == null ? "null" : string.Join("|", m_OutputFormatIds)), TraceEventType.Information);

				for (int i = 0; i < m_InputFormatIds.Length; i++)
				{
					string uniqueId = m_InputFormatIds[i];

					string location = m_Subsystem.SNodeSubsystem.GetFormatLocation(uniqueId);
					if (string.IsNullOrEmpty(location) && m_DownloadSourceFileFlags[i] == true)
					{
						long childTaskId;
						if (m_Subsystem.SNodeSubsystem.OrderInsertFormatTask(ID, uniqueId, null, null, null, null, null, out childTaskId) == ErrorType.Success)
						{
							State = TaskState.WaitingForAnswer;
							AddChild(childTaskId);
						}
						else
						{
							FinishTaskWithError((int)ErrorType.TransmittingFailed, "Nie udało się zlecić SNode'owi zadania pobrania formatu o Id=" + uniqueId);
							return;
						}
					}
					else
					{
						m_InputFilePaths[i] = location;
					}
				}
				if (State == TaskState.WaitingForAnswer)
					return;
			}

			if (ShiftToNextOperation())
			{
				State = TaskState.WaitingForFinish;
				return;
			}

			if (m_InputFilePaths != null)
			{
				foreach (string fs in m_InputFilePaths)
				{
					if (string.IsNullOrEmpty(fs) || !System.IO.File.Exists(fs))
					{
						FinishTaskWithError((int)ErrorType.TransmittingFailed, "Nie udało się pozyskać przynajmniej jednego z formatów wymaganych do rekodowania.");
						return;
					}
				}
			}

			m_OutputFilePaths = new string[m_OutputFormatIds.Length];
			string[] outputPathsWithOutExt = new string[m_OutputFormatIds.Length];
			for (int i = 0; i < m_OutputFormatIds.Length; i++)
			{
				m_OutputFilePaths[i] = GetFullPathForOutput(m_OutputFormatIds[i]);
				outputPathsWithOutExt[i] = m_OutputFilePaths[i];
				int pos = m_OutputFilePaths[i].LastIndexOf('.');
				if (pos > 0 && pos >= m_OutputFilePaths[i].Length - 6)
					outputPathsWithOutExt[i] = m_OutputFilePaths[i].Substring(0, pos);
			}


			while (m_CurrentOperation != null && !m_WasCancelled)
			{
				SingleOperationResult sor = new SingleOperationResult(m_CurrentOperation.Name);
				try
				{
					DoCmdLineOperation(outputPathsWithOutExt, sor);
					Manager.ShowText(string.Format("Operacja '{0}' zakończona z wynikiem: {1}. Komentarz: {2}.", m_CurrentOperation.Name, sor.Success, sor.Comments), TraceEventType.Verbose);
				}
				catch (ThreadAbortException)
				{
					sor.Success = false;
					sor.Comments = "Wątek wykonawczy został zatrzymany przez Zarządce kolejki zadań.";
					m_OperationsPassed.Add(sor);
					Thread.ResetAbort();
					FinishTaskWithError((int)ErrorType.ExecutionTimeout, "Wątek wykonawczy został zatrzymany przez Zarządce kolejki zadań.");
					return;
				}
				catch (Exception ex)
				{
					sor.Success = false;
					sor.Comments = ex.Message;
					Manager.ReportError(string.Format("Wyjątek podczas wykonania operacji."), ex);
				}
				finally
				{
					if (m_RunningProcessHandle != null)
						m_RunningProcessHandle.Close();
					m_RunningProcessHandle = null;
				}

				if (m_WasCancelled)
				{
					sor.Comments = "Zadanie zostało przerwane na życzenie zlecającego.";
					sor.Success = false;
				}

				m_OperationsPassed.Add(sor);

				if (!m_WasCancelled)
				{
					if (ShiftToNextOperation())
						break;
				}
			}



			if (!m_WasCancelled && m_ErrorCode == (int)ErrorType.Success)
			{
				List<int> lstOutputs2OrderInsert = new List<int>();
				List<string> lstOutputsMimes = new List<string>();
				for (int i = 0; i < m_OutputFilePaths.Length; i++)
				{
					FileInfo fi = new FileInfo(m_OutputFilePaths[i]);
					if (fi.Exists)
					{
						ulong duration = 0;
						string mime;
						string metadata = m_Subsystem.SNodeSubsystem.GetFormatXmlForFile("", m_OutputFilePaths[i], "PUBLIC", out duration, out mime);
						m_Subsystem.RepoDBAccess.SetFormatMetadataExt(m_OutputFormatIds[i], metadata, null, fi.Length, mime);

						lstOutputs2OrderInsert.Add(i);
						lstOutputsMimes.Add(mime);
					}
				}

				if (lstOutputs2OrderInsert.Count < 1)
				{
					if (m_OutputFormatIds.Length > 0)
					{
						FinishTaskWithError((int)ErrorType.NotFound, string.Format("Zadanie o Id={0} i RepoId={1} nie zakończyło się wygenerowaniem ani jednego nowego pliku.", ID, m_RepoTaskId));
						return;
					}
					else
					{
						Manager.ShowText(string.Format("Zadanie o Id={0} i RepoId={1} nie zakończyło się wygenerowaniem ani jednego nowego pliku .", ID, m_RepoTaskId), TraceEventType.Warning);
						State = TaskState.WaitingForFinish;
					}
				}
				else
				{

					State = TaskState.WaitingForAnswer;
					for (int i = 0; i < lstOutputs2OrderInsert.Count; i++)
					{
						long childTaskId;
						if (m_Subsystem.SNodeSubsystem.OrderInsertFormatTask(ID, m_OutputFormatIds[lstOutputs2OrderInsert[i]], null, lstOutputsMimes[i], Path.GetExtension(m_OutputFilePaths[lstOutputs2OrderInsert[i]]), null, "file://" + m_OutputFilePaths[lstOutputs2OrderInsert[i]], out childTaskId) == ErrorType.Success)
							AddChild(childTaskId);
						else
						{
							FinishTaskWithError((int)ErrorType.TransmittingFailed, "Nie udało się zlecić SNode'owi zadania pozyskania(FileMove) formatu o Id=" + m_OutputFormatIds[lstOutputs2OrderInsert[i]]);
							return;
						}
					}
					m_LastCall2SNode = true;

					lock (m_ChildrenIDs)
					{
						if (m_ChildrenIDs.Count > 0)
						{
							m_FinishAfterLastChild = true;
							State = TaskState.WaitingForAnswer;
						}
						else
						{
							State = TaskState.WaitingForFinish;
						}
					}
				}
			}
		}

		protected void DoCmdLineOperation(string[] outputPathsWithOutExt, SingleOperationResult sor)
		{
			string cmdLine = m_CurrentOperation.Value;
			if (string.IsNullOrEmpty(cmdLine))
			{
				sor.Comments = "Wykonano pustą operacje typu CommandLine!";
				sor.Success = true;
			}
			else
			{
				cmdLine = InsertSpecialSigns(cmdLine, outputPathsWithOutExt);

				Manager.ShowText(string.Format("Wykonanie operacji typu CmdLine w postaci: {0}.", cmdLine), System.Diagnostics.TraceEventType.Verbose);

				m_RunningProcessHandle = new Process();
				m_RunningProcessHandle.StartInfo = new ProcessStartInfo("cmd.exe", "/C " + cmdLine);
				m_RunningProcessHandle.StartInfo.UseShellExecute = true;
				m_RunningProcessHandle.StartInfo.CreateNoWindow = true;
				m_RunningProcessHandle.StartInfo.WorkingDirectory = m_Subsystem.ToolsFolder;

				try
				{
					m_RunningProcessHandle.Start();
					m_Subsystem.AfterNewProcessStarted(m_RunningProcessHandle.Id, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));
					m_RunningProcessHandle.PriorityClass = ProcessPriorityClass.BelowNormal;

					if (!m_RunningProcessHandle.HasExited)
						m_RunningProcessHandle.WaitForExit();

					sor.Success = m_RunningProcessHandle.ExitCode == 0;
				}
				catch (ThreadAbortException)
				{
					sor.Success = false;
					sor.Comments = "Wątek wykonawczy został zatrzymany przez Zarządce kolejki zadań.";
					m_OperationsPassed.Add(sor);
					Thread.ResetAbort();

					m_Subsystem.KillProcessAndChildren(m_RunningProcessHandle.Id);
					m_Subsystem.AfterProcessEnded(m_RunningProcessHandle.Id, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));

					FinishTaskWithError((int)ErrorType.ExecutionTimeout, "Wątek wykonawczy został zatrzymany przez Zarządce kolejki zadań.");
					return;
				}
				catch
				{
					m_Subsystem.KillProcessAndChildren(m_RunningProcessHandle.Id);
					sor.Success = false;
				}
				m_Subsystem.AfterProcessEnded(m_RunningProcessHandle.Id, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));

				if (string.IsNullOrEmpty(sor.Comments))
					sor.Comments = string.Format("Process CmdLine['{1}'] zakończył się z wartością: {0}.", m_RunningProcessHandle.ExitCode, cmdLine);
				if (!sor.Success)
					Manager.ShowText(sor.Comments, System.Diagnostics.TraceEventType.Warning);
				else
					Manager.ShowText(sor.Comments, System.Diagnostics.TraceEventType.Verbose);

				
			}
		}

		protected override void AfterFinish()
		{
			base.AfterFinish();

			if (m_RepoTaskId > -1 && m_ResultSent == false)
			{
				try
				{
					Manager.ShowText(string.Format("Wysłanie info o zakończeniu zadania UniXML o Id={0} i RepoId={3} do bazy RepoDB. Wynikiem: {1}:{2}.", ID, m_ErrorCode, m_ErrorDesc, m_RepoTaskId), TraceEventType.Verbose);

					m_Subsystem.RepoDBAccess.SetTaskResult(m_RepoTaskId, (m_ErrorCode == (int)ErrorType.Success) ? RepDBAccess.TaskStatus.Success : RepDBAccess.TaskStatus.Failure, Result);
					m_ResultSent = true;
				}
				catch (Exception ei)
				{
					Manager.ReportError(string.Format("Błąd podczas komunikacji z bazą RepoDB. Recoder nie zdołał powiadomić o wynikach zadania nr {0}.", m_RepoTaskId), ei);
				}
			}
		}

		public override bool OnMessageReceived(MessageBody mb)
		{
			bool res = base.OnMessageReceived(mb);
			if (mb.m_Type == "ShiftStateMsg" && m_LastCall2SNode == true && res == true)
			{
				if (m_RepoTaskId > -1 && m_ResultSent == false)
				{
					try
					{
						Manager.ShowText(string.Format("Wysłanie info o zakończeniu zadania UniXML o Id={0} i RepoId={3} do bazy RepoDB. Wynikiem: {1}:{2}.", ID, m_ErrorCode, m_ErrorDesc, m_RepoTaskId), TraceEventType.Verbose);

						m_Subsystem.RepoDBAccess.SetTaskResult(m_RepoTaskId, (m_ErrorCode == (int)ErrorType.Success) ? RepDBAccess.TaskStatus.Success : RepDBAccess.TaskStatus.Failure, Result);
						m_ResultSent = true;
					}
					catch (Exception ei)
					{
						Manager.ReportError(string.Format("Błąd podczas komunikacji z bazą RepoDB. Recoder nie zdołał powiadomić o wynikach zadania nr {0}.", m_RepoTaskId), ei);
					}
				}				
			}
			return res;
		}

	}

}
