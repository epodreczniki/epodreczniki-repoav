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

        void AddFormat(Task task, bool checkFormat = false)
        {
            string msg = string.Empty;          

            Format format;
            if (checkFormat)
            {
                format = GetFormat(task.UniqueId, out msg);
                if (format == null)
                {
                    FinishTaskWithError(task, msg);
                    return;
                }
                if (format.Id != -1)
                {
                    FinishTaskWithError(task, string.Format("Format '{0}' jest już w repozytorium", format.UniqueId));
                    return;
                }
            }

            format = new Format() { UniqueId = task.UniqueId, Type = FormatType.Related };
            FormatGroup[] formatGroups = GetFormatGroups(task.PublicId, out msg);
            if (formatGroups == null)
            {
                FinishTaskWithError(task, msg);
                return;
            }
            FormatGroup formatGroup = formatGroups.FirstOrDefault(fg => fg.SourceId == null);
            if (formatGroup == null)
            {
                FinishTaskWithError(task, string.Format("Błąd ustalenia grupy źródłowej dla formatu '(0}'", format.UniqueId));
                return;
            }
            format.FormatGroupId = formatGroup.Id;

            if (!AddFormatDB(format, out msg))
            {
                FinishTaskWithError(task, msg);
                return;
            }

            if (ImportFormat(format, task.Content[AddKeywords.FormatURL.ToString()],
                 task.Content.ContainsKey(AddKeywords.MimeType.ToString()) ? task.Content[AddKeywords.MimeType.ToString()] : null,
                 task.Content.ContainsKey(AddKeywords.FormatMD5.ToString()) ? task.Content[AddKeywords.FormatMD5.ToString()] : null, out msg))
                FinishTask(task);
            else
                FinishTaskWithError(task, msg);
        }

        void UpdateFormat(Task task)
        {
            string msg = string.Empty;
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie update formatu '{0}', zadanie {1}", task.UniqueId, task.Id));
            Format format = GetFormat(task.UniqueId, out msg);
            if (format == null)
            {
                FinishTaskWithError(task, msg);
                return;
            }
            if (format.Id == -1) // there is no such format - add
            {
                FinishTask(task);
                AddFormat(task, false);
                return;
            }

            bool? result = RemoveFormat(format, true, out msg);
            if (result == null)
            {
                FinishTaskWithError(task, msg);
                return;
            }

            //submit AddFormat which will wait for remove to complete
            TaskAdd newTask = new TaskAdd() { Type = TaskType.AddFormat, PublicId = task.PublicId, UniqueId = task.UniqueId };               
            foreach (var item in task.Content)
                newTask.Content.Add(item.Key, item.Value);
            AddTaskDB(newTask, out msg); 

           FinishTask(task);        
            return;
        }

        void RemoveFormat(Task task)
        {
            string msg = string.Empty;
            if (RemoveFormat(task.UniqueId, out msg))
                FinishTask(task);
            else
                FinishTaskWithError(task, msg);
                
        }


        /// <summary>
        /// Create Remove tasks for all snodes that store given format replica 
        /// </summary>
        /// <param name="format">format to be removed</param>
        /// <param name="result"></param>
        /// <returns>null on error, true if work finished, false otherwise </returns>
        bool? RemoveFormat(Format format, bool forced, out string msg)
        {
            msg = string.Empty;
            if (!SetFormatInternalStatus(format, FormatInternalStatus.RemovePending, out msg))
                return null;

            FormatLocation[] locations = null;
            locations = GetAllFormatLocations(format.UniqueId);
            if (locations == null)
            {
                msg = "Błąd pobrania lokalizacji formatu";
                return null;
            }
            foreach (FormatLocation location in locations)
            {
                if (forced && !location.IsOnLine) // remove location from RepoDB to be able to proceed 
                    m_repDbAccess.RemoveFormatLocation(format.UniqueId, location.NodeId, -1);
                else
                {
                    TaskAdd newTask = new TaskAdd() { Type = TaskType.Remove, UniqueId = format.UniqueId, PreferredNodeIds = new int[] { location.NodeId } };
                    newTask.Content.Add(RemoveKeywords.FormatId.ToString(), format.Id.ToString());

                    if (AddTaskDB(newTask, out msg))
                        Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zlecono zadanie {2} typu Remove dla formatu '{0}' snode'owi {1}", format.UniqueId, location.NodeId, newTask.Id));
                    else
                        return null;
                }
            }
            
            if (locations.Length == 0 || (forced && locations.Count(l => l.IsOnLine) == 0))
                if (RemoveFormatDB(format.UniqueId, out msg) != null)
                    return true;
                else
                    return null;
            else
                return false;
        }

        /// <summary>
        /// Create Recode tasks for the given format
        /// </summary>
        /// <param name="format">format to be recoded</param>
        /// <param name="audioIndex">index of source format audio stream </param>
        /// <param name="urls">urls for all source formats</param>
        /// <param name="profile">recoding profile to be used</param>
        /// <returns>null on error, true if done, false otherwise</returns>
        bool? RecodeFormat(Format[] formats, int audioIndex, string[] sourceFormatIds, string downloadSourceFiles, string operationXMl, string taskSubtype, out string msg)
        {
            string uniqueIds = string.Join(";", formats.Select(f => f.UniqueId));
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Wywołanie rekodowania formatów '{0}' dla audioId '{1}'", uniqueIds, audioIndex));

            if (formats.Count(f => f.InternalStatus == FormatInternalStatus.Recoding) > 0)
            {
                bool? result = ExistsTask(formats[0].UniqueId, TaskType.Recode, out msg);
                if(!result.HasValue)
                    return null;
                if (result.Value) // check if there is task defined already  
                {
                    Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Jest już zdefiniowane zadanie rekodowania formatu '{0}'", formats[0].UniqueId));
                    msg = string.Empty;
                    return false;
                }
            }
            else
                foreach (Format format in formats)
                    if (!SetFormatInternalStatus(format, FormatInternalStatus.Recoding, out msg))
                        return null;


            TaskAdd task = new TaskAdd() { Type = TaskType.Recode, UniqueId = formats[0].UniqueId, TaskSubtype = taskSubtype };

            int[] recoders = SelectRecoder(sourceFormatIds[0]);

            if (recoders.Length > 0)
            {
                task.PreferredNodeIds = recoders;
                task.CanSkipPreferredNodes = true;
            }

            if (sourceFormatIds.Length < downloadSourceFiles.Split(';').Length)
                downloadSourceFiles = string.Join(";", downloadSourceFiles.Split(';').Take(sourceFormatIds.Length));

            if (sourceFormatIds.Length != downloadSourceFiles.Split(';').Length)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd konfiguracji operacji rekodowania: liczba sourceFormatIds różna od DownloadSourceFiles"));
                msg = string.Empty;
                return null;
            }
            task.Content.Add(RecodeKeywords.SourceFormatIds.ToString(), string.Join(";", sourceFormatIds));
            task.Content.Add(RecodeKeywords.DownloadSourceFiles.ToString(), downloadSourceFiles);

            task.Content.Add(RecodeKeywords.OutputFormatIds.ToString(), uniqueIds);

            task.Content.Add(RecodeKeywords.Parameter1.ToString(), audioIndex == -1 ? "0" : audioIndex.ToString());
            task.Content.Add(RecodeKeywords.ParameterCount.ToString(), "1");

            task.Content.Add(RecodeKeywords.OperationXML.ToString(), operationXMl);



            if (AddTaskDB(task, out msg))
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zlecono zadanie rekodowania formatów '{0}'", uniqueIds));
            else
                return null;

            return false;
        }

        /// <summary>
        /// Add format to database and import format
        /// </summary>
        /// <param name="format"></param>
        /// <param name="url"></param>
        /// <param name="mimeType"></param>
        /// <param name="md5"></param>
        /// <returns></returns>
        bool AddSourceFormat(Format format, string url, out string msg, string mimeType = null, string md5 = null)
        {
            msg = string.Empty;
            if (format.Id == -1)
            {
                if (!AddFormatDB(format, out msg))
                    return false;
            }

            if (!ImportFormat(format, url, mimeType, md5, out msg))
            {
                SetFormatInternalStatus(format, FormatInternalStatus.AddError, out msg);
                return false;
            }

            return true;
        }

        /// <summary>
        /// import format from an external url
        /// </summary>
        /// <param name="format">format to be imported</param>
        /// <param name="url">format access url</param>
        /// <param name="mimeType">mime type (optional)</param>
        /// <param name="md5">md5 value (optional)</param>
        /// <returns></returns>
        bool ImportFormat(Format format, string url, string mimeType, string md5, out string msg)
        {
            msg = string.Empty;
            bool? result = ExistsTask(format.UniqueId, TaskType.Download, out msg);
            if (!result.HasValue)
                return false;
            if (result.Value) // there are Download tasks in progress for the given format
                return true;

            TaskAdd task = new TaskAdd() { Type = TaskType.Download, UniqueId = format.UniqueId };
            task.Content.Add(DownloadKeywords.FormatURL.ToString(), url);
            if (!string.IsNullOrEmpty(mimeType))
            {
                task.Content.Add(DownloadKeywords.MimeType.ToString(), mimeType);
                string fileExt = m_repDbAccess.GetFileExtension4Mime(mimeType);
                if (!string.IsNullOrEmpty(fileExt))
                    task.Content.Add(DownloadKeywords.FileExtension.ToString(), fileExt);
            }
            if (!string.IsNullOrEmpty(md5))
                task.Content.Add(DownloadKeywords.FormatMD5.ToString(), md5);

            task.Content.Add(DownloadKeywords.OverwriteIfExists.ToString(),"true");

            if (!SetFormatInternalStatus(format, FormatInternalStatus.Adding, out msg))
                return false;

            if (AddTaskDB(task, out msg))
            {
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zlecono zadanie typu Download dla formatu '{0}' z url={1}, mimeType={2} i md5={3}",
                      format.UniqueId, url, mimeType, md5));
                return true;
            }

            return false;
        }

        bool PushFormat(Format format, int[] target)
        {
            TaskAdd task = new TaskAdd() { Type = TaskType.Download, UniqueId = format.UniqueId, PreferredNodeIds = target };


            string fileExt = m_repDbAccess.GetFileExtension4Mime(format.Mime);
            if (!string.IsNullOrEmpty(fileExt))
                task.Content.Add(DownloadKeywords.FileExtension.ToString(), fileExt);
            task.Content.Add(DownloadKeywords.OverwriteIfExists.ToString(), "true");

            string msg = string.Empty;
            if (AddTaskDB(task, out msg))
            {
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zlecono zadanie typu Download snode'owi '{0}' dla formatu '{1}'",
                    string.Join(" ", target.Select(n => n.ToString())), format.UniqueId));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replicate format
        /// </summary>
        /// <param name="format"></param>
        /// <returns>null on error, true when done, false otherwise</returns>
        bool? ReplicateFormat(Format format)
        {
            string msg;
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Replikacja formatu: '{0}'", format.UniqueId));
            bool? result  = ExistsTask(format.UniqueId, TaskType.Download, out msg);
            if (!result.HasValue)
                return null;
            if (result.Value) // there are Download tasks in progress for the given format
            {
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Istnieją niezakończone zadania typu Download dla formatu: '{0}'",
                    format.UniqueId));
                return false;
            }

            bool notAvailable = false;
            int[][] targetLocations = SelectTargetLocations(format, out notAvailable);

            if (targetLocations == null)
                return null;

            if (notAvailable)
            {
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Stwierdzono niedostępność formatu '{0}'", format.UniqueId));
                SetFormatInternalStatus(format, FormatInternalStatus.AddError, out msg);
                return true;

                /*if (format.Type == FormatType.Recoded)
                {
                    Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Ze względu na niedostępność formatu '{0}', konieczne jest rekodowanie", format.UniqueId));
                    if (RecodeFormat(format, out msg) == null)
                        return null;
                    return false;
                }
                else
                    return null; // source format unavailable*/
            }
            if (targetLocations.Length == 0) // have all replicas needed
            {
                if (!SetFormatStatus(format, FormatStatus.Ready, out msg) || !SetFormatInternalStatus(format, FormatInternalStatus.Added, out msg))
                    return null;
                return true;
            }

            int scheduledCount = 0;
            if (SetFormatInternalStatus(format, FormatInternalStatus.Adding, out msg))
                foreach (int[] snodes in targetLocations)
                {
                    if (PushFormat(format, snodes))
                        scheduledCount++;
                }
            return (scheduledCount == 0);
        }

        /// <summary>
        /// Create recoded formats for the given group of formats in database and schedule Recode tasks
        /// </summary>
        /// <param name="formatGroupId">format goup id</param>
        /// <param name="profiles">array of applicable profiles</param>
        /// <param name="audioIndex">index of source format audio stream</param>
        /// <param name="urls">access urls for all source formats</param>
        /// <returns></returns>
        bool CreateRecodedFormats(int formatGroupId, ProfileGroupExt[] profileGroups, int audioIndex, string[] sourceFormatIds, string partialId, out string msg)
        {
            Format[] formats = GetFormats4Group(formatGroupId, out msg);
            if (formats == null)
                return false;

            bool success = true;
            foreach (ProfileGroupExt profileGroup in profileGroups)
            {
                List<Format> newFormats = new List<Format>();
                bool recodingNeeded = false;

                foreach (Profile profile in profileGroup.Profiles)
                {
                    Format newFormat = formats.FirstOrDefault(f => f.ProfileId == profile.Id);
                    if (newFormat == null)
                    {
                        newFormat = new Format()
                        {
                            UniqueId = partialId.Replace(")", profile.Name + ")"),
                            FormatGroupId = formatGroupId,
                            ProfileId = profile.Id,
                            Type = FormatType.Recoded,
                            Mime = profile.Mime,
                            InternalStatus = FormatInternalStatus.Recoding
                        };
                        if (!AddFormatDB(newFormat, out msg))
                            return false;
                    }
                    else
                        Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla profilu {0} istnieje już format '{1}' w grupie '{2}'", profile.Name, newFormat.UniqueId, formatGroupId));

                    newFormats.Add(newFormat);
                    if (newFormat.InternalStatus == FormatInternalStatus.Recoded)
                    {
                        FormatLocation[] locations = GetAllFormatLocations(newFormat.UniqueId);
                        if (locations == null)
                            return false;
                        if (locations.Length > 0) // format available
                            ReplicateFormat(newFormat);
                        else // not avaialble 
                            recodingNeeded = true;
                        //success &= (RecodeFormat(newFormat, audioIndex, sourceFormatIds, profileGroup.OperationXML, profileGroup.TaskSubtype, out msg) != null);
                    }
                    else
                        if (newFormat.InternalStatus == FormatInternalStatus.Recoding || newFormat.InternalStatus == FormatInternalStatus.RecError)
                            recodingNeeded = true;
                    //success &= (RecodeFormat(newFormat, audioIndex, sourceFormatIds, profileGroup.OperationXML, profileGroup.TaskSubtype, out msg) != null);
                }
                if (recodingNeeded)
                    success &= (RecodeFormat(newFormats.ToArray(), audioIndex, sourceFormatIds, profileGroup.DownloadSourceFiles, profileGroup.OperationXML, profileGroup.TaskSubtype, out msg) != null);
            }
            return success;
        }
    }
}
