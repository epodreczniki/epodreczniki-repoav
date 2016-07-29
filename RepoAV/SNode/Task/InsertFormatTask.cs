using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.MaterialFormatDBAccess;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.Common;
using PSNC.RepoAV.TaskQueue;
using System.Net;

namespace PSNC.RepoAV.SNode
{
	public class InsertFormatTask : BaseDemanTask
	{
		protected FormatMetadata m_MaterialMD;
		protected DateTime m_DateOfFirstAttemptToFindMetadata;//data pierwszej próby pozyskania meta-danych technicznych z GCD.	
		protected bool m_OverwriteIfExists;//gdy FALSE i material juz jest, to zadanie zaraz sie konczy sukcesem
		protected WebClient m_WC;
		protected string m_SourceLocation;
		protected string m_DestinationLocation;
		protected bool m_PreserveFileName;//gdy TRUE, to plik musi sie nazywać tak, jakie jest UniqueId formatu, inaczej zadanie skończy sie z błędem
		protected bool m_IsPrimeInsert;
		protected string m_PublicId;
		protected string m_Mime;
		protected string m_FileExtension;
		protected string m_FileHash;
		protected string m_DownloadUrl;
		protected float m_LastCheckProgress = 0;
		protected long m_RecoderTaskId;

		public FormatMetadata MaterialMetadata
		{
			get { return m_MaterialMD; }
		}
		public long RecoderTaskId
		{
			get { return m_RecoderTaskId; }
		}

		public InsertFormatTask(long repoTaskId, string uniqueId, string publicId, string mime, string fileExt, string fileHash, string downloadUrl, long recoderTaskId, bool preserveFileName = false, bool overrideIfExists = false)
			: base(repoTaskId)
		{
			m_PreserveFileName = preserveFileName;
			CurrentExecState = TransferState.Init;
			Priority = 4.0;
			m_DateOfFirstAttemptToFindMetadata = DateTime.MinValue;
			m_OverwriteIfExists = overrideIfExists;
			m_UniqueIds = new string[] { uniqueId };
			m_IsPrimeInsert = false;
			m_PublicId = publicId;
			m_Mime = mime;
			m_FileExtension = fileExt;
			m_FileHash = fileHash;
			m_DownloadUrl = downloadUrl;
			m_FinishAfterLastChild = false;
			m_RecoderTaskId = recoderTaskId;
		}

		protected override bool ShouldAskingTaskWaitForMe(BaseTask askingTask)
		{
			if (askingTask is InsertFormatTask)
			{
				InsertFormatTask ift = askingTask as InsertFormatTask;

				if (ift.UniqueIds != null && m_UniqueIds != null)
				{
					if (ift.UniqueIds.Intersect(m_UniqueIds).ToArray().Length > 0)
						return true;
				}
			}
			else if (askingTask is RemoveFormatTask)
			{
				RemoveFormatTask ift = askingTask as RemoveFormatTask;

				if (ift.UniqueIds != null && m_UniqueIds != null)
				{
					if (ift.UniqueIds.Intersect(m_UniqueIds).ToArray().Length > 0)
						return true;
				}
			}

			return base.ShouldAskingTaskWaitForMe(askingTask);
		}

		protected override void GetDetailsAfterFinished(StringBuilder sb)
		//przygotowanie opisu zadania po jego zakonczeniu
		{
			if (sb == null)
				return;

			base.GetDetailsAfterFinished(sb);
			sb.AppendFormat("\r\n    Override={0},PreserveFileName={1},SourceUrl={2},PrimeInsert={3};RecoderTaskId={4}", m_OverwriteIfExists, m_PreserveFileName, m_SourceLocation ?? "NULL", m_IsPrimeInsert, m_RecoderTaskId);
		}

		protected override bool CanBeRunningLonger(double runningTime)
		{
			if (Progress > m_LastCheckProgress)
			{
				m_LastCheckProgress = Progress;
				return true;
			}

			m_LastCheckProgress = Progress;

			if (runningTime > DemanSubsys.InsertMaterialTaskRunningTimeout)
				return false;
			else
				return true;
		}

		protected override bool CanBeWaitingLonger(double totalWaitingSeconds, int maxPeriodSeconds)
		{
			if (Progress > m_LastCheckProgress)
			{
				m_LastCheckProgress = Progress;
				return true;
			}

			m_LastCheckProgress = Progress;

			if (totalWaitingSeconds > DemanSubsys.InsertMaterialTaskRunningTimeout)
				return false;
			else
				return true;
		}

