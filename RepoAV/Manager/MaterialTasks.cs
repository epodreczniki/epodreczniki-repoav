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
        void AddMaterial(RepDBAccess.Task task)
        {
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie dodania materiału '{0}'; zadanie {1}", task.PublicId, task.Id));
            string message = string.Empty;
            if (!AddMaterial(task.PublicId, task.Content, out message))
                FinishTaskWithError(task, message);
            else
                FinishTask(task);
        }

        bool AddMaterial(string publicId, Dictionary<string, string> content, out string message)
        {
            message = string.Empty;

            Dictionary<string, string> metadata = null;
            string[] tags = null;
            if (content.ContainsKey(AddKeywords.Metadata.ToString()))
            {
                metadata = XmlUtils.GetXmlNodes(content[AddKeywords.Metadata.ToString()], new string[] { "title", "subtitlesURL", "captionsURL"}, false);
                string customMetadata = XmlUtils.GetXmlNode(content[AddKeywords.Metadata.ToString()], "customMeta", true);
                if(!string.IsNullOrEmpty(customMetadata))
                    metadata.Add("customMeta", customMetadata);
                tags = XmlUtils.GetXmlNodes(content[AddKeywords.Metadata.ToString()], "tag");
                if (metadata == null || tags == null)
                {
                    message = string.Format("Błąd przetwarzania metadanych XML materiału '{0}'", publicId);
                    Log.TraceMessage(TraceEventType.Error, GetName(), message);
                    return false;
                }
            }            

            // create material if needed
            Material material = GetMaterial(publicId, out message);
            if(material == null )
                return false;

            if (material.Id == -1)
            {
                MaterialType materialType = MaterialType.Unknown;
                if (content.ContainsKey(AddKeywords.MimeType.ToString()))
                    if (content[AddKeywords.MimeType.ToString()].ToLower().StartsWith("audio"))
                        materialType = MaterialType.Audio;
                    else if (content[AddKeywords.MimeType.ToString()].ToLower().StartsWith("video"))
                        materialType = MaterialType.Video;                  

                material = new Material() { PublicId = publicId, MaterialType = materialType, AllowDistribution = true };
                material.Title = metadata != null && metadata.ContainsKey("title") ? metadata["title"] : publicId;
                if(metadata != null && metadata.ContainsKey("customMeta"))
                {
                    UTF8Encoding encoding = new UTF8Encoding();
                    material.Metadata = new System.Data.SqlTypes.SqlXml(new System.IO.MemoryStream(encoding.GetBytes(metadata["customMeta"])));
                }
                if(tags != null && tags.Length > 0)
                    material.Tags = tags;

                if (!AddMaterialDB(material, out message))
                    return false;                
            }
            else
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano materiał '{0}' z db", publicId));


            // create format group for source format and subtitles 
            FormatGroup[] formatGroups = m_repDbAccess.GetFormatGroups4Material(material.PublicId);
            FormatGroup sourceFormatGroup = formatGroups.FirstOrDefault(group => group.SourceId == null);
            Format[] sourceFormats = new Format[0];

            if (sourceFormatGroup == null)
            {
                sourceFormatGroup = new FormatGroup() { MaterialId = material.Id };
                if (!AddFormatGroupDB(sourceFormatGroup, material.PublicId, out message))
                    return false;
            }
            else
            {
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano grupę źródłową '{0}' dla materiału '{1}' z FSC", sourceFormatGroup.Id, material.PublicId));
                sourceFormats = GetFormats4Group(sourceFormatGroup.Id, out message);
                if (sourceFormats == null)
                    return false;
            }

            // create source format 
            Format sourceFormat = sourceFormats.FirstOrDefault(fr => fr.Type == FormatType.Source);
            if (sourceFormat == null)
                sourceFormat = new Format() { UniqueId = material.PublicId, FormatGroupId = sourceFormatGroup.Id, Type = FormatType.Source };
            else
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla materiału '{0}' ustalono format źródłowy '{1}'", material.PublicId, sourceFormat.UniqueId));
            if (!AddSourceFormat(sourceFormat, content[AddKeywords.FormatURL.ToString()], out message, content[AddKeywords.MimeType.ToString()], 
                content.ContainsKey(AddKeywords.FormatMD5.ToString()) ? content[AddKeywords.FormatMD5.ToString()] : null))
            {
                message = "Błąd dodania formatu źródłowego";
                return false;
            }

            // create subtitles and captions                        
            if (metadata != null && metadata.ContainsKey("subtitlesURL"))
            {
                sourceFormat = sourceFormats.FirstOrDefault(fr => fr.Type == FormatType.Subtitle);
                if (sourceFormat == null)
                    sourceFormat = new Format() { UniqueId = string.Format("{0}_subtitles", material.PublicId), FormatGroupId = sourceFormatGroup.Id, Type = FormatType.Subtitle };
                else
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla materiału '{0}' ustalono format subtitle '{1}'", material.PublicId, sourceFormat.UniqueId));
                if (!AddSourceFormat(sourceFormat, metadata["subtitlesURL"], out message))
                {
                    message = "Błąd dodania formatu subtitles";
                    return false;
                }
            }

            if (metadata != null && metadata.ContainsKey("captionsURL"))
            {
                sourceFormat = sourceFormats.FirstOrDefault(fr => fr.Type == FormatType.Caption);
                if (sourceFormat == null)
                    sourceFormat = new Format() { UniqueId = string.Format("{0}_captions",material.PublicId),  FormatGroupId = sourceFormatGroup.Id, Type = FormatType.Caption };
                else
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla materiału '{0}' ustalono format captions '{1}'", material.PublicId, sourceFormat.UniqueId));
                if (!AddSourceFormat(sourceFormat, metadata["captionsURL"], out message))
                {
                    message = "Błąd dodania formatu captions";
                    return false;
                }
            }
            return true;
        }

        bool UpdateMaterial(Task task)
        {
            string msg = string.Empty;
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie update materiału '{0}', zadanie {1}", task.PublicId, task.Id));
            Material material = GetMaterial(task.PublicId, out msg);
            if (material == null)
            {
                FinishTaskWithError(task, msg);
                return false;
            }
            if (material.Id == -1) // there is no such material - proceed with update
            {
                FinishTask(task);
                AddMaterial(task);
                return true;
            }
               
            bool? result = RemoveMaterial(material, true, out msg);
            if (result == null)
            {
                FinishTaskWithError(task, msg);
                return false;
            }
    
            // submitt AddMaterial task which will wait for remove to complete
            TaskAdd newTask = new TaskAdd() { Type = TaskType.AddMaterial, PublicId = task.PublicId };
            foreach (var item in task.Content)
                newTask.Content.Add(item.Key, item.Value);
            AddTaskDB(newTask, out msg); // addMaterial added to Task only if there is no new update or remove waiting (checked in DB)

            FinishTask(task);
            return true; 
        }

        bool? RecodeMaterial(int materialId)
        {
            Material material = GetMaterial(materialId);
            if (material == null)
                return false;
            string msg;
            return RecodeMaterial(material, out msg);
        }

        // return null on error, true if done, false if need to wait
        bool? RecodeMaterial(Material material, out string msg)
        {
            msg = string.Empty;
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie rekodowania materiału '{0}'.", material.PublicId));

			ProfileGroupExt[] profGroups = m_repDbAccess.GetProfilesWithGroups4MaterialType(material.MaterialType);           

			if (profGroups.Length == 0)
            {
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Dla materiału '{0}' brak profilów rekodowania.", material.PublicId));
                return true;
            }
			Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano profile rekodowania dla materiału typu {0} w liczbie {1}",
                material.MaterialType, profGroups.Sum(gr => gr.Profiles.Count)));

            FormatGroup[] formatGroups = m_repDbAccess.GetFormatGroups4Material(material.PublicId);
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano grupy formatów dla materiału '{0}' w liczbie {1}", material.PublicId, formatGroups.Length));

            // determine source formats
            var sourceFormatGroup = formatGroups.FirstOrDefault(group => group.SourceId == null);
            if (sourceFormatGroup == null)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Brak źródłowej grupy formatów dla materiału '{0}'", material.PublicId));
                return null;
            }
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla materiału '{0}' ustalono grupę źródłową '{1}'", material.PublicId, sourceFormatGroup.Id));

            Format[] sourceFormats = GetFormats4Group(sourceFormatGroup.Id, out msg);
            if (sourceFormats == null)
                return null;

            Format sourceFormat = sourceFormats.FirstOrDefault(fr => fr.Type == FormatType.Source);
            if (sourceFormat == null)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Brak formatu źródłowego dla materiału '{0}'", material.PublicId));
                return null;
            }
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla materiału '{0}' ustalono format źródłowy '{1}'", material.PublicId, sourceFormat.UniqueId));

            if (!GetApplicableProfiles(profGroups, sourceFormat, material.MaterialType))
                return null;

            if (profGroups.Sum(gr => gr.Profiles.Count) == 0)
            {
                Log.TraceMessage(TraceEventType.Warning, GetName(), string.Format("Dla materiału '{0}' stwierdzono brak pasujących profilów rekodowania", material.PublicId));
                return true;
            }


            // determine subtitles and audio tracks
            Format[] subtitles = new Format[2];
            subtitles[0] = sourceFormats.FirstOrDefault(fr => fr.Type == FormatType.Subtitle);
            subtitles[1] = sourceFormats.FirstOrDefault(fr => fr.Type == FormatType.Caption);
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla materiału '{0}' znaleziono formaty z napisami w liczbie {1}", material.PublicId, 
                subtitles.Count(s => s != null)));

            int audioStreamCount = XmlUtils.GetXmlNodeCount(sourceFormat.XmlMetadata, "AudioCoding");
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Liczba ścieżek dźwiękowych dla materiału '{0}' wynosi {1}  ", material.PublicId, audioStreamCount));
            bool hasAudioStream = true;
            if (audioStreamCount == 0)
            {
                hasAudioStream = false;
                audioStreamCount = 1;
            }


            // create format groups and formats for recoded formats
            for (int audioIndex = 0; audioIndex < (m_ProcessFirstAudioOnly ? 1 : audioStreamCount); audioIndex++)
            {
                FormatGroup formatGroup = formatGroups.FirstOrDefault(gr => gr.SubtitleId == null && (!hasAudioStream || gr.AudioId == audioIndex) && gr.SourceId == sourceFormat.UniqueId);
                if (formatGroup == null)
                {
                    formatGroup = new FormatGroup() { MaterialId = material.Id, SourceId = sourceFormat.UniqueId, PublicId = material.PublicId };
                    if (hasAudioStream)
                        formatGroup.AudioId = audioIndex;
                    if (!AddFormatGroupDB(formatGroup, material.PublicId, out msg))
                        return null;
                }
                else
                    Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla ścieżki '{0}' materiału '{1}' istnieje już grupa formatów '{2}'", formatGroup.AudioId, 
                        material.PublicId, formatGroup.Id));

                List<string> sourceFormatIds = new List<string>(){sourceFormat.UniqueId};

                string  partialId = string.Format("{0}({1},,)", material.PublicId, audioIndex > 0 ? audioIndex.ToString() : "");
                if (!CreateRecodedFormats(formatGroup.Id, profGroups, hasAudioStream ? audioIndex : -1, new string[]{sourceFormat.UniqueId}, partialId, out msg))
                    return null;

                foreach (Format subtitle in subtitles)
                    if (subtitle != null)
                    {
                        formatGroup = formatGroups.FirstOrDefault(gr => gr.SubtitleId == subtitle.UniqueId && (!hasAudioStream || gr.AudioId == audioIndex) && gr.SourceId == sourceFormat.UniqueId);
                        if (formatGroup == null)
                        {
                            formatGroup = new FormatGroup() { MaterialId = material.Id, SubtitleId = subtitle.UniqueId, SourceId = sourceFormat.UniqueId, PublicId = material.PublicId };
                            if (hasAudioStream)
                                formatGroup.AudioId = audioIndex;
                            if (!AddFormatGroupDB(formatGroup, material.PublicId, out msg))
                                continue;
                        }
                        else
                            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla ścieżki '{0}' i napisów typu {3} materiału '{1}' istnieje już grupa formatów '{2}'", 
                                formatGroup.AudioId, material.PublicId, formatGroup.Id, subtitle.Type));
                        
                            partialId = string.Format("{0}({1},{2}s,)", material.PublicId, audioIndex > 0 ? audioIndex.ToString() : "", subtitle.Type.ToString().ToLower());
                        if (!CreateRecodedFormats(formatGroup.Id, profGroups, hasAudioStream ? audioIndex : -1, new string[]{sourceFormat.UniqueId, subtitle.UniqueId}, partialId, out msg))
                        {
                            return null;
                        }
                    }
            }
            return false;
        }

        bool RemoveMaterial(Task task)
        {
            string msg = string.Empty;
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie usunięcia materiału '{0}', zadanie {1}", task.PublicId, task.Id));
            Material material = GetMaterial(task.PublicId, out msg);
            if (material == null)
            {
                FinishTaskWithError(task, msg);
                return false;
            }
            if(material.Id == -1)
            {
                FinishTask(task);
                return true;
            }

            if (RemoveMaterial(material, false, out msg) == null)
            {
                FinishTaskWithError(task, msg);
                return false;
            }
            else
            {
                FinishTask(task);
                return true; 
            }
        }

        // null on error, true done, false not done 
        bool? RemoveMaterial(Material material, bool forced, out string msg)
        {         
            msg = string.Empty;

            FormatGroup[] formatGroups = GetFormatGroups(material.PublicId, out msg);
            if (formatGroups == null)
                return null;
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Pobrano grupy formatów dla materiału '{0}' w liczbie {1}", material.PublicId, formatGroups.Length));
            bool done = true;
            
            // start with recoded format groups - source formats cannot be removed before other format groups
            FormatGroup sourceFormatGroup = formatGroups.FirstOrDefault(group => group.SourceId == null);            
            if(sourceFormatGroup != null && formatGroups.Length > 1) // switch source group with the last element
            {
                int i = Array.IndexOf(formatGroups, sourceFormatGroup);
                FormatGroup temp = formatGroups[i];
                formatGroups[i] = formatGroups.Last();
                formatGroups[formatGroups.Length-1] = temp;
            }

            foreach (FormatGroup formatGroup in formatGroups)
            {
                Format[] formats = GetFormats4Group(formatGroup.Id, out msg);
                if (formats == null)
                    return null;

                // set sourceId to null to enable source and subtitle format removal
                formatGroup.SourceId = null;
                formatGroup.SubtitleId = null;
                m_repDbAccess.ChangeFormatGroup(formatGroup);

                bool scheduledTasks = false;
                foreach (Format format in formats)
                {
                    bool? res = RemoveFormat(format, forced, out msg);
                    if (res == null)
                        return null;
                    if (res == false)
                        scheduledTasks = true;
                }
                if (!scheduledTasks)
                {
                    if (!RemoveFormatGroupDB(formatGroup.Id, out msg))
                    {
                        return null;
                    }
                }
                else
                    done = false;
            }
            if (done)
                return RemoveMaterialDB(material.Id, out msg);
            else
                return false; 
        }

       
    }
}
