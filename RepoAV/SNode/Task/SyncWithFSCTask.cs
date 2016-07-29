using PSNC.RepoAV.Common;
using PSNC.RepoAV.MaterialFormatDBAccess;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.TaskQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.SNode
{
	public class SyncWithFSCTask : BaseDemanTask
	{
		protected List<FormatData4Sync> m_ContainedFormats;

		public SyncWithFSCTask(long repoTaskId)
			: base(repoTaskId)
		{
			m_ContainedFormats = null;
			Priority = 11.0;
		}

		protected override void GetDetailsAfterFinished(StringBuilder sb)
		{
			if (sb == null)
				return;

			base.GetDetailsAfterFinished(sb);
			if (m_ContainedFormats != null)
				sb.AppendFormat(", ContainedFormatsCount={0}\r\n  NotInRep={1}\r\n  AddInRep={2}", m_ContainedFormats.Count,
									m_AddInfo.Count > 0 ? m_AddInfo[0] : "", m_AddInfo.Count > 1 ? m_AddInfo[1] : "");
		}

		protected override bool ShouldAskingTaskWaitForMe(BaseTask askingTask)
		{
			try
			{
				if (m_ID != askingTask.ID
					&& (State == TaskState.Running || State == TaskState.WaitingForAnswer)
					&& (askingTask is SyncWithFSCTask || askingTask is ValidateTask))
				{
					return true;
				}
			}
			catch (Exception exc)
			{
				Manager.ReportError("Błąd podczas sprawdzania, czy zadanie : " + ID.ToString() + " powinno czekać na zadanie : " + askingTask.ID.ToString(), exc);
			}
			return false;
		}

		protected override bool CanBeRunningLonger(double runningTime)
		{
			if (runningTime > DemanSubsys.MaxTime4SyncWithFSCTaskExecution)
				return false;
			return true;
		}

		protected override void DoSpecificAction()
		{
			if (m_RepoTaskId > -1)
				DemanSubsys.RepoDBAccess.UpdateTaskLastActivityDate(m_RepoTaskId);

			FormatSelector4Sync fs = new FormatSelector4Sync();
			fs.Count = 1000;
			fs.Offset = 0;
			fs.Id_Node = DemanSubsys.LocalNode.NodeIdAsInt;
            fs.Total = 1; // to start

			m_ContainedFormats = new List<FormatData4Sync>();
			while(fs.Total > m_ContainedFormats.Count)
			{
				FormatData4Sync[] data = RepoDBAccess.GetFormats4Sync(fs);

				if (data == null)
					break;

				fs.Offset += data.Length;
				m_ContainedFormats.AddRange(data);
			}

			FormatMetadata[] formats = DBAccess.GetAllFormats();//te sa w Repozytoriach
			if (formats == null)
			{
				FinishDemanTask((int)ErrorType.MaterialFormatDBError, "Nie powiodło się pozyskanie listy materiałów znajdujących się w repozytoriach z bazy danych,");
				return;
			}

			StringBuilder sb1 = new StringBuilder();
			sb1.AppendFormat("Z FSC pobrano listę materiałów w liczbie {0}.\r\n", m_ContainedFormats.Count);
			sb1.AppendFormat("Lista materiałów w repozytoriach ma rozmiar {0}.\r\n", formats.Length);
			Manager.ShowText(sb1.ToString(), System.Diagnostics.TraceEventType.Verbose);

			long freeSpace;

			Dictionary<string, FormatMetadata> dictMaterials = formats.ToDictionary((mi) => { return mi.UniqueId; });
			//po przejsciu petli zostana tu te formaty, ktore sa w Repozytorium, a nie ma o tym wpisu w FSC

			List<string> notInRepFormatIDs = new List<string>();//jest wpis w FSC, a nie ma tego w Repozytorium
			foreach (var fmsi in m_ContainedFormats)//te wpisy sa w FSC
			{
				if (fmsi == null)
					continue;

				FormatMetadata miLocal = null;

				dictMaterials.TryGetValue(fmsi.UniqueId, out miLocal);

				if (miLocal == null) // nie ma tego w zadnym Repozytorium
				{
					freeSpace = DemanSubsys.Repository.GetRepositoryFreeSpace();//w MB
					RepoDBAccess.RemoveFormatLocation(fmsi.UniqueId, DemanSubsys.LocalNode.NodeIdAsInt, freeSpace);
					notInRepFormatIDs.Add(fmsi.UniqueId);
					continue;
				}
				dictMaterials.Remove(fmsi.UniqueId);

				if (miLocal.Size != fmsi.Size && fmsi.Size > 0)//było wołane zadanie zmiany metadanych, a ten węzeł chyba nie działał - trzeba usunąć prawdopodobnie zmieniony plik
				{
					string fullPath = DemanSubsys.Repository.GetFormatFullLocation(miLocal);
					System.IO.FileInfo fi = new System.IO.FileInfo(fullPath);
					bool remove = true;

					Manager.ShowText(string.Format("UWAGA! Rozmiar formatu w MaterialFormatDB ({2} B) inny niż wpis w FSC ({3} B) dla UniqueId='{0}' i File='{1}'.", miLocal.UniqueId, fullPath, miLocal.Size, fmsi.Size), TraceEventType.Warning);

					if (fmsi.Size == fi.Length)//w MaterialFormatDB jest zła wielkośc pliku
					{
						if (DBAccess.SetFormatSize(miLocal.UniqueId, fmsi.Size, DemanSubsys.Repository.CalculateRealFileSize(fmsi.Size)))
						{
							Manager.ShowText(string.Format("\r\nRozmiar pliku '{1}' inny niż wpis w tabeli - poprawiono dane w DB dla formatu '{0}'.", miLocal.UniqueId, fullPath), TraceEventType.Information);
							remove = false;
						}
						else
							Manager.ShowText(string.Format("\r\nRozmiar pliku '{1}' inny niż wpis w tabeli, a nie udało się zmienić wielkości w DB - format '{0}' zostanie usunięty.", miLocal.UniqueId, fullPath), TraceEventType.Information);
					}
					if (remove)
					{
						DBAccess.RemoveFormat(miLocal.UniqueId);

						try
						{
							System.IO.File.Delete(fullPath);

							Result = Result + miLocal.UniqueId + ";";
						}
						catch
						{
						}
						freeSpace = DemanSubsys.Repository.GetRepositoryFreeSpace();//w MB
						RepoDBAccess.RemoveFormatLocation(miLocal.UniqueId, DemanSubsys.LocalNode.NodeIdAsInt, freeSpace);

						Manager.ShowText(string.Format("Pomyślnie usunięto format o ID={0} z repozytorium, ze względu na zmianę formatu (pliku) - synchronizacja z FSC.", fmsi.UniqueId), TraceEventType.Warning);
					}
				}

				if (fmsi.Status == RepDBAccess.FormatStatus.New)
				{//uwazany jest za niekompletny
					if (miLocal.Status == PSNC.RepoAV.MaterialFormatDBAccess.FormatStatus.Full) // a w lokalnej bazie jest caly
					{
						RepoDBAccess.SetFormatStatus(miLocal.UniqueId, fmsi.Status, RepDBAccess.FormatStatus.Ready);
					}
				}

				if (fmsi.AllowDistribution != miLocal.AllowDistribution)
				{
					if (!DBAccess.SetFormatAllowDistribution(fmsi.UniqueId, fmsi.AllowDistribution))
					{
						miLocal.AllowDistribution = fmsi.AllowDistribution;
						Manager.ShowText(string.Format("Uspójnono flagę AllowDistribution dla formatu o ID={0} - nowa wartość to {1}. [synchronizacja z FSC]", fmsi.UniqueId, fmsi.AllowDistribution), TraceEventType.Information);
					}
					else
						Manager.ShowText(string.Format("Nie powiodło się uspójnienie flagi AllowDistribution dla formatu o ID={0}. [synchronizacja z FSC]", fmsi.UniqueId), TraceEventType.Warning);
				}
			}

			m_AddInfo.Add(string.Join(";", notInRepFormatIDs.ToArray()));//ID, ktorych nie w Rep, a FSC mysli, ze sa
			Manager.ShowText(string.Format("Lista materiałów, których nie ma w repozytorium dyskowym, a są lokalizacje w FSC: {0}", string.Join(",", notInRepFormatIDs.ToArray())), TraceEventType.Verbose);

			bool removeFormatFromRepo;
			freeSpace = DemanSubsys.Repository.GetRepositoryFreeSpace();//w MB
			StringBuilder sb = new StringBuilder();
			foreach (var add in dictMaterials)
			{
				RepoDBAccess.AddFormatLocationIfMetadataExists(add.Key, DemanSubsys.LocalNode.NodeIdAsInt, freeSpace, out removeFormatFromRepo);

				if (removeFormatFromRepo == true)
				{
					DBAccess.RemoveFormat(add.Key);

					try
					{
						string fullPath = DemanSubsys.Repository.GetFormatFullLocation(add.Value);
						System.IO.File.Delete(fullPath);

						Result = Result + add.Key + ";";
					}
					catch
					{
					}
					Manager.ShowText(string.Format("Pomyślnie usunięto z repozytorium format o ID={0}, którego metadanych nie ma już w RepoDB - synchronizacja z FSC.", add.Key), TraceEventType.Warning);
				}
				else
					sb.AppendFormat("{0};", add.Key);
			}
			if (sb.Length > 0)
				sb.Remove(sb.Length - 1, 1);
			m_AddInfo.Add(sb.ToString());//ID, ktore sa w Rep, a FSC nic o tym nie wie
			if (sb.Length > 0)
				Manager.ShowText(string.Format("LISTA których nie ma w FSC, a są w Rep: {0}", sb), TraceEventType.Warning);

			State = TaskState.WaitingForFinish;
		}
	}
}