		protected override void DoSpecificAction()
		{
			bool wasMaterialAdded = false;
			try
			{
				if (m_RepoTaskId > -1)
					DemanSubsys.RepoDBAccess.UpdateTaskLastActivityDate(m_RepoTaskId);

				//Manager.ShowText(string.Format("###### DoSpecificAction dla Id={0} i formatu o UId={1}'.", ID, m_UniqueIds[0] ?? "NULL"), System.Diagnostics.TraceEventType.Verbose);

				if (CurrentExecState == TransferState.Init)
				{
					string repoDir = DemanSubsys.Repository.Path;

					Manager.ShowText(string.Format("Obsługa zadania (nr w Repo {3}) wstawienia formatu o UId={0} do repozytorium [Overwrite={1},PreserveFileName={2}.", m_UniqueIds[0], m_OverwriteIfExists, m_PreserveFileName, m_RepoTaskId), System.Diagnostics.TraceEventType.Information);

					m_MaterialMD = DBAccess.GetFormat(m_UniqueIds[0]);
					if (m_MaterialMD != null)//juz jest w DB
					{
						if (m_OverwriteIfExists == false)
						{
							FinishDemanTask((int)StandardErrors.Success, "Format znajduje się już w repozytorium - nie ma konieczności ponownego dostarczenia.");

							return;
						}
						else
						{
							string fullMaterialLocation = System.IO.Path.Combine(repoDir, m_MaterialMD.Location);
							Manager.ShowText(string.Format("Usunięcie istniejącego formatu o ID={0} - zostanie ponownie pobrany (Override).", m_UniqueIds[0]), System.Diagnostics.TraceEventType.Information);
							DBAccess.RemoveFormat(m_MaterialMD.Id);
							m_MaterialMD = null;
							try
							{
								System.IO.File.Delete(fullMaterialLocation);
							}
							catch
							{

							}
						}
					}

					if (m_MaterialMD == null)
					{
						Format format = RepoDBAccess.GetFormat(m_UniqueIds[0]);
						if (format == null)// nie ma metadanych w głownej DB
						{
							FinishDemanTask((int)ErrorType.FormatNotFound, "Nie można pozyskać metadanych formatu o UId=" + m_UniqueIds[0] + " .");
							return;
						}
						m_MaterialMD = DemanSubsys.TranslateMetadata(format, m_Mime);
					}

					if (m_MaterialMD.Size > 0)
					{
						long realFileSize = DemanSubsys.Repository.CalculateRealFileSize(m_MaterialMD.Size);

						long freeSpace = DemanSubsys.Repository.GetRepositoryFreeSpace() * (1024L * 1024L); //w B

						if (freeSpace < realFileSize)
						{
							FinishDemanTask((int)ErrorType.NotEnoughFreeSpace, "Zbyt mało wolnej przestrzeni w repozytorium.");
							return;
						}
						m_MaterialMD.RealSize = realFileSize;
					}

					if (string.IsNullOrEmpty(m_FileExtension))
						m_FileExtension = DemanSubsys.GetExtension4MimeType(m_MaterialMD.Mime);

					string subLocation;
					m_DestinationLocation = DemanSubsys.Repository.BuildLocationForNewMaterial(m_MaterialMD.UniqueId, m_FileExtension, m_PreserveFileName, out subLocation);
					//tu potrzebna jest tylko nazwa pliku z rozszerzeniem. faktyczna lokalizacja nie jest ważna

					if (string.IsNullOrEmpty(m_DestinationLocation))
					{
						FinishDemanTask((int)ErrorType.AlreadyExists, string.Format("Istnieje już plik z domyślną nazwą '{1}' dla formatu o Id={0}.", m_MaterialMD.UniqueId, m_MaterialMD.Location));
						return;
					}
					m_MaterialMD.Status = MaterialFormatDBAccess.FormatStatus.Partial;
					m_MaterialMD.Location = subLocation;

					DBAccess.AddFormat(m_MaterialMD);
					wasMaterialAdded = true;

					bool localFileMove = false;
					m_SourceLocation = GetFormatSourceUrl(out localFileMove);

					if (localFileMove)
					{
						System.IO.File.Move(m_SourceLocation, m_DestinationLocation);

						CurrentExecState = TransferState.Completed;
					}
					else
					{
						if (string.IsNullOrEmpty(m_SourceLocation))
						{
							FinishDemanTask((int)ErrorType.InvalidParameter, string.Format("Przekazano pusty łańcuch jako adres źródłowy dla formatu o Id={0}.", m_MaterialMD.UniqueId));
							return;
						}
						m_WC = new WebClient();
						m_WC.DownloadFileCompleted += m_WC_DownloadFileCompleted;
						m_WC.DownloadProgressChanged += m_WC_DownloadProgressChanged;

						CurrentExecState = TransferState.Completed;
						State = TaskState.WaitingForAnswer;
						m_WC.DownloadFileAsync(new Uri(m_SourceLocation), m_DestinationLocation);


						Manager.ShowText(string.Format("Rozpoczęto transfer pliku formatu o UId={0} w ścieżce '{1}'.", m_UniqueIds[0], m_DestinationLocation ?? "NULL"), System.Diagnostics.TraceEventType.Verbose);
						return;
					}
				}




				if (CurrentExecState == TransferState.Completed)
				{
					Manager.ShowText(string.Format("Faza końcowa obsługi zadania (nr w Repo {3}) wstawienia formatu o UId={0} do repozytorium [Overwrite={1},PreserveFileName={2}.", m_UniqueIds[0], m_OverwriteIfExists, m_PreserveFileName, m_RepoTaskId), System.Diagnostics.TraceEventType.Verbose);

					if (m_ErrorCode == (int)ErrorType.Success)
					{
						System.IO.FileInfo fi = new System.IO.FileInfo(m_DestinationLocation);
						long realFileSize = DemanSubsys.Repository.CalculateRealFileSize(fi.Length);

						if (!string.IsNullOrEmpty(m_FileHash) && fi.Exists)
						{
							string calcHash = DemanSubsys.GetMD5Hashtable(new System.IO.FileStream(m_DestinationLocation, System.IO.FileMode.Open, System.IO.FileAccess.Read));

							if (m_FileHash != calcHash)
							{
								DemanSubsys.DBAccess.RemoveFormat(m_MaterialMD.UniqueId);

								FinishTaskWithError((int)ErrorType.TransmittingFailed, string.Format("Plik pobrany dla formatu o UniqueId={0} ma niepoprawny hash code - wystąpiły błędy transferu.", m_UniqueIds[0]), System.Diagnostics.TraceEventType.Warning);
								return;
							}
						}

						string xmlMetadata = null;
						ulong duration = 0;
						string mime = m_Mime;
						if (m_IsPrimeInsert == true)
						{
							xmlMetadata = DemanSubsys.GetFormatXmlForFile(m_PublicId, m_DestinationLocation, "PUBLIC", out duration, out mime);

							if (string.IsNullOrEmpty(xmlMetadata))
							{
								DemanSubsys.DBAccess.RemoveFormat(m_MaterialMD.UniqueId);
								FinishTaskWithError((int)ErrorType.IncompatibleState, string.Format("Plik pobrany dla formatu o UniqueId={0} nie został rozpoznany przez system.", m_UniqueIds[0]));
								return;
							}

							if (string.IsNullOrEmpty(m_Mime))
							{
								m_Mime = mime;
								m_FileExtension = DemanSubsys.GetExtension4MimeType(mime);
								string newPath = System.IO.Path.ChangeExtension(m_DestinationLocation, m_FileExtension);

								try
								{
									int suff = 1;
									while (System.IO.File.Exists(newPath))
										newPath = newPath.Substring(0, newPath.Length - m_FileExtension.Length) + "_" + (suff++).ToString() + m_FileExtension;

									System.IO.File.Move(m_DestinationLocation, newPath);
									m_DestinationLocation = newPath;
								}
								catch (Exception ex)
								{
									DemanSubsys.DBAccess.RemoveFormat(m_MaterialMD.UniqueId);
									FinishTaskWithError((int)ErrorType.FileMoveFailed, string.Format("Wystąpił błąd przy nadawaniu plikowi poprawnego rozszerzenia po transferze (UniqueId={0}, File={1}).", m_UniqueIds[0], m_DestinationLocation), System.Diagnostics.TraceEventType.Warning);
									return;
								}
							}
							else if (!string.IsNullOrEmpty(mime))
							{
								//if (m_Mime != mime)
								if (!DemanSubsys.MimeAreEquivalent(m_Mime, mime))
								{
									DemanSubsys.DBAccess.RemoveFormat(m_MaterialMD.UniqueId);
									FinishTaskWithError((int)ErrorType.IncompatibleState, string.Format("Plik pobrany dla formatu o UniqueId={0} ma inny {1} niż zadeklarowany {2} typ MIME.", m_UniqueIds[0], mime, m_Mime), System.Diagnostics.TraceEventType.Warning);
									return;
								}
							}

							int? dur = null;
							if (duration > 0)
								dur = (int)duration;


							Manager.ShowText(string.Format("Wołanie 'SetFormatMetadataExt' z parametrami: UniqueId={0}, xmlMetadata={1}, duration={2}, Size={3}, Mime={4}.", m_UniqueIds[0], xmlMetadata ?? "NULL", dur.HasValue ? dur.Value.ToString() : "NULL", fi.Length, m_Mime), System.Diagnostics.TraceEventType.Information);
							bool resss = RepoDBAccess.SetFormatMetadataExt(m_MaterialMD.UniqueId, xmlMetadata, dur, fi.Length, m_Mime);
							Manager.ShowText(string.Format("Zawołano 'SetFormatMetadataExt' z wynikiem {0}.", resss), System.Diagnostics.TraceEventType.Information);
						}

						try
						{
							//Manager.ShowText(string.Format("Próba zmiany stanu materiału po zakończeniu transferu (UniqueId={0}).", m_UniqueIds[0]), System.Diagnostics.TraceEventType.Verbose);
							DBAccess.UpdateFormat(m_MaterialMD.UniqueId, fi.Length, realFileSize, MaterialFormatDBAccess.FormatStatus.Full, mime, m_DestinationLocation.Substring(DemanSubsys.Repository.Path.Length + 1));
							Manager.ShowText(string.Format("Po zakończeniu transferu zmieniono stan na 'Full' i wpisano rozmiar dla formatu o UniqueId={0}.", m_UniqueIds[0]), System.Diagnostics.TraceEventType.Verbose);
						}
						catch (Exception ei)
						{
							Manager.ReportError(string.Format("Błąd podczas komunikacji z lokalną bazą MaterialFormatDB po poprawnym zakończeniu transferu dla zadania o Id={1}. [UniqueId={0}].", m_UniqueIds[0], m_RepoTaskId), ei);

							m_ErrorCode = (int)ErrorType.MaterialFormatDBError;
							m_ErrorDesc = string.Format("Błąd podczas komunikacji z lokalną bazą MaterialFormatDB po poprawnym zakończeniu transferu dla zadania o Id={1}. [UniqueId={0}].", m_UniqueIds[0], m_RepoTaskId);

							DemanSubsys.DBAccess.RemoveFormat(m_MaterialMD.UniqueId);
							try
							{
								if (System.IO.File.Exists(m_DestinationLocation))
									System.IO.File.Delete(m_DestinationLocation);
							}
							catch
							{

							}
						}

						try
						{
							long freeSpace = DemanSubsys.Repository.GetRepositoryFreeSpace();
							Manager.ShowText(string.Format("Wołanie AddFormatLocation (UniqueId={0}).", m_UniqueIds[0]), System.Diagnostics.TraceEventType.Verbose);
							RepoDBAccess.AddFormatLocation(m_MaterialMD.UniqueId, DemanSubsys.LocalNode.NodeIdAsInt, freeSpace);
						}
						catch (Exception ei)
						{
							Manager.ReportError(string.Format("Błąd podczas komunikacji z bazą RepoDB po poprawnym zakończeniu transferu dla zadania o Id={1}. [UniqueId={0}].", m_UniqueIds[0], m_RepoTaskId), ei);

							m_ErrorCode = (int)ErrorType.RepoDBError;
							m_ErrorDesc = string.Format("Błąd podczas komunikacji z bazą RepoDB po poprawnym zakończeniu transferu dla zadania o Id={1}. [UniqueId={0}].", m_UniqueIds[0], m_RepoTaskId);

							State = TaskState.WaitingForFinish;
							return;
						}
					}
					else//transfer zakonczony z bledem
					{
						Manager.ShowText(string.Format("Ze względu na zakończenie transferu z błędem materiał zostanie usunięty (UniqueId={0}).", m_UniqueIds[0]), System.Diagnostics.TraceEventType.Warning);

						DemanSubsys.DBAccess.RemoveFormat(m_MaterialMD.UniqueId);
						try
						{
							if (System.IO.File.Exists(m_DestinationLocation))
								System.IO.File.Delete(m_DestinationLocation);
						}
						catch
						{

						}

						
					}
					State = TaskState.WaitingForFinish;
				}
			}
			catch (Exception ex)
			{
				Manager.ReportError(string.Format("Niespodziewany błąd podczas wykonanaia zadania o Id={1}. [UniqueId={0}].", m_UniqueIds[0], m_RepoTaskId), ex);

				if (wasMaterialAdded)
					DBAccess.RemoveFormat(m_MaterialMD.UniqueId);

				try
				{
					if (System.IO.File.Exists(m_DestinationLocation))
						System.IO.File.Delete(m_DestinationLocation);
				}
				catch
				{

				}
				FinishTaskWithError(ex);
			}
		}

