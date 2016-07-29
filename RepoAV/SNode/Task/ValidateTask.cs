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
using System.IO;
using System.Threading;

namespace PSNC.RepoAV.SNode
{
	public class ValidateTask : BaseDemanTask
	{
		static object ValidatingMonitor = new object();
		static bool DuringValidating = false;

		public ValidateTask(long repoTaskId)
			: base(repoTaskId)
		{
			Priority = 12.0;
		}

		protected override void GetDetailsAfterFinished(StringBuilder sb)
		{
			if (sb == null)
				return;

			base.GetDetailsAfterFinished(sb);
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

		private void GetAllFilesInsideDir(DirectoryInfo di, bool withSubDirectories, List<FileInfo> files)
		{
			if (di == null || files == null)
				return;

			try
			{
				FileSystemInfo[] entries = di.GetFileSystemInfos();
				if (entries != null)
				{
					foreach (FileSystemInfo fsi in entries)
					{
						if (fsi is DirectoryInfo)
							GetAllFilesInsideDir((DirectoryInfo)fsi, withSubDirectories, files);
						else
							files.Add((FileInfo)fsi);
					}
				}
			}
			catch (Exception ex)
			{
				Manager.ReportError(string.Format("Błąd podczas przeglądania katalogu '{0}'!", di.Name), ex);
			}
		}

		protected override void DoSpecificAction()
		{

			lock(ValidatingMonitor)
			{
				if (DuringValidating)
					FinishDemanTask((int)ErrorType.OperationAborted, "Zlecono wykonanie uspójniania podcza, gdy inne tego typu zadanie było już wykonywane.");

				DuringValidating = true;
			}

			if (m_RepoTaskId > -1)
				DemanSubsys.RepoDBAccess.UpdateTaskLastActivityDate(m_RepoTaskId);

			StringBuilder sb = new StringBuilder();

			string repPath = DemanSubsys.Repository.Path;

			string tmp;
			List<FileInfo> files = new List<FileInfo>();

			FileInfo file;
			try
			{

				DirectoryInfo di = new DirectoryInfo(repPath);
				if (!di.Exists)
				{
					sb.AppendFormat("Nie istnieje katalog '{0}' podany jako repozytorium.", repPath);

					Directory.CreateDirectory(repPath);

					State = TaskState.WaitingForFinish;
					return;
				}

				GetAllFilesInsideDir(di, true, files);

				FormatMetadata[] formats = DBAccess.GetAllFormats();//te sa w Repozytoriach
				if (formats == null)
				{
					FinishDemanTask((int)ErrorType.MaterialFormatDBError, "Nie powiodło się pozyskanie listy materiałów znajdujących się w repozytorium z bazy danych,");
					return;
				}

				List<FormatMetadata> lstMaterials = new List<FormatMetadata>();
				List<FileInfo> filesToRemove = new List<FileInfo>();
				for (int idx = 0; idx < formats.Length; idx++)
				{

					FormatMetadata mi = formats[idx];
					string fullMaterialLocation = System.IO.Path.Combine(DemanSubsys.Repository.Path, mi.Location);

					file = null;
					for (int i = 0; i < files.Count; i++)
					{
						if (files[i].FullName.Equals(fullMaterialLocation, StringComparison.OrdinalIgnoreCase))
						{
							file = files[i];
							files.RemoveAt(i);
							break;
						}
					}
					bool remMaterial;
					ValidateMaterial(mi, out remMaterial, sb, lstMaterials);
				}

				foreach (FileInfo fi in files)
				{
					try
					{
						if (!DBAccess.IsFileUsedByAnyMaterial(fi.FullName))
						{
							tmp = string.Format("Usunięcie pliku '{0}' bez wpisu w DB.", fi.FullName);
							Manager.ShowText(tmp, TraceEventType.Warning);
							sb.AppendFormat("\r\n  {0}", tmp);

							fi.Delete();
						}
					}
					catch (Exception ex)
					{
						tmp = string.Format("Nie udało sie usunąć pliku '{0}' bez wybranego podczas uspójniania.", fi.FullName);
						Manager.ReportError(tmp, ex);
						sb.AppendFormat("\r\n  {0}", tmp);
					}
				}
				AchieveTheDesiredLoad(sb);

			}
			catch (Exception ex)
			{
				tmp = "Błąd podczas uspójniania repozytorium.";
				Manager.ReportError(tmp, ex);
				sb.AppendFormat("\r\n  {0}", tmp);
			}
			finally
			{
				lock (ValidatingMonitor)
				{
					DuringValidating = false;
				}
			}
			m_AddInfo.Add(sb.ToString());
			State = TaskState.WaitingForFinish;
		}

		internal bool IsAnyTaskForContentInProgress(string uniqueId)
		{
			BaseTask[] tasks = Manager.GetAllTasksInQueue(false, false);
			if (tasks != null)
			{
				foreach (BaseTask bt in tasks)
				{
					if (bt is InsertFormatTask)
					{
						if (((InsertFormatTask)bt).UniqueIds.Contains(uniqueId))
							return true;
					}
				}
			}
			return false;
		}

		void ValidateMaterial(FormatMetadata mi, out bool materialWasRemoved, StringBuilder sb, List<FormatMetadata> lstcompleteMaterials)
		{
			string tmp;
			materialWasRemoved = false;
			try
			{
				ErrorType ec;
				string errorDesc;

				string fullLocation = System.IO.Path.Combine(DemanSubsys.Repository.Path, mi.Location);

				FileInfo file = null;

				if (!string.IsNullOrEmpty(fullLocation))
				{
					file = new FileInfo(fullLocation);
					if (!file.Exists)
					{
						mi = DBAccess.GetFormat(mi.UniqueId);//ponowne pobranie z DB
						if (mi == null)
						{
							materialWasRemoved = true;
							return;
						}
						fullLocation = System.IO.Path.Combine(DemanSubsys.Repository.Path, mi.Location);
						file = new FileInfo(fullLocation);
					}
				}


				if (mi.Status == MaterialFormatDBAccess.FormatStatus.Full)
				{
					if (!file.Exists)
					{
						sb.AppendFormat("\r\nUWAGA! Ze względu na brak pliku '{1}' wybrano do usunięcia materiał '{0}'.", mi.UniqueId, fullLocation);
						materialWasRemoved = true;
					}
					else if (file.Length != mi.Size)
					{
						if (DBAccess.SetFormatSize(mi.UniqueId, file.Length, DemanSubsys.Repository.CalculateRealFileSize(file.Length)))
						{
							sb.AppendFormat("\r\nRozmiar pliku '{1}' inny niż wpis w tabeli - poprawiono dane w DB dla formatu '{0}'.", mi.UniqueId, fullLocation);
							lstcompleteMaterials.Add(mi);
						}
						else
						{
							sb.AppendFormat("\r\nRozmiar pliku '{1}' inny niż wpis w tabeli, a nie udało się zmienić wielkości w DB - wybrano do usunięcia materiał '{0}', w stanie 'Completed'.", mi.UniqueId, fullLocation);
							materialWasRemoved = true;
						}
					}
					else
						lstcompleteMaterials.Add(mi);
				}
				else if (mi.Status == MaterialFormatDBAccess.FormatStatus.Partial)
				{
					if (!IsAnyTaskForContentInProgress(mi.UniqueId))
					//juz sie zakonczyl transfer
					{
						mi = DBAccess.GetFormat(mi.UniqueId);//ponowne pobranie z DB
						if (mi == null)
						{
							materialWasRemoved = true;
							return;
						}

						if (mi.Status == MaterialFormatDBAccess.FormatStatus.Partial)
						{
							if (!file.Exists || file.Length != mi.Size)
							{
								sb.AppendFormat("\r\nPlik '{1}' niekompletny - wybrano do usunięcia materiał '{0}'.", mi.UniqueId, fullLocation);
								materialWasRemoved = true;
							}
							else //jest caly - zmieniamy stan na Completed
							{
								if (DBAccess.SetFormatStatus(mi.UniqueId, MaterialFormatDBAccess.FormatStatus.Full))
								{
									mi.Status = MaterialFormatDBAccess.FormatStatus.Full;
									lstcompleteMaterials.Add(mi);
								}
								else
								{
									sb.AppendFormat("\r\nNie udało się zmienić stanu materiału '{0}' na kompletny - będzie usunięty.", mi.UniqueId);
									materialWasRemoved = true;
								}
							}
						}
					}
				}
				else if (mi.Status == MaterialFormatDBAccess.FormatStatus.Removed)
				{
					sb.AppendFormat("\r\nMateriał '{0}' ma stan 'Removed' - nastąpi usunięcie.", mi.UniqueId);
					materialWasRemoved = true;
				}

				if (materialWasRemoved)
				{
					if (!DemanSubsys.RemoveFormat(mi.UniqueId, false, out errorDesc))
					{
						sb.AppendFormat("\r\nWalidacja - wystąpił problem podczas usuwania materiału o ID='{0}': {1}.", mi.UniqueId, errorDesc ?? "");

						if (CodeOfError == (int)ErrorType.Success)
						{
							CodeOfError = (int)ErrorType.FileDeleteFailed;
							ErrorDesc = errorDesc;
						}
					}
				}
			}
			catch (Exception ex)
			{
				tmp = string.Format("Błąd podczas uspójniania materiału o ID={0}.", mi.UniqueId);
				Manager.ReportError(tmp, ex);
				sb.Append(tmp);
			}
		}

		private void AchieveTheDesiredLoad(StringBuilder sb)//wywalic nadmiar towaru
		{
			if (DemanSubsys.Repository.Size < 1)
				return;

			Manager.ShowText("@@@@@@@@   AchieveTheDesiredLoad   @@@@@@@@@@@@@@@@", TraceEventType.Verbose);

			long curLoadB = DBAccess.GetTotalLoad();//Bytes
			long repoSizeB = ((long)DemanSubsys.Repository.Size) * ((long)(1024 * 1024)); //w B

			long wantedMaxLoad = repoSizeB;//w B
			if (DemanSubsys.Repository.MinFreeSpace > 0)
				wantedMaxLoad = repoSizeB - (((long)DemanSubsys.Repository.MinFreeSpace * repoSizeB) / 100);//w B

			Manager.ShowText(string.Format("Obecne obciążenie repozytorium = {0} B, a maksymalne pożądane wynosi {1} B. (MinFreeSpace={2} %, Size={3} B )", curLoadB, wantedMaxLoad, DemanSubsys.Repository.MinFreeSpace, repoSizeB), TraceEventType.Warning);

			if (wantedMaxLoad >= curLoadB)
				return;

			short numOfDays = 0;
			if (DemanSubsys.IgnoreRemoveFormatsOlderThanDays == false)
				numOfDays = DemanSubsys.RemoveFormatsOlderThanDays;

			Format2Remove[] formats = DBAccess.GetFormatsPossibleToRemove(numOfDays);

			if (formats != null)
				Manager.ShowText(string.Format("Lista formatów potencjalnie możliwych do usunięcia zawiera {0} wpisów.", formats.Length), TraceEventType.Warning);
			else
				return;

			if (formats.Length == 0)
				return;

			List<string> materialsToRemove = new List<string>();
			long spacePlannedToFree = 0;
			int i = 0;
			sb.AppendFormat("\r\nObecne obciążenie repozytorium = {0} MB, a maksymalne pożądane wynosi {1} MB.", curLoadB / (1024 * 1024), wantedMaxLoad / (1024 * 1024));

			while (wantedMaxLoad < (curLoadB - spacePlannedToFree))//trzeba cos usunac
			{
				spacePlannedToFree += DemanSubsys.Repository.CalculateRealFileSize(formats[i].Size); // w B
				materialsToRemove.Add(formats[i].UniqueId);
				sb.AppendFormat("\r\nWybrano do usunięcia materiał '{0}', w lokalizacji '{2}' i wielkości {1} MB.", formats[i].UniqueId, formats[i].Size / (1024 * 1024), formats[i].Location);
				i++;
				if (i >= formats.Length)
					break;
			}


			if (materialsToRemove.Count > 0)
			{
				string errorDesc;
				foreach (string ui in materialsToRemove)
				{
					if (!DemanSubsys.RemoveFormat(ui, false, out errorDesc))
					{
						sb.AppendFormat("\r\nOczyszczanie repozytorium - wystąpił problem podczas usuwania materiału o ID='{0}': {1}.", ui, errorDesc ?? "");

						if (CodeOfError == (int)ErrorType.Success)
						{
							CodeOfError = (int)ErrorType.FileDeleteFailed;
							ErrorDesc = errorDesc;
						}
					}
				}
			}
		}
	}
}
