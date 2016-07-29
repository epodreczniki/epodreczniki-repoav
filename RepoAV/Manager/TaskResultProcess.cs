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
using PSNC.RepoAV.Common;
using PSNC.RepoAV.RepDBAccess;

namespace PSNC.RepoAV.Manager
{
    partial class ManagerSubsystem : Subsystem, IManagerSubsystem
    {
        void ProcessDownloadResult(Task task)
        {
            string msg;
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przetwarzanie rezultatów zadania {0} typu Download", task.Id));
            Format format = GetFormat(task.UniqueId, out msg);
            if (format == null)
            {
				Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania formatu o id='{0}' z db przy przetwarzaniu rezultatów Download", task.UniqueId));
                return;
            }
             if(format.Id == -1)
             {
                 Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Brak w db formatu o id='{0}' przy przetwarzaniu rezultatów Download o id='{1}'", task.UniqueId, task.Id));
                 m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                 return;
             }

            if (task.Status == TaskStatus.Success)
				Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Format o id='{0}' został umieszczony w repozytorium snode'a {1}", task.UniqueId, task.ExecutingNodeId));
            else
				Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Format o id='{0}' nie został umieszczony w repozytorium snode'a {1}", task.UniqueId, task.ExecutingNodeId));

            if (task.Content.ContainsKey(DownloadKeywords.FormatURL.ToString())) // Download - Import - replication needed
                if (task.Status != TaskStatus.Success)
                {
                    if(task.Result.StartsWith(((int)ErrorType.IncompatibleState).ToString()))
                        SetFormatInternalStatus(format, FormatInternalStatus.InvalidFile, out msg);
                    else
                        if (task.Result.Contains("The remote server returned an error: (404) Not Found."))
                            SetFormatInternalStatus(format, FormatInternalStatus.NotFound, out msg);
                        else
                            SetFormatInternalStatus(format, FormatInternalStatus.AddError, out msg);

                    Format[] srcFormats = GetFormats4Group(format.FormatGroupId, out msg);
                    if (srcFormats.Count(f => f.InternalStatus == FormatInternalStatus.Adding) == 0) // end of material processing
                        SendNotification(format);
                    m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                    return;
                }
                else
                {
                    bool? res = ReplicateFormat(format);
                    if (res == null)
                        return;
                    if (!res.Value) // need to wait
                    {
                        m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                        return;
                    }
                }
            else // Download - Replication
            {
                bool? result = ExistsTask(task.UniqueId, task.Type, out msg, task.Id);
                if(!result.HasValue)
                    return;
                if(result.Value)               
                {
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("ProcessDownloadResult: dla formatu '{0}' trwają jeszcze inne zadania typu Download", task.UniqueId));
                    m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                    return;
                }

                int[] locations = GetFormatLocations(format.UniqueId);
                if (locations == null)
                    return;
                
                int minReplicaCount = ReplicaCount(true);
                if (minReplicaCount < 0)
                    return;
                
                if (locations != null && locations.Length < minReplicaCount)
                {
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Liczba kopii formatu '{0}' jest poniżej wymaganego min={1}",
                        task.UniqueId, minReplicaCount));
                    SetFormatInternalStatus(format, FormatInternalStatus.AddError, out msg);
                    CheckMaterialDone(format);                        

                    m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                    return;
                }
                
                if(format.Type == FormatType.Recoded)
                    CleanRecoderFiles(format, task);
                    