		void m_WC_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			m_Progress = (float)e.ProgressPercentage;
		}

		void m_WC_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			//Manager.ShowText(string.Format("###### Event DownloadFileCompleted dla zadania o Id={0} i formatu o UId={1}'.", ID, m_UniqueIds[0] ?? "NULL"), System.Diagnostics.TraceEventType.Verbose);

			CurrentExecState = TransferState.Completed;
			TimeSpan ts = DateTime.Now - m_HandlingStarted;
			if (e != null && e.Error != null)
			{
				Manager.ShowText(string.Format("Transfer pliku formatu o UniqueId={1} ze źródła '{2}' po czasie {0} sekund przy {4} % zaciągniętych zakończył się błędem: {3}.",
												ts.TotalSeconds,
												m_UniqueIds[0] ?? "NULL",
												m_SourceLocation ?? "NULL",
												e.Error.ToString(),
												m_Progress),
								 System.Diagnostics.TraceEventType.Warning);

				m_ErrorCode = (int)ErrorType.TransmittingFailed;
				m_ErrorDesc = "Nie powiodło się przetransferowanie pliku: " + e.Error.Message;
			}
			else
			{
				if (e != null && e.Cancelled == true)
					m_ErrorCode = (int)ErrorType.OperationAborted;
				else
					m_ErrorCode = (int)ErrorType.Success;

				Manager.ShowText(string.Format("Zakończono transfer pliku formatu o UniqueId={1} ze źródła '{3}' w czasie {0} sekund z wynikiem {2} [EC={4}, Progres={5} %].",
												ts.TotalSeconds,
												m_UniqueIds[0] ?? "NULL",
												((e != null && e.Cancelled == true) ? "Cancelled" : "Ok"),
												m_SourceLocation ?? "NULL",
												m_ErrorCode,
												m_Progress),
								 System.Diagnostics.TraceEventType.Verbose);

			}
			Manager.ShowText(string.Format("Próba dokończenia zadania po zakończeniu transfewru formatu o UId={0} w ścieżce '{1}'.", m_UniqueIds[0], m_DestinationLocation ?? "NULL"), System.Diagnostics.TraceEventType.Verbose);
			TryToRun();
			//State = TaskState.Waiting;
		}

		protected string GetFormatSourceUrl(out bool localFileMove)
		{
			localFileMove = false;
			m_IsPrimeInsert = false;

			if (!string.IsNullOrEmpty(m_DownloadUrl))
			{
				Manager.ShowText(string.Format("Zadanie InsertMaterial przy zleceniu otrzymało adres źródłowy dla formatu o UniqueId={0} w postaci: {1}.", m_UniqueIds[0], m_DownloadUrl ?? "NULL"), System.Diagnostics.TraceEventType.Verbose);

				if (m_DownloadUrl.StartsWith("file://"))
				{
					m_DownloadUrl = m_DownloadUrl.Substring(7);
					localFileMove = true;
				}
				else
					m_IsPrimeInsert = true;
				return m_DownloadUrl;
			}

			FormatSource fs = RepoDBAccess.GetSourceUrl4Format(m_UniqueIds[0]);
			if (fs != null && !string.IsNullOrEmpty(fs.SourceNodeIP))
			{
				Manager.ShowText(string.Format("Baza RepoDB oddała adres źródłowy dla formatu o UniqueId={0} w postaci: Url={1}, NodeId={2}, NodeIp={3}.", m_UniqueIds[0], fs.SourceUrl ?? "NULL", fs.SourceNodeId, fs.SourceNodeIP ?? "NULL"), System.Diagnostics.TraceEventType.Verbose);
				if (string.IsNullOrEmpty(fs.SourceUrl))
				{
					string val = string.Format(@"http://{0}/RepositoryAccess/{1}", fs.SourceNodeIP, m_UniqueIds[0]);

					Manager.ShowText(string.Format("Zbudowano domyślny adres źródła posiadającego format o UniqueId={0} w postaci: Url={1}.", m_UniqueIds[0], val ?? "NULL"), System.Diagnostics.TraceEventType.Verbose);

					return val;
				}
				else
					return fs.SourceUrl;

			}
			return null;
		}

		protected override void BeforeFinish()
		{			
			if (m_WC != null)
			{
				m_WC.DownloadFileCompleted -= m_WC_DownloadFileCompleted;
				m_WC.DownloadProgressChanged -= m_WC_DownloadProgressChanged;
			}
			base.BeforeFinish();
		}


	}

}
