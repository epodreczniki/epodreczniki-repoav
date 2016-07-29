using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Management;
using System.Diagnostics;
using PSNC.Proca3.Subsystem;
using PSNC.RepoAV.SNode;
using PSNC.Util;
using System.IO;
using System.Reflection;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.TaskQueue;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.Recoder
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class RecoderSubsystem : Subsystem, IRecoderSubsytem, ISubsystemService
    {
		[SubsystemParameter("Odstęp pomiędzy kolejnymi wołaniami wątku czyszczącego katalog Temp (w sekundach)!",
						DefaultValue = 600,
						MaxValue = Int32.MaxValue,
						MinValue = 10,
						IsReadOnly = false)]
		protected Parameter<int> m_CleaningInterval;

		[SubsystemParameter("Ścieżka do katalogu temporalnego używanego przy rekodowaniu.",
						DefaultValue = "c:\\Temp",
						IsReadOnly = true)]
		private Parameter<string> m_TempDirectoryPath;


		[SubsystemParameter("Ścieżka do katalogu z narzędziami używanymi przy rekodowaniu.",
						DefaultValue = "c:\\Tools",
						IsReadOnly = true)]
		private Parameter<string> m_ToolsDirectoryPath;


		[SubsystemParameter("Liczba wątków wykonujących równolegle zadania rekodowania.",
						DefaultValue = 5,
						MaxValue = 1000,
						MinValue = 1,
						IsReadOnly = false)]
		private Parameter<int> m_ExecutingThreadsCount;

		[SubsystemParameter("Max czas, jaki może pojedynczy wątek wykonawczy spędzić na aktywnym wykonaniu zadania (w sekundach).",
						DefaultValue = 600,
						MaxValue = int.MaxValue,
						MinValue = 1,
						IsReadOnly = false)]
		private Parameter<int> m_MaxTimeOfWorkingThreadExecution;


		[SubsystemParameter("Max liczba zadań pobieranych przy każdorazowym zapytaniu do bazy RepoDB o zadania do wykonania.",
						DefaultValue = 5,
						MaxValue = int.MaxValue,
						MinValue = 1,
						IsReadOnly = false)]
		private Parameter<int> m_MaxNewTasksCount;

		[SubsystemParameter("Czas w sekundach, co jaki Recoder sprawdza bazę RepDB pod kątem nowych zadań rekodowania do wykonania.",
						DefaultValue = 10,
						MaxValue = int.MaxValue,
						MinValue = 1,
						IsReadOnly = false)]
		private Parameter<int> m_Checking4NewTasksInterval;

		[SubsystemParameter("Lista obsługiwanych podtypów zadań (dla typu Recode) odzielonych średnikiem.",
						DefaultValue = "",
						IsReadOnly = true)]
		private Parameter<string> m_TaskSubtypes;


		private ISNodeSubsystem m_SNode;
		protected DateTime m_LastTempCleaning;
		protected bool m_DuringTaskOrdering;
		protected DateTime m_LastChecking4NewTasksDate;
		protected RepDBAccess.RepDBAccess m_RepoDBAccess;
		protected TaskQueueManager m_TaskQueue;
		protected HashSet<long> m_TasksInSystem;
		protected TaskTypeComplex[] m_MyTaskTypes;

        public string GetId()
        {
            return Guid.NewGuid().ToString();
        }
		public string TempFolder
		{
			get
			{
				return m_TempDirectoryPath.Value;
			}
		}
		public string ToolsFolder
		{
			get
			{
				return m_ToolsDirectoryPath.Value;
			}
		}
        public override string GetName()
        {
            return "Recoder";
        }
		internal RepDBAccess.RepDBAccess RepoDBAccess
		{
			get { return m_RepoDBAccess; }
		}
		internal ISNodeSubsystem SNodeSubsystem
		{
			get
			{
				return m_SNode;
			}
		}



		public RecoderSubsystem()
			: base()
		{
			m_LastTempCleaning = DateTime.Now;
			m_TasksInSystem = new HashSet<long>();
		}

		void m_TaskQueue_ReportErrorEvent(string message, Exception exc)
		{
			Log.TraceMessage(exc, message);
		}

		void m_TaskQueue_ShowTextEvent(string message, System.Diagnostics.TraceEventType level)
		{
			Log.TraceMessage(level, message);
		}

        public override void OnStart()
        {
            base.OnStart();

			KillAllRunningProcesses(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));

			m_SNode = (ISNodeSubsystem)LocalNode.GetSubsystem("SNode");
			if (m_SNode == null)
				throw new ApplicationException("Brak podsystemu SNode - inicjalizacja rekodera nie powiodła się.");

			m_SNode.OnTaskFinished += m_SNode_OnTaskFinished;


			string cnnString = this.LocalNode.GetGlobalParameter("RepoDBConnection");
			if (string.IsNullOrEmpty(cnnString))
				throw new ApplicationException("Zła konfiguracja - brak connection string dla bazy RepoDB.");
			m_RepoDBAccess = new RepDBAccess.RepDBAccess(cnnString, false);

			if (!string.IsNullOrEmpty(m_TaskSubtypes.Value))
			{
				string[] tts = m_TaskSubtypes.Value.Split(';');
				if (tts != null)
				{
					m_MyTaskTypes = new TaskTypeComplex[tts.Length];
					for (int i = 0; i < tts.Length; i++)
						m_MyTaskTypes[i] = new TaskTypeComplex() { Type = TaskType.Recode, TaskSubtype = tts[i] };
				}
			}

			if (m_MyTaskTypes == null)
				m_MyTaskTypes = new TaskTypeComplex[1] { new TaskTypeComplex() { Type = TaskType.Recode, TaskSubtype = null } };

			Log.TraceMessage(System.Diagnostics.TraceEventType.Information, GetName(), string.Format("Katalog temp: '{0}', katalog narzędzi: '{1}'. Obsługiwane typy zadań: {2}.",
																										TempFolder ?? "NULL",
																										ToolsFolder ?? "NULL",
																										(m_MyTaskTypes != null && m_MyTaskTypes.Length > 0) ? (string.Join(";", m_MyTaskTypes.Select(tt => string.Format("{0}:{1}", (short)tt.Type, tt.TaskSubtype ?? "")).ToArray())) : "NULL")
																									);

			m_TaskQueue = new TaskQueueManager(1000, 10000, (uint)m_ExecutingThreadsCount.Value, GetName(), this);
			m_TaskQueue.CheckIfRunningTasksAreLate = true;
			m_TaskQueue.MaxTimeOfWorkingThreadExecution = m_MaxTimeOfWorkingThreadExecution.Value;
			m_TaskQueue.QueueCheckingInterval = 2000;

			m_TaskQueue.ShowTextEvent += m_TaskQueue_ShowTextEvent;
			m_TaskQueue.ReportErrorEvent += m_TaskQueue_ReportErrorEvent;
			m_TaskQueue.TaskFinished += m_TaskQueue_TaskFinished;
			m_TaskQueue.Start();

        }

		void m_TaskQueue_TaskFinished(BaseTask task)
		{
			if (task is UniXmlTask)
			{
				lock (m_TasksInSystem)
					m_TasksInSystem.Remove(((UniXmlTask)task).RepoTaskId);
			}
		}

        public override void OnStop()
        {
			m_SNode.OnTaskFinished -= m_SNode_OnTaskFinished;

            base.OnStop();
        }

        public override void OnTimer(long tick)
        {
            base.OnTimer(tick);

			if (m_Checking4NewTasksInterval.Value > -1 && (DateTime.Now - m_LastChecking4NewTasksDate).TotalSeconds >= m_Checking4NewTasksInterval.Value)
			{
				Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, GetName(), "Okresowe pobranie zadań rekodowania do wykonania.");

				GetAndOrderTasks();
			}

			TimeSpan ts = DateTime.Now - m_LastTempCleaning;
			if (ts.TotalSeconds >= m_CleaningInterval && m_CleaningInterval > -1)
			{
				CleanTempFolderIfIdle();
			}

        }

		public void CleanTempFolderIfIdle()
		{
			Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, "Czyszczenie katalogu temporalnego.");

			string[] fis = Directory.GetFiles(TempFolder);
			if (fis != null)
			{
				foreach (string f in fis)
				{
					try
					{
						FileInfo fi = new FileInfo(f);
						if (fi.CreationTime < DateTime.Now.AddHours(-24))
							fi.Delete();
					}
					catch (Exception ex)
					{
						Log.TraceMessage(ex, string.Format("Błąd podczas usuwania pliku '{0}' w ramach czyszczenia katalogu temporalnego.", f));
					}
				}
			}
			
			m_LastTempCleaning = DateTime.Now;
		}

		private TaskDefinition[] ReadTasksDefinitionsFromXml(string xml)
		{
			if (string.IsNullOrEmpty(xml))
				return null;

			try
			{
				const string Namespace = "http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0";
				TaskCollection taskDefinitions = XmlValidationHelper.DeserializeXml<TaskCollection>(xml, "RecoderTaskDefinition.xsd", Namespace, Assembly.GetExecutingAssembly(), typeof(RecoderSubsystem).Namespace);
				if (taskDefinitions != null && taskDefinitions.Task.Length > 0)
					return taskDefinitions.Task;
			}
			catch (Exception iop)
			{
				Log.TraceMessage(iop, "Błąd podczas odczytu definicji zadań z XMLa '" + xml + "' .");
			}
			return null;
		}

		//public TaskResponse NewUniversalXmlTask(UniversalXmlTaskRequest request)
		//{
		//	if (request != null && request.TransactionID != null && request.TransactionID.Length > 0)
		//		TransactionDesc.RegisterInThread(request.TransactionID);
		//	else
		//		request.TransactionID = RegisterNewTransaction();

		//	TaskResponse resp = new TaskResponse();

		//	try
		//	{
		//		if (request == null)
		//		{
		//			resp.ErrorDesc = "Brak lub niepoprawny parametr wejściowy!";
		//			resp.ErrorCode = PSNC.Diagnostics.ErrorCode.InvalidEntryParameter;
		//			TraceMessage(TraceEventType.Error, "Zgłoszono uniwersalne żądanie obróbki pliku bez żadnego parametru wejściowego.");
		//			return resp;
		//		}

		//		if (request.InputPaths == null || request.InputPaths.Length < 1 || string.IsNullOrEmpty(request.InputPaths[0]) || string.IsNullOrEmpty(request.TaskDefinition))
		//		{
		//			resp.ErrorDesc = "Brak lub niepoprawny parametr wejściowy!";
		//			resp.ErrorCode = PSNC.Diagnostics.ErrorCode.InvalidEntryParameter;
		//			TraceMessage(TraceEventType.Error, string.Format("Zgłoszono uniwersalne żądanie obróbki pliku bez lub z niepoprawnym parametrem wejściowym. InputFile='{0}', TaskDefinition='{1}'.",
		//				request.InputPaths == null ? "NULL" : string.Join(";", request.InputPaths), request.TaskDefinition == null ? "NULL" : request.TaskDefinition));
		//			return resp;
		//		}
		//		TaskDefinition def = null;
		//		if (request.TaskDefinition.StartsWith("<"))
		//		{
		//			TaskDefinition[] defs = ReadTasksDefinitionsFromXml(request.TaskDefinition);
		//			if (defs != null && defs.Length > 0)
		//				def = defs[0];
		//		}
		//		else
		//			def = FindTaskByName(request.TaskDefinition);

		//		if (def == null)
		//		{
		//			resp.ErrorDesc = "Brak lub niepoprawny parametr wejściowy opisujący definicję zadania!";
		//			resp.ErrorCode = PSNC.Diagnostics.ErrorCode.InvalidEntryParameter;
		//			TraceMessage(TraceEventType.Error, "Zgłoszono uniwersalne żądanie bez lub z niepoprawnym parametrem wejściowym opisującym definicję zadania.");
		//			return resp;
		//		}

		//		PSNC.Diagnostics.TransactionDesc.SetDescriptionInThread(request.ID);



		//		TraceMessage(TraceEventType.Information, string.Format("Obsługa żądania wykonania uniwersalnego zadania rekodowania według definicji '{0}'. Ścieżka wejściowa: '{1}', wyjściowa: '{2}', czas rozpoczęcia wykonywania: {3}.",
		//															request.TaskDefinition,
		//															string.Join(";", request.InputPaths),
		//															request.OutputPath == null ? "NULL" : request.OutputPath,
		//															request.BeginDate.ToString("yyyy-MM-dd HH:mm:ss")));

		//		ErrorCode errorId;
		//		string errorDesc;
		//		UniXmlTask task = new UniXmlTask(request.InputPaths, request.OutputPath, request.Parameters, def, request.BeginDate, request.TransactionID,
		//											request.ParentTaskId, request.ParentNode, request.ID, this);
		//		task.Priority = request.Priority;

		//		task = (UniXmlTask)m_TaskQueue.AddTask(task, request, out errorDesc, out errorId);
		//		if (task == null)
		//		{
		//			resp.ErrorDesc = errorDesc;
		//			resp.ErrorCode = errorId;
		//			TraceMessage(TraceEventType.Error, resp.ErrorDesc);
		//		}
		//		else
		//		{
		//			resp.TaskID = task.ID;
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		resp.ErrorDesc = string.Format("Błąd podczas dodawania zadania uniwersalnego dla definicji '{0}' i pliku wejściowego '{1}'.", request.TaskDefinition, request.InputPaths == null ? "NULL" : string.Join(";", request.InputPaths));
		//		resp.ErrorCode = PSNC.Diagnostics.ErrorCode.Runtime_Error;
		//		TraceMessage(ex, resp.ErrorDesc);
		//	}
		//	return resp;
		//}

		//internal UniXmlTask AddUniversalXmlTask(string[] inputPaths, string outputPath, string[] parameters, TaskDefinition definition, long parentTaskId,
		//											string parentNode, out ErrorCode errorCode, out string errorMsg)
		//{
		//	UniXmlTask task = new UniXmlTask(inputPaths, outputPath, parameters, definition, DateTime.Now, TransactionDesc.GetTransactionIDFromCurrentThreadInfo(),
		//										parentTaskId, parentNode, TransactionDesc.GetDescriptionFromCurrentThreadInfo(), this);

		//	return ((UniXmlTask)m_TaskQueue.AddTask(task, null, out errorMsg, out errorCode));
		//}

		//[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		//public TaskResponse NewMultiInputOutputTask(MultiInputOutputTaskRequest request)
		//{
		//	if (request != null && request.TransactionID != null && request.TransactionID.Length > 0)
		//		TransactionDesc.RegisterInThread(request.TransactionID);
		//	else
		//		request.TransactionID = RegisterNewTransaction();

		//	TaskResponse resp = new TaskResponse();

		//	try
		//	{
		//		if (request == null)
		//		{
		//			resp.ErrorDesc = "Brak lub niepoprawny parametr wejściowy.";
		//			resp.ErrorCode = PSNC.Diagnostics.ErrorCode.InvalidEntryParameter;
		//			TraceMessage(TraceEventType.Error, "Zgłoszono żądanie wykonania zadania uniwersalnego dla listy wejść/wyjść bez żadnego parametru wejściowego.");
		//			return resp;
		//		}

		//		if (request.InOutDefs == null || request.InOutDefs.Length < 1 || Array.Exists(request.InOutDefs, io => io == null || string.IsNullOrEmpty(io.InputPath) || string.IsNullOrEmpty(io.OutputPath)))
		//		{
		//			resp.ErrorDesc = "Brak lub niepoprawny parametr wejściowy.";
		//			resp.ErrorCode = PSNC.Diagnostics.ErrorCode.InvalidEntryParameter;
		//			TraceMessage(TraceEventType.Error, string.Format("Zgłoszono żądanie wykonania uniwersalnego zadania dla wielu wejść/wyjść bez lub z niepoprawnym parametrem wejściowym. TaskDefinition='{0}'.", request.TaskDefinition == null ? "NULL" : request.TaskDefinition));
		//			return resp;
		//		}
		//		TaskDefinition def = null;
		//		if (request.TaskDefinition.StartsWith("<"))
		//		{
		//			TaskDefinition[] defs = ReadTasksDefinitionsFromXml(request.TaskDefinition);
		//			if (defs != null && defs.Length > 0)
		//				def = defs[0];
		//		}
		//		else
		//			def = FindTaskByName(request.TaskDefinition);

		//		if (def == null)
		//		{
		//			resp.ErrorDesc = "Brak lub niepoprawny parametr wejściowy opisujący definicję zadania.";
		//			resp.ErrorCode = PSNC.Diagnostics.ErrorCode.InvalidEntryParameter;
		//			TraceMessage(TraceEventType.Error, "Zgłoszono żądanie wykonania zadania uniwersalnego dla listy wejść/wyjść bez lub z niepoprawnym parametrem wejściowym opisującym definicję zadania.");
		//			return resp;
		//		}

		//		PSNC.Diagnostics.TransactionDesc.SetDescriptionInThread(request.ID);



		//		TraceMessage(TraceEventType.Information, string.Format("Obsługa żądanie wykonania zadania uniwersalnego dla listy wejść/wyjść według definicji '{0}'. We/wy: '{1}', czas rozpoczęcia wykonywania: {2}.",
		//															request.TaskDefinition,
		//															request.GetAllInputOuputFilePaths(),
		//															request.BeginDate.ToString("yyyy-MM-dd HH:mm:ss")));

		//		ErrorCode errorId;
		//		string errorDesc;
		//		MultiInputOutputTask task = new MultiInputOutputTask(request.InOutDefs, request.Parameters, def, request.BeginDate, request.TransactionID,
		//																request.ParentTaskId, request.ParentNode, request.ID, this);
		//		task.Priority = request.Priority;

		//		task = (MultiInputOutputTask)m_TaskQueue.AddTask(task, request, out errorDesc, out errorId);
		//		if (task == null)
		//		{
		//			resp.ErrorDesc = errorDesc;
		//			resp.ErrorCode = errorId;
		//			TraceMessage(TraceEventType.Error, resp.ErrorDesc);
		//		}
		//		else
		//		{
		//			resp.TaskID = task.ID;
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		resp.ErrorDesc = string.Format("Błąd podczas dodawania zadania uniwersalnego dla listy wejść/wyjść '{1}' i definicji '{0}'.", request.TaskDefinition, request.GetAllInputOuputFilePaths());
		//		resp.ErrorCode = PSNC.Diagnostics.ErrorCode.Runtime_Error;
		//		TraceMessage(ex, resp.ErrorDesc);
		//	}
		//	return resp;
		//}


		void m_SNode_OnTaskFinished(long taskId, PSNC.RepoAV.Common.ErrorType et, string errorDesc, string result, string[] addInfo, long repoTaskId, long recoderTaskId)
		{
			Log.TraceMessage(TraceEventType.Verbose, string.Format("Event zakończenie zadania. TaskId={0}, RepoTaskId={1}, RecoderTaskId={2}, ErrorType={3}.", taskId, repoTaskId, recoderTaskId, et));

			if (recoderTaskId > -1)
			{
				MessageBody mb = new MessageBody("ShiftStateMsg",
												recoderTaskId,
												taskId,
												(int)et,
												errorDesc,
												false,
												DateTime.MinValue,
												result,
												addInfo,
												null);

				m_TaskQueue.OnMessageForTask(mb);
			}
		}

		protected void GetAndOrderTasks()
		{
			try
			{
				if (m_RepoDBAccess == null)
					return;

				lock (this)
				{
					if (m_DuringTaskOrdering)
						return;

					m_DuringTaskOrdering = true;
				}
				m_LastChecking4NewTasksDate = DateTime.Now;

				Log.TraceMessage(System.Diagnostics.TraceEventType.Warning, string.Format("Okresowe sprawdzenie zadań w DB: NumberOfRunningTasks={0}, NumOfWorkingThreads={1}.", m_TaskQueue.NumberOfRunningTasks, m_TaskQueue.NumOfWorkingThreads));

				int numOfTasks2Get = (int)m_TaskQueue.NumOfWorkingThreads;//Math.Min((int)m_TaskQueue.NumOfWorkingThreads - m_TaskQueue.NumberOfRunningTasks, m_MaxNewTasksCount.Value);
				if (numOfTasks2Get < 1)
					return;

				TaskShort[] tss = RepoDBAccess.GetTasks2Execute(LocalNode.NodeIdAsInt, m_MyTaskTypes, numOfTasks2Get);

				if (tss != null && tss.Length > 0)
				{
					string sourceFormatIdsKey = RecodeKeywords.SourceFormatIds.ToString();
					string outputFormatIdsKey = RecodeKeywords.OutputFormatIds.ToString();
					string operationXMLKey = RecodeKeywords.OperationXML.ToString();
					string parameterCountKey = RecodeKeywords.ParameterCount.ToString();
					string downloadSourceFilesKey = RecodeKeywords.DownloadSourceFiles.ToString();
					
					string errorMsg;
					int errorId;

					foreach (TaskShort ts in tss)
					{
						lock (m_TasksInSystem)
						{
							if (m_TasksInSystem.Contains(ts.Id))
								continue;
						}
						BaseTask bt = null;

						string[] inputFormatUniqueIds = null;
						string[] outputFormatUniqueIds = null;
						string operationXML = string.Empty;
						int parCount = 0;
						string[] parameters = null;
						bool[] downloadSourceFiles = null;

						if (ts.Content != null)
						{
							if (ts.Content.ContainsKey(sourceFormatIdsKey))
								inputFormatUniqueIds = ts.Content[sourceFormatIdsKey].Split(';');

							if (ts.Content.ContainsKey(outputFormatIdsKey))
								outputFormatUniqueIds = ts.Content[outputFormatIdsKey].Split(';');

							if (ts.Content.ContainsKey(operationXMLKey))
								operationXML = ts.Content[operationXMLKey];

							if (ts.Content.ContainsKey(parameterCountKey))
								parCount = int.Parse(ts.Content[parameterCountKey]);

							if (ts.Content.ContainsKey(downloadSourceFilesKey))
								downloadSourceFiles = ts.Content[downloadSourceFilesKey].Split(';').Select<string, bool>(cont => bool.Parse(cont)).ToArray();


							if (parCount > 0)
							{
								parameters = new string[parCount];
								for (int i = 0; i < parCount; i++)
								{
									string key = "Parameter" + (i + 1).ToString();
									if (ts.Content.ContainsKey(key))
										parameters[i] = ts.Content[key];
									else
										parameters[i] = string.Empty;
								}
							}
						}

						TaskDefinition[] defs = ReadTasksDefinitionsFromXml(operationXML);

						bt = new UniXmlTask(ts.Id, inputFormatUniqueIds, outputFormatUniqueIds, parameters, defs[0], this, downloadSourceFiles);

						m_TaskQueue.AddTask(bt, out errorMsg, out errorId);


						if (errorId != (int)StandardErrors.Success)
							Log.TraceMessage(System.Diagnostics.TraceEventType.Warning, "Nie udało sie dodanie nowego zadania rekodowania dla zadania z Repo o Id=" + ts.Id.ToString());
						else
						{
							lock (m_TasksInSystem)
								m_TasksInSystem.Add(ts.Id);

							Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, string.Format("Dodano do kolejki zadanie o Id={1} dla zadania z Repo o Id={0}.", ts.Id, bt.ID));
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.TraceMessage(ex, "Błąd podczas pobierania nowych zadań do wykonania.");
			}
			finally
			{
				lock (this)
				{
					m_DuringTaskOrdering = false;
				}
			}
		}

		internal void KillProcessAndChildren(int pid)
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
			ManagementObjectCollection moc = searcher.Get();
			foreach (ManagementObject mo in moc)
			{
				KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
			}
			try
			{
				Process proc = Process.GetProcessById(pid);
				proc.Kill();
			}
			catch (ArgumentException)
			{
				// Process already exited.
			}
		}

		internal void AfterNewProcessStarted(int processId, string directoryPath)
		{
			try
			{
				string fullName = Path.Combine(directoryPath, string.Format("{0}.pid", processId));
				if (File.Exists(fullName))
					return;

				File.Create(fullName).Close();
			}
			catch(Exception ex)
			{
				Log.TraceMessage(ex, "Błąd podczas tworzenia pliku z identyfikatorem uruchomionego procesu.");
			}
		}

		internal void AfterProcessEnded(int processId, string directoryPath)
		{
			try
			{
				string fullName = Path.Combine(directoryPath, string.Format("{0}.pid", processId));
				if (File.Exists(fullName))
					File.Delete(fullName);
			}
			catch (Exception ex)
			{
				Log.TraceMessage(ex, "Błąd podczas usuwania pliku z identyfikatorem zakończonego procesu.");
			}
		}

		internal void KillAllRunningProcesses(string directoryPath)
		{
			try
			{
				string[] files = Directory.GetFiles(directoryPath, "*.pid");
				foreach(string fi in files)
				{
					try
					{
						int pid = int.Parse(Path.GetFileNameWithoutExtension(fi));
						KillProcessAndChildren(pid);
						File.Delete(fi);
					}
					catch
					{

					}
				}
			}
			catch (Exception ex)
			{
				Log.TraceMessage(ex, "Błąd podczas ubijania osieroconych procesów.");
			}
		}
    }
}
