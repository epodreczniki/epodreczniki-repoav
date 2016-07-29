using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml;
using System.Diagnostics;
using PSNC.Proca3.Subsystem;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.Common;
using PSNC.Util;

namespace PSNC.RepoAV.Manager
{
    partial class ManagerSubsystem : Subsystem, IManagerSubsystem
    {
        bool AddTaskDB(TaskAdd task, out string msg, bool existsOK = false)
        {
            msg = string.Empty;
            bool result = true;
            try
            {
                m_repDbAccess.AddTask(task);
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dodano zadanie typu {0} o id '{1}'", task.Type, task.Id));
            }
            catch (DBAccess.DBAccessException ex)
            {
                if (ex.Error != ErrorType.AlreadyExists || !existsOK)
                {
                    result = false;
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd dodania zadania typu {0}: {1}", task.Type, ex.ToString()));
                }
                msg = ex.Message;
            }
            return result;    
        }

        bool AddFormatGroupDB(FormatGroup formatGroup, string publicId, out string msg)
        {
            msg = string.Empty;
            try
            {
                m_repDbAccess.AddFormatGroup(formatGroup);
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Dodano grupę formatów z audioIndex={0}, subtitle={1} dla materiału '{2}'",
                    formatGroup.AudioId, formatGroup.SubtitleId, publicId));
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd dodania grupy formatów audioIndex={0}, subtitle={1} dla materiału '{2}' :{3}",
                    formatGroup.AudioId, formatGroup.SubtitleId, publicId, ex.Message));
                msg = ex.Message;
                return false;
            }
            return true;
        }

        bool AddFormatDB(Format format,out string msg)
        {
            msg = string.Empty;
            try
            {
                m_repDbAccess.AddFormat(format);
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Dodano format '{0}' do grupy '{1}' dla profilu '{2}' ", format.UniqueId, format.FormatGroupId, format.ProfileId));
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd dodania formatu '{0}' typu {3} do grupy '{1}' dla profilu '{2}': {4} ", format.UniqueId, format.FormatGroupId,
                    format.ProfileId, format.Type, ex.Message));
                msg = ex.Message;
                return false;
            }
            return true;
        }

        bool AddMaterialDB(Material material, out string msg)
        {
            msg = string.Empty;
            try
            {
                m_repDbAccess.AddMaterial(material);
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dodano materiał '{0}' do db", material.PublicId));
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd dodania materiału '{0}': {1}", material.PublicId, ex.ToString()));
                msg = ex.Message;
                return false;
            }
            return true;
        }

        Format[] GetFormats4Group(int formatGroupId, out string msg)
        {
            msg = string.Empty;
            Format[] formats;
            try
            {
                formats = m_repDbAccess.GetFormats4Group(formatGroupId);
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano formaty dla grupy '{0}' w liczbie {1}", formatGroupId, formats.Length));
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania formatów dla grupy {0} : {1}", formatGroupId, ex.ToString()));
                msg = ex.Message;
                return null;
            }
            return formats;
        }

        Material GetMaterial(string publicId, out string msg)
        {
            msg = string.Empty;
            Material material = new Material();
            try
            {
                material = m_repDbAccess.GetMaterial(publicId);
            }
            catch (DBAccess.DBAccessException ex)
            {
                if (ex.Error != ErrorType.NotFound)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania materiału o id = '{0}' z db: {1}", publicId, ex.ToString()));
                    msg = ex.Message;
                    return null;
                }
            }
            return material;
        }

        Material GetMaterial(int materialId)
        {
            Material material = new Material();
            try
            {
                material = m_repDbAccess.GetMaterial(materialId);
            }
            catch (DBAccess.DBAccessException ex)
            {
                if (ex.Error != ErrorType.NotFound)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania materiału o id = '{0}' z db: {1}", materialId, ex.ToString()));
                    return null;
                }
            }
            return material;
        }

        MaterialStatus? GetMaterialStatus(string publicId, out bool notFound)
        {
            MaterialStatus? status = null;
            notFound = false;
            try
            {
                status = m_repDbAccess.GetMaterialStatus(publicId);
            }
            catch (DBAccess.DBAccessException ex)
            {
                if (ex.Error != ErrorType.NotFound)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania stanu materiału o id = '{0}' z db: {1}", publicId, ex.ToString()));
                    return null;
                }
                notFound = true;
            }
            return status;
        }

        FormatGroup[] GetFormatGroups(string publicId, out string msg)
        {
            msg = string.Empty;
            FormatGroup[] formatGroups = new FormatGroup[0];
            try
            {
                formatGroups = m_repDbAccess.GetFormatGroups4Material(publicId);
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania grup formatów dla materiału o id = '{0}' z db: {1}", publicId, ex.ToString()));
                msg = ex.Message;
                return null;
            }
            return formatGroups;
        }
      
        bool SetFormatStatus(Format format, FormatStatus status, out string msg)
        {
            msg = string.Empty;
            if (format.Status != status)
                try
                {
                    m_repDbAccess.SetFormatStatus(format.UniqueId, format.Status, FormatStatus.Ready);
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Ustawiono status  formatu '{0}' na {1}", format.UniqueId, status));
                }
                catch (DBAccess.DBAccessException ex)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("ProcessDownloadResult: błąd zmiany stanu formatu o id='{0}' na Ready : {1}", format.Id, ex.ToString()));
                    msg = ex.Message;
                    return false;
                }
            return true;
        }

        bool SetFormatInternalStatus(Format format, FormatInternalStatus status, out string msg)
        {
            msg = string.Empty;
            if(format.InternalStatus != status)
                try
                {
                    m_repDbAccess.SetFormatInternalStatus(format.UniqueId, format.InternalStatus, status);
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Ustawiono status wew. formatu '{0}' na {1}", format.UniqueId, status));
                }
                catch (DBAccess.DBAccessException ex)
                {
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Błąd zmiany status wew. formatu '{0}' na {1} : {2}", format.UniqueId, status, ex.ToString()));
                    msg = ex.Message;
                    return false;
                }
            return true;
        }

        Format GetFormat(int formatId)
        {
            Format format = new Format();
            try
            {
                format = m_repDbAccess.GetFormat(formatId);
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania formatu o id = '{0}' z db: {1}", formatId, ex.ToString()));                
                return null;
            }
            return format;
        }

        Format GetFormat(string uniqueId, out string msg)
        {
            msg = string.Empty;
            Format format = new Format();
            try
            {
                format = m_repDbAccess.GetFormat(uniqueId);
            }
            catch (DBAccess.DBAccessException ex)
            {
                if (ex.Error != ErrorType.NotFound)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania formatu o id = '{0}' z db: {1}", uniqueId, ex.ToString()));
                    msg = ex.Message;
                    return null;
                }
            }
            return format;
        }

        FormatGroup GetFormatGroup(int formatGroupId)
        {
            FormatGroup formatGroup;
            try
            {
                formatGroup = m_repDbAccess.GetFormatGroup(formatGroupId);
            }
            catch (DBAccess.DBAccessException ex)
            {
                if(ex.Error == ErrorType.NotFound)
                     Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Brak grupy formatów o id '{0}' w db :{1}", formatGroupId, ex.Message));
                else
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania group dla id '{0}' :{1}", formatGroupId, ex.ToString()));
                return null;
            }
            return formatGroup;
        }

        FormatLocation[] GetAllFormatLocations(string uniqueId) // TODO metoda w DB !! - zwraca wszystkie lokalizacje również temp 
        {
            FormatLocation[] locations = null;
            try
            {
                locations = m_repDbAccess.GetFormatLocationsExt(uniqueId);
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano lokalizacje formatu '{0}': '{1}'", uniqueId, string.Join(", ", locations.Select(l => l.NodeId))));
            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania lokalizacji formatu '{0}': {1}", uniqueId, ex.ToString()));
                return null;
            }
            return locations;
        }
        
        // only snodes
        int[] GetFormatLocations(string uniqueId)
        {
            FormatLocation[] locations = GetAllFormatLocations(uniqueId);
            if (locations == null)
                return null;
            return locations.Where(l => (l.Role == NodeRole.Snode || l.Role == NodeRole.SNodeRecoder)).Select(l => l.NodeId).ToArray();            
        }

        Material GetMaterial4Format(Format format, out string msg)
        {
            Material material = null;
            msg = string.Empty;
            try
            {
                material = m_repDbAccess.GetMaterial4Format(format.Id);
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano materiał dla formatu o id = '{0}' z db: {1}", format.Id, material.PublicId));
            }
            catch (DBAccess.DBAccessException ex)
            {
                if (ex.Error != ErrorType.NotFound)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania materiału dla formatu o id = '{0}' z db: {1}", format.Id, ex.ToString()));
                    msg = ex.Message;
                    return null;
                }
            }
            return material;
        }
        
        // return null on error, true if removed, false if not found
        bool? RemoveMaterialDB(int materialId, out string msg)
        {
            msg = string.Empty;
            try
            {
                m_repDbAccess.RemoveMaterial(materialId);
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Materiał '{0}' został usunięty z db", materialId ));
            }
            catch(DBAccess.DBAccessException ex)
            {
                if (ex.Error != ErrorType.NotFound)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd usunięcia materiału '{0}' z db : {1}", materialId, ex.ToString()));
                    msg = ex.Message;
                    return null;
                }
                else
                    return false;
            }
            return true;
        }

        // return null on error, true if removed, false if not found
        bool? RemoveFormatDB(string uniqueId, out string msg)
        {
            msg = string.Empty;
            try
            {
                m_repDbAccess.RemoveFormat(uniqueId);
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Format '{0}' został usunięty z db", uniqueId ?? "NULL"));
            }
            catch (DBAccess.DBAccessException ex)
            {
                if (ex.Error != ErrorType.NotFound)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd usunięcia formatu '{0}' z db : {1}", uniqueId ?? "NULL", ex.ToString()));
                    msg = ex.Message;
                    return null;
                }
                else
                    return false;
            }
            return true;
        }

        bool RemoveFormatGroupDB(int formatGroupId, out string msg)
        {
            msg = string.Empty;
            try
            {
                m_repDbAccess.RemoveFormatGroup(formatGroupId);
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("FormatGroup '{0}' została usunięta z db", formatGroupId));
            }
            catch (DBAccess.DBAccessException ex)
            {
                if (ex.Error != ErrorType.NotFound)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd usunięcia grupy formatów '{0}' z db : {1}", formatGroupId, ex.ToString()));
                    msg = ex.Message;
                    return false;
                }
            }
            return true;
        }

        // return null on error, true if exists, false if does not exist
        bool? ExistsTask(string uniqueId, TaskType type, out string msg, long taskId = -1)
        {
            TaskShort[] tasks = null;
            msg = string.Empty;
            try
            {
                tasks = m_repDbAccess.GetTasksOfType4Format(type, uniqueId); //.GetTasksOfType(type);
            }
            catch (DBAccess.DBAccessException ex)
            {
                msg = ex.Message;
                return null;
            }
            if(taskId == -1)
			    return (tasks.Count(t => (t.Status == TaskStatus.New || t.Status == TaskStatus.Executing)) > 0);               
            else
                return (tasks.Count(t => (t.Status == TaskStatus.New || t.Status == TaskStatus.Executing) && t.Id != taskId) > 0);
        }
  
    
        int ReplicaCount(bool min)
        {
            int replicaCount = -1;
                try
                {
                    if(min)
                        replicaCount = Convert.ToInt16(m_repDbAccess.GetGlobalData("MinReplicaCount"));
                    else
                        replicaCount = Convert.ToInt16(m_repDbAccess.GetGlobalData("TargetReplicaCount"));
                }
                catch(Exception ex)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania wartości minimalnej/docelowej liczby kopii formatu {0}", ex.ToString()));                    
                }
            return replicaCount;
        }      
    }
}
