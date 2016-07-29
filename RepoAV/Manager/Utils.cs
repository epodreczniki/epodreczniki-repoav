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
        
        void FinishTaskWithError(Task task, string msg)
        {
            m_repDbAccess.FinishTask(task.Id, msg, false);
        }

        void FinishTask(RepDBAccess.Task task)
        {
            m_repDbAccess.FinishTask(task.Id, string.Empty, true);
        }
              
        /// <summary>
        /// select recoder with the smallest load 
        /// </summary>
        /// <returns>id of the selected recoder</returns>
        int[] SelectRecoder(string uniqueId)
        {           
             if (!m_selectRecoders)
                 return new int[0]; // no preferred nodes

            // choose recoders as preferred nodes
            var recoders = m_repDbAccess.GetNodes(NodeRole.Recoder).Where(n => n.IsOnline);
            if (recoders.Count() > 0)
                return recoders.Select(r => r.Id).ToArray();

            FormatLocation[] locations = GetAllFormatLocations(uniqueId);
            if (locations == null)
                return null;

            // find SNodeRecoder with format
            var nodes = locations.Where(l => l.Role == NodeRole.SNodeRecoder && l.IsOnLine);
            if(nodes.Count() > 0)
                return nodes.Select(l => l.NodeId).ToArray();

            return new int[0]; // no preferred nodes
        }

        /// <summary>
        /// select snode with sufficient storage space for the format
        /// </summary>
        /// <param name="format"> format for replication</param>
        /// <returns> array of selected node ids</returns>
        int[] GetSNodes(long sizeNeeded = 0)
        {
             Node[] snodes  = new Node[0];
             try
             {
                 snodes = m_repDbAccess.GetNodes(NodeRole.Snode);
				 snodes = snodes.Union(m_repDbAccess.GetNodes(NodeRole.SNodeRecoder)).ToArray();
                 snodes = snodes.Where(n => n.IsOnline).ToArray();                  
             }
              catch (DBAccess.DBAccessException ex)
             {
                 Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania węzłow z db: {0}", ex.Error));
             }
             if (snodes.Length == 0)
                 return new int[0];

            if(sizeNeeded == 0)
                return snodes.Select(n => n.Id).ToArray();
            
            // select snodes with sufficient storage space for format
            snodes = snodes.Where(sn => (sn.FreeSpace.HasValue && sn.FreeSpace > sizeNeeded/1024.0/1024)).ToArray();
            Array.Sort(snodes, new SNodeComparer());          
            return snodes.Select(n => n.Id).ToArray();
        }

        // check profile applicability
        /// <summary>
        ///  select profiles from a given list whose constraints are satisfied by the given format
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="sourceFormat"></param>
        /// <returns></returns>
        bool GetApplicableProfiles(ProfileGroupExt[] profileGroups, Format format, MaterialType materialType)
        {
            if (format.XmlMetadata == null)
                return false;                    

            ushort width = 0;
            ushort height = 0;

            if (materialType == MaterialType.Video)
            {
                Dictionary<string, string> dimensions = XmlUtils.GetXmlNodes(format.XmlMetadata, new string[] { "Width", "Height" });
                if (!dimensions.ContainsKey("Width") || !dimensions.ContainsKey("Height"))
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd ustalenia rozdzielczości video formatu '{0}'", format.UniqueId));
                    return false;
                }

                if ((dimensions["Width"] != null && !UInt16.TryParse(dimensions["Width"], out width)) || (dimensions["Height"] != null && !UInt16.TryParse(dimensions["Height"], out height)))
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd ustalenia rozdzielczości video formatu '{0}': width = {1}, height = {2}", format.UniqueId,
                        dimensions["Width"], dimensions["Height"]));
                    return false;
                }

                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla formatu '{0}' ustalono rozdzielczość: width = {1}, height ={2}", format.UniqueId,
                       dimensions["Width"], dimensions["Height"]));
            }
			foreach (ProfileGroupExt pg in profileGroups)
            {
                List<Profile> appProfiles = new List<Profile>();
				foreach (Profile profile in pg.Profiles)
				{
					if (materialType == MaterialType.Audio || (!profile.MinHeight.HasValue  && !profile.MinWidth.HasValue && !profile.Apect.HasValue))
					{
						appProfiles.Add(profile);
						Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla formatu '{0}' wybrano bezwarunkowy profil '{1}'", format.UniqueId, profile.Name));
						continue;
					}
					if ((!profile.MinWidth.HasValue || width >= profile.MinWidth) && (!profile.MinHeight.HasValue || height >= profile.MinHeight) &&
						(!profile.Apect.HasValue || width / height != profile.Apect))
					{
						appProfiles.Add(profile);
						Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla formatu '{0}' wybrano profil '{1}'", format.UniqueId, profile.Name));
					}
					else
						Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Format '{0}' nie spełnia warunków profilu '{1}'", format.UniqueId, profile.Name));
				}
                pg.Profiles = appProfiles;
            }
            return true;
        }

        // return null on error 
        int[][] SelectTargetLocations(Format format, out bool notAvailable)
        {
            List<List<int>> targetLocations = new List<List<int>>();
            notAvailable = false;

            FormatLocation[] formatLocations = GetAllFormatLocations(format.UniqueId);
            //int[] formatLocations = GetFormatLocations(format.UniqueId);
            if (formatLocations == null)
                return null;

            if (formatLocations.Length == 0)
            {
                notAvailable = true;
                return targetLocations.Select(l => l.ToArray()).ToArray();
            }

            int targetReplicaCount = ReplicaCount(false);
            if (targetReplicaCount < 0)
                return null;

            int[] formatLocationIds = formatLocations.Where(fl => fl.Role == NodeRole.Snode || fl.Role == NodeRole.SNodeRecoder).Select(fl => fl.NodeId).ToArray();
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla formatu '{0}' ustalono lokalizacje na snode'ach: {1}", format.UniqueId, string.Join(",", formatLocationIds)));
            if (formatLocationIds.Length < targetReplicaCount)
            {
                int needed = targetReplicaCount - formatLocationIds.Length;
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla formatu '{0}' ustalono liczbę potrzebnych snode'ów na {1}", format.UniqueId, needed));

                var snodes = GetSNodes(format.Size).Where(sid => !formatLocationIds.Contains(sid)).ToList();
                Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Dla formatu '{0}' ustalono potencjalne snode'y: {1}", format.UniqueId, string.Join(",", snodes)));
                if (snodes.Count == 0)
                    return targetLocations.Select(l => l.ToArray()).ToArray(); // empty list

                if (snodes.Count <= needed)
                    foreach (var snode in snodes)
                    {
                        targetLocations.Add(new List<int>()); // all snodes get task
                        targetLocations.Last().Add(snode);
                    }
                else // split snodes into several groups (needed)
                {
                    int step = (int)Math.Ceiling(snodes.Count / (double)needed);
                    while (snodes.Count() > 0)
                    {
                        targetLocations.Add(new List<int>(snodes.Take(Math.Min(step, snodes.Count))));
                        snodes.RemoveRange(0, Math.Min(step, snodes.Count));
                    }
                }
            }

            return targetLocations.Select(l => l.ToArray()).ToArray();
        }

        bool StartRepair(TaskType taskType)
        {
            Task[] tasks = m_repDbAccess.GetTasksOfType(taskType);
            string msg = string.Empty;
            if (tasks.Count(t => t.Status == TaskStatus.New || t.Status == TaskStatus.Executing) < 1)
            {
                if (!AddTaskDB(new TaskAdd() { Type = taskType }, out msg))
                    return false;
            }
            else
                if (tasks.Count(t => t.Status == TaskStatus.New && t.BeginDate > DateTime.Now.AddMinutes(1)) > 0) // if wait is longer than 1 minute - change it to now
                    if (m_repDbAccess.RemoveTask(tasks.Where(t => t.Status == TaskStatus.New).First().Id))
                    {
                        if (!AddTaskDB(new TaskAdd() { Type = taskType }, out msg))
                            return false;
                    }
            return true;
        }

        bool SendNotification(Format format)
        {
            string msg;
            Material material = GetMaterial4Format(format, out msg);
            if (material == null)
                return false;
            return SendNotification(material.PublicId);

        }
        
        bool SendNotification(string publicId)
        {
            string url = string.Empty;
            string statusName = string.Empty;
            try
            {
                url = m_repDbAccess.GetGlobalData("NotificationURL");
                
                if (string.IsNullOrEmpty(url))
                    return true;

                bool notFound = false;
                MaterialStatus? status =  GetMaterialStatus(publicId, out notFound);
                if (status == null)
                {
                    Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błąd pobrania stanu materiału '{0}'.", publicId));
                    return false;
                }
                statusName = notFound ? "Removed" : status.ToString();

                System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                request.Method = "POST";

                string data = string.Format("transactionId={0}&status={1})", System.Web.HttpUtility.UrlEncode(publicId), System.Web.HttpUtility.UrlEncode(statusName));
                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                System.IO.Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                System.Net.WebResponse response = request.GetResponse();
                if (((System.Net.HttpWebResponse)response).StatusCode != System.Net.HttpStatusCode.OK)
                    Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Odpowiedź na wysłanie powiadomienia o przyjęciu materiału {0} z rezultatem {1} na adres {2} = {3}",
                  publicId, status.ToString(), url, ((System.Net.HttpWebResponse)response).StatusDescription));             
                response.Close();

            }
            catch (Exception ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format(string.Format("Błąd wysłania powiadomienia dla materiału '{0}': {1}", publicId), ex.Message));
                return false;
            }
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Wysłano powiadomienie dla materiału {0} na adres {1} o stanie {2}", publicId, url, statusName));           
            return true;
        }       
    }


    class SNodeComparer: IComparer<Node>
    {
          public int Compare(Node x, Node y)
        {

            if (x == null)
                if (y == null)
                    return 0;
                else
                    return -1;
            else
                if (y == null)
                    return 1;
                else
                    return x.FreeSpace.Value.CompareTo(y.FreeSpace.Value);			
        }
    }
}