                if (!SetFormatInternalStatus(format, FormatInternalStatus.Added, out msg) || !SetFormatStatus(format, FormatStatus.Ready, out msg))
                    return;
            }

            // check if material should be recoded 
            if (format.Type != FormatType.Recoded) // source format
            {
                Format[] srcFormats = GetFormats4Group(format.FormatGroupId, out msg);
                if (srcFormats == null)
                    return;

                if (srcFormats.Count(f => f.InternalStatus != FormatInternalStatus.Added) == 0)
                {
                    Material material = GetMaterial4Format(format, out msg);
                    if (material == null)
                        return;
                    bool? result = RecodeMaterial(material, out msg);
                    if (result.HasValue && result.Value) // done
                        SendNotification(material.PublicId);
                }
                else
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Nie można zlecić rekodowania materiału ze względu na stan formatów źródłowych"));
            }
            m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
        }

        //return false on error, true otherwise
        bool CleanRecoderFiles(Format format, Task task)
        {
            FormatLocation[] allLocations = GetAllFormatLocations(format.UniqueId);
            var tempLocation = allLocations.FirstOrDefault(l => l.Role == NodeRole.Recoder);
            string msg = string.Empty;
            bool? result = ExistsTask(format.UniqueId, TaskType.Remove, out msg);
            if(!result.HasValue)
                return false;
            if (tempLocation != null && !result.Value)
            {
                TaskAdd newTask = new TaskAdd() { Type = TaskType.Remove, UniqueId = format.UniqueId, PreferredNodeIds = new int[] { tempLocation.NodeId }, CanSkipPreferredNodes = false };
                newTask.Content.Add(RemoveKeywords.FormatId.ToString(), format.Id.ToString());
                if (AddTaskDB(newTask, out msg))
                    Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zlecono zadanie {2} typu Remove dla formatu '{0}' node'owi {1}", format.UniqueId,
                        tempLocation.NodeId, newTask.Id));
                else
                    return false;
            }
            return true;
        }

        void ProcessRecodeResult(Task task)
        {
            string msg;
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przetwarzanie rezultatów zadania {0} typu Recode", task.Id));

            if (task.Status == TaskStatus.Success)
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zadanie rekodowania formatów '{0}' zakończone powodzeniem", task.Content[RecodeKeywords.OutputFormatIds.ToString()]));
            else
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zadanie rekodowania formatów '{0}' zakończone błędem", task.Content[RecodeKeywords.OutputFormatIds.ToString()]));

        
            string[] uniqueIds = task.Content[RecodeKeywords.OutputFormatIds.ToString()].Split(';');
            List<Format> formats = new List<Format>();
            foreach (string uniqueId in uniqueIds)
            {
                Format format = GetFormat(uniqueId, out msg);
                if (format == null)
                    return;
                if (format.Id == -1)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Brak w db formatu {0} dla zadania rekodowania {1}", format.UniqueId, task.Id));
                    m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                    return;
                }
                formats.Add(format);
            }

            if (task.Status == TaskStatus.Success)
            {
                bool needNotWait = true;
                foreach (Format format in formats)
                {
                    if (!SetFormatInternalStatus(format, FormatInternalStatus.Recoded, out msg))
                        return;
                    format.InternalStatus = FormatInternalStatus.Recoded;

                    bool? result = ReplicateFormat(format);
                    if (result == null)
                        return;
                    needNotWait &= result.Value;                    
                }
                if (needNotWait)
                    CheckMaterialDone(formats[0]);
                m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                return;
            }
            else
            {
                bool success = true;
                foreach (Format format in formats)
                    success &= SetFormatInternalStatus(format, FormatInternalStatus.RecError, out msg);
                if (success)
                    m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
            }
        }

        void CheckMaterialDone(Format format)
        {
            string msg;
            Material material = GetMaterial4Format(format, out msg);
            if(material == null)
                return;
            Format[] allFormats = m_repDbAccess.GetFormats4Material(material.PublicId);
            if (allFormats.Count(f => f.InternalStatus != FormatInternalStatus.Added) == 0)
                SendNotification(material.PublicId);            
        }

        void ProcessRemoveResult(Task task)
        {
            string msg;
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przetwarzanie rezultatów zadania {0} typu Remove", task.Id));

            if (task.Status == TaskStatus.Failure)
            {
                Log.TraceMessage(TraceEventType.Warning, GetName(), string.Format("Format '{0}' nie został usunięty z node'a {1}", task.UniqueId ?? "NULL", task.ExecutingNodeId ?? -1));
                m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                return;
            }

            Format format = null;
            format = GetFormat(task.UniqueId, out msg);
            if (format == null || format.Id != Convert.ToInt16(task.Content[RemoveKeywords.FormatId.ToString()])) // no format in db - done
            {
                m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                return;
            }

            // remove format from db if there is no copy of the format in repo
            FormatLocation[] locations = GetAllFormatLocations(format.UniqueId);
            if (locations == null)
                return;
            if (locations.Length > 0)
            {
                m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                return;
            }

            // get material - must be done before format is removed from db
            Material material = GetMaterial4Format(format, out msg);
            if (material == null)
                return;

            bool? result = RemoveFormatDB(format.UniqueId, out msg);
            if (!result.HasValue || !result.Value)
                return;


            // check if task was submitted due to formatUpdate operation
            /* TaskShort updateTask = m_repDbAccess.GetTasksOfType(TaskType.UpdateFormat).FirstOrDefault(t => t.UniqueId == task.UniqueId && t.Status == TaskStatus.Executing);
             if (updateTask != null)
             {
                 Task addTask = m_repDbAccess.GetTask(updateTask.Id);
                 FinishTask(addTask); // finish update 
                 AddFormat(addTask);
                 Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Zlecono dodanie formatu '{0}' w trybie update", task.UniqueId));
             }
             else
             {*/
            // remove format group if it does not have any other format
            Format[] formats = GetFormats4Group(format.FormatGroupId, out msg);
            if (formats == null)
                return;
            if (formats.Length > 0)
            {
                m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
                return;
            }

            if (!RemoveFormatGroupDB(format.FormatGroupId, out msg))
            {
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Powrót po nieudanym usunięciu grupy formatów"));
                return;
            }
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Powrót po usunięciu grupy formatów"));

            // remove material if it does not have any other format groups 
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobranie grup formatów dla materiału {0}", material.PublicId));
            FormatGroup[] formatsInGroup = m_repDbAccess.GetFormatGroups4Material(material.PublicId);
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano groupy formatów dla materiału '{0}' w liczbie {1}", material.PublicId, formatsInGroup.Length));
            if (formatsInGroup.Length == 0)
            {
                result = RemoveMaterialDB(material.Id, out msg);

                if (result == null) // error on checking material
                    return;

                if (result.Value) // material existed and was successfully removed 
                {
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Materiał '{0}' został usunięty z db", material.PublicId));
                    SendNotification(material.PublicId);

                    // continue updateMaterial task if there is one
                    /*    updateTask = m_repDbAccess.GetTasksOfType(TaskType.UpdateMaterial).FirstOrDefault(t => t.PublicId == material.PublicId && t.Status == TaskStatus.Executing);
                        if (updateTask != null)
                        {
                            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Zlecono dodanie materiału '{0}' w trybie update", material.PublicId));
                            Task addTask = m_repDbAccess.GetTask(updateTask.Id);
                            FinishTask(addTask); // finish update 
                            AddMaterial(addTask); // start add
                        }*/
                }
            }
            m_repDbAccess.SetTaskResultProcessedFlag(task.Id, true);
        }        
    }
}
