using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml;
using System.Diagnostics;
using PSNC.Util;
using PSNC.Proca3.Subsystem;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.Manager
{
    partial class ManagerSubsystem : Subsystem, IManagerSubsystem
    {
        internal bool RepairFormats(FormatInternalStatus status, TaskType taskType, out string msg)
        {
            msg = string.Empty;
            int step = 10;
            FormatSelector4IntStat selector = new FormatSelector4IntStat() { Offset = 0, Count = step, FormatInternalStatus = status };

            try
            {
                Format[] formats = m_repDbAccess.GetFormats4WithInternalStatus(selector);
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano formaty w stanie {0} w liczbie {1}", status, formats.Length));
                while (formats.Length > 0)
                {
                    foreach (Format format in formats)
                    {
                        Task[] tasks = m_repDbAccess.GetTasksOfType4Format(taskType, format.UniqueId);
                        
                        Task task = tasks.Where(t => t.CreatedDate == tasks.Max(ts => ts.CreatedDate)).FirstOrDefault();
                        if (task == null || format.CreatedDate > task.CreatedDate) // wrong task
                            continue;
                        
                        if(!task.Content.ContainsKey("Repeated") && task.Status != TaskStatus.New && task.Status != TaskStatus.Executing)
                            {
                                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Ponowne zlecenie zadania typu {0} dla formatu {1}", taskType, format.UniqueId));
                                TaskAdd newTask = new TaskAdd()
                                {
                                    UniqueId = task.UniqueId,
                                    Type = task.Type,
                                    TaskSubtype = task.TaskSubtype,
                                    PreferredNodeIds = task.PreferredNodeIds,
                                    CanSkipPreferredNodes = task.CanSkipPreferredNodes
                                };
                                foreach (var key in task.Content.Keys)
                                    newTask.Content.Add(key, task.Content[key]);
                                newTask.Content.Add("Repeated", task.Id.ToString());
                                if (!AddTaskDB(newTask, out msg))
                                    return false;
                            }
                    }

                    if (formats.Length < step)
                        break;
                    selector.Offset += step;
                    formats = m_repDbAccess.GetFormats4WithInternalStatus(selector);
                }
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania formatów w stanie {1} z db: {0}", ex.ToString(), status));
                msg = ex.Message;
                return false;
            }
            return true;
        }

        public void FixErrors(Task task)
        {
            string msg = string.Empty;
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Rozpoczęcie zadania {0} typu FixError", task != null ? task.Id : -1));
            bool success = RepairFormats(FormatInternalStatus.AddError, TaskType.Download, out msg)
                && RepairFormats(FormatInternalStatus.RecError, TaskType.Recode, out msg)
                && RepairFormats(FormatInternalStatus.RemovePending, TaskType.Remove, out msg);
            
            if (task == null)
                return;

            if (success)
                FinishTask(task);
            else
                FinishTaskWithError(task, msg);

            if (m_formatRepairInterval > 0)
            {
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zlecenie kolejnego wykonania zdania FixError o {0}",
                    (DateTime.Now.AddMinutes(m_formatRepairInterval).ToShortTimeString())));
                TaskAdd taskAdd = new TaskAdd() { Type = TaskType.FixFormatErrors, BeginDate = DateTime.Now.AddMinutes(m_formatRepairInterval) };
                AddTaskDB(taskAdd, out msg);
            }
        }

        internal void FixReplication(Task task)
        {
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Rozpoczęcie zadania{0} typu FixReplication", task != null ? task.Id : -1));
            string msg = string.Empty;
            bool success = true;
            int minReplicaCount = ReplicaCount(true);
            if (minReplicaCount < 0)
                return;
            int targetReplicaCount = ReplicaCount(false);
            if (targetReplicaCount < -1)
                return;
            if (GetSNodes().Length > minReplicaCount)
            {
                int step = 10;
                // get list of formats with number of location lower than n with status Added
                NumOfCopiesSelector selector = new NumOfCopiesSelector() { MinNumOfLocations = targetReplicaCount, Count = step, Offset = 0 };

                try
                {
                    Format[] formats = m_repDbAccess.GetFormatsWithoutNumOfLocations(selector);
                    while (formats.Length > 0)
                    {
                        Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano formaty w liczbie {0} o liczbie kopii mniejszej niż {1}", formats.Length, selector.MinNumOfLocations));
                        foreach (Format format in formats)
                        {
                            if (format.InternalStatus != FormatInternalStatus.Added)
                                continue;
                            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Zlecenie zadania replikacji formatu {0}", format.UniqueId));
                            if (ReplicateFormat(format) == null)
                            {
                                success = false;
                                break;
                            }
                        }
                        selector.Offset += step;
                        formats = m_repDbAccess.GetFormatsWithoutNumOfLocations(selector);
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                    success = false;
                }
            }

            if (task == null)
                return;

            if (success)
                FinishTask(task);
            else
                FinishTaskWithError(task, msg);

            if (m_replicaRepairInterval > 0)
            {
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zlecenie kolejnego wykonania zdania FixReplication o {0}",
                    (DateTime.Now.AddMinutes(m_replicaRepairInterval).ToShortTimeString())));
                TaskAdd taskAdd = new TaskAdd() { Type = TaskType.FixReplication, BeginDate = DateTime.Now.AddMinutes(m_replicaRepairInterval) };
                AddTaskDB(taskAdd, out msg);
            }
        }

        internal bool  RemoveOldMaterials(Task task)
        {
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Rozpoczęcie zadania {0} typu RemoveOldMaterials", task != null ? task.Id : -1));

            int step = 20;
            DBAccess.RangeSelector selector = new DBAccess.RangeSelector() { Offset = 0, Count = step };
            bool success = true;
            string msg = string.Empty;
            List<string> publicIds = new List<string>();
            try
            {
                Material[] materials = m_repDbAccess.GetMaterials2Remove(selector);               
                while (materials.Length > 0)
                {
                    publicIds.AddRange(materials.Select(m => m.PublicId));                  

                    if (materials.Length < step)
                        break;
                    selector.Offset += step;
                    materials = m_repDbAccess.GetMaterials2Remove(selector);
                }             
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania materiałów do usunięcia z db: {0}", ex.ToString()));
                msg = ex.Message;
                success = false;
            }

            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano materiały do usunięcia w liczbie {0}", publicIds.Count));
            foreach (string publicId in publicIds)
                if (!RemoveMaterial(publicId))
                    success = false;
           
            if (success)
                FinishTask(task);
            else
                FinishTaskWithError(task, msg);

            if (m_oldMaterialRemovalInterval > 0)
            {
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zlecenie kolejnego wykonania zdania RemoveOldMaterials {0}",
                    (DateTime.Now.AddHours(m_oldMaterialRemovalInterval))));
                TaskAdd taskAdd = new TaskAdd() { Type = TaskType.RemoveOldMaterials, BeginDate = DateTime.Now.AddHours(m_oldMaterialRemovalInterval) };
                AddTaskDB(taskAdd, out msg);
            }
            return true;
        }

        internal void  RemoveOldTasks(Task task)
        {
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Rozpoczęcie zadania {0} typu RemoveOldTasks", task != null ? task.Id : -1));
            try
            {
                m_repDbAccess.RemoveOldTasks(false);
            }
            catch (DBAccess.DBAccessException ex)
            {
               Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania materiałów do usunięcia z db: {0}", ex.ToString()));
               FinishTaskWithError(task, ex.Message);
            }

            FinishTask(task);
            string  msg;
            TaskAdd taskAdd = new TaskAdd() { Type = TaskType.RemoveOldTasks, BeginDate = DateTime.Now.AddDays(1)};
            AddTaskDB(taskAdd, out msg);
        }        


        public void ProcessTasks()
        {
            lock (this)
            {
                if (m_DuringTaskOrdering)
                    return;
                m_DuringTaskOrdering = true;
            }
            m_LastChecking4NewTasksDate = DateTime.Now;

            TaskShort[] tasks = null;
            try
            {
                tasks = m_repDbAccess.GetManagerTasks2Execute(m_myId);
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Pobrano zadania dla managera w liczbie : {0}", tasks.Length));
                foreach (TaskShort stask in tasks)
                {
                    RepDBAccess.Task task = m_repDbAccess.GetTask(stask.Id);
                    Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przetwarzanie zadania {0} typu {1}", task.Id, task.Type.ToString()));
                    switch (task.Type)
                    {
                        case TaskType.AddMaterial:
                            AddMaterial(task);
                            break;
                        case TaskType.UpdateMaterial:
                            UpdateMaterial(task);
                            break;
                        case TaskType.RemoveMaterial:
                            RemoveMaterial(task);
                            break;
                        case TaskType.AddFormat:
                            AddFormat(task);
                            break;
                        case TaskType.UpdateFormat:
                            UpdateFormat(task);
                            break;
                        case TaskType.RemoveFormat:
                            RemoveFormat(task);
                            break;
                        case  TaskType.FixFormatErrors:
                            FixErrors(task);
                            break;
                        case TaskType.FixReplication:
                            FixReplication(task);
                            break;
                        case TaskType.RemoveOldMaterials:
                            RemoveOldMaterials(task);
                            break;
                        case TaskType.RemoveOldTasks:
                            RemoveOldTasks(task);
                            break;
                    }
                }

                tasks = m_repDbAccess.GetManagerTasks4ResultProcessing(m_myId);
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Pobrano zadania do przetworzenia rezultatów w liczbie : {0}", tasks.Length));
                foreach (TaskShort stask in tasks)
                {
                    RepDBAccess.Task task = m_repDbAccess.GetTask(stask.Id);
                    Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przetwarzanie wyników zadania {0} typu {1}", task.Id, task.Type.ToString()));
                    switch (stask.Type)
                    {
                        case TaskType.Download:
                            ProcessDownloadResult(task);
                            break;
                        case TaskType.Recode:
                            ProcessRecodeResult(task);
                            break;
                        case TaskType.Remove:
                            ProcessRemoveResult(task);
                            break;
                    }
                }
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania zadań dla managera z db : {0}", ex.ToString()));
                return;
            }
            finally
            {
                lock (this)
                {
                    m_DuringTaskOrdering = false;
                }
            }
        }
    }
}

