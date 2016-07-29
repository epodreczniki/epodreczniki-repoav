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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    partial class ManagerSubsystem : Subsystem, IManagerSubsystem
    {
      
        RepDBAccess.RepDBAccess m_repDbAccess;
        bool m_selectRecoders = true;
        DateTime m_LastChecking4NewTasksDate;
        bool m_DuringTaskOrdering = false;
        int m_myId = 1;
        bool m_ProcessFirstAudioOnly = false;
        int m_formatRepairInterval;
        int  m_replicaRepairInterval;
        int m_oldMaterialRemovalInterval;

        public ManagerSubsystem()
            : base()
        {           
            Parameters.AddNewParameter<int>("TaskCheckInterval", "Czas w sekundach odpytywania RepDB o zadania", 30, false);
            //Parameters.AddNewParameter<int>("FormatRepairInterval", "Czas w minutach między kolejnymi sprawdzeniami formatów z błędami", 120, false);
            //Parameters.AddNewParameter<int>("ReplicaRepairInterval", "Czas w minutach między kolejnymi sprawdzeniami replikacji", 360, false);
            //Parameters.AddNewParameter<int>("OldMaterialRemovalInterval", "Czas w godzinach między kolejnymi usuwaniami starych materiałów", 24, false);           
        }

        public string GetId()
        {
            return "ep."+Guid.NewGuid().ToString().Replace("-","");
        }

        public override string GetName()
        {
            return "Manager";
        }

        public override void OnStart()
        {
            base.OnStart();

            string cnnString = this.LocalNode.GetGlobalParameter("RepoDBConnection");
           
            m_repDbAccess = new RepDBAccess.RepDBAccess(cnnString, false);
            m_myId = Convert.ToInt16(this.LocalNode.NodeId);
            m_ProcessFirstAudioOnly = Convert.ToBoolean(m_repDbAccess.GetGlobalData("ProcessFirstAudioOnly"));
            m_formatRepairInterval = Convert.ToInt16(m_repDbAccess.GetGlobalData("FormatRepairInterval"));
            m_replicaRepairInterval = Convert.ToInt16(m_repDbAccess.GetGlobalData("ReplicaRepairInterval"));
            m_oldMaterialRemovalInterval = Convert.ToInt16(m_repDbAccess.GetGlobalData("OldMaterialRemovalInterval"));
            m_selectRecoders = Convert.ToBoolean(m_repDbAccess.GetGlobalData("SelectRecoders"));

            StartPeriodicTasks();          
        }

        void StartPeriodicTasks()
        {
            string msg = string.Empty;
            if (m_formatRepairInterval > 0)
            {
                TaskAdd taskAdd = new TaskAdd() { Type = TaskType.FixFormatErrors, BeginDate = DateTime.Now.AddMinutes(1)};
                AddTaskDB(taskAdd, out msg, true);
            }

            if (m_replicaRepairInterval > 0)
            {
                TaskAdd taskAdd = new TaskAdd() { Type = TaskType.FixReplication, BeginDate = DateTime.Now.AddMinutes(1)};
                AddTaskDB(taskAdd, out msg, true);
            }

            if (m_oldMaterialRemovalInterval > 0)
            {
                TaskAdd taskAdd = new TaskAdd() { Type = TaskType.RemoveOldMaterials, BeginDate = DateTime.Now.AddMinutes(1) };
                AddTaskDB(taskAdd, out msg, true);
            }

            TaskAdd task = new TaskAdd() { Type = TaskType.RemoveOldTasks, BeginDate = DateTime.Now.AddMinutes(1) };
            AddTaskDB(task, out msg, true);
        }

        public override void OnStop()
        {
            base.OnStop();
        }

        public override void OnTimer(long tick)
        {
           //base.OnTimer(tick);
            int interval = (int)Parameters["TaskCheckInterval"].ObjectValue;

            if (interval > -1 && (DateTime.Now - m_LastChecking4NewTasksDate).TotalSeconds >= interval)
            {
                Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, GetName(), "Okresowe pobranie zadań do wykonania.");
                ProcessTasks();
            }
        }

        public bool? RecodeMaterial(string publicId)
        {
            string msg = string.Empty;
            Material material = GetMaterial(publicId, out msg);
            if (material == null)
                return null;
            if(material.Id == -1 || material.Deleted) 
                return false;
            return (RecodeMaterial(material, out msg).HasValue);
        }
       
        // return false on error, true if operation accepted
        public bool RemoveMaterial(string publicId)
        {
            string msg = string.Empty;
            Log.TraceMessage(TraceEventType.Verbose, GetName(), string.Format("Przyjęto zlecenie usunięcia materiału '{0}'", publicId));
            Material material = GetMaterial(publicId, out msg);
            if (material == null)
                return false;
            else
                if (material.Id == -1)
                    return true;
            TaskAdd task = new TaskAdd() { PublicId = publicId, Type = TaskType.RemoveMaterial };
            return AddTaskDB(task, out msg);
        }

        public bool RemoveFormat(string uniqueId)
        {
            string msg = string.Empty;
            return RemoveFormat(uniqueId, out msg);
        }

        bool RemoveFormat(string uniqueId, out string msg)
        {
            Format format = GetFormat(uniqueId, out msg);
            if (format == null)
                return false;
            if (format.Id == -1)
                return true;
            else
                return RemoveFormat(format, false, out msg) != null;
        }

        public string GetMaterialStatus(string publicId)
        {
            bool notFound;
            MaterialStatus? status = GetMaterialStatus(publicId, out notFound);
            return status.HasValue ? status.ToString() : null;           
        }
    
        public bool ReplicateFormat(string uniqueId)
        {
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie sprawdzenia liczby kopii formatu '{0}'.", uniqueId));
            string msg = string.Empty;
            Format format = GetFormat(uniqueId, out msg);            
            if (format == null || format.Id == -1)
                return false;
            return ReplicateFormat(format) != null;
        }

        public bool ReplicateMaterial(string publicId)
        {
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie sprawdzenia liczby kopii formatów materiału '{0}'.", publicId));
            Format[] formats = m_repDbAccess.GetFormats4Material(publicId);
            if (formats == null)
                return false;
            foreach (Format format in formats)
                ReplicateFormat(format);
            return true;
        }

        public bool PushFormatToNode(string uniqueId, int nodeId)
        {
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie umieszczenia formatu '{0}' na węźle {1}", uniqueId, nodeId));
            Node snode =null;
            try
            {
                snode = m_repDbAccess.GetNode(nodeId);
            }
             catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(TraceEventType.Error, GetName(), string.Format("Błędny id węzła {0}: {1}", nodeId, ex.Message));
                return false;
            }
            string msg = string.Empty;
            Format format = GetFormat(uniqueId, out msg);
            if (format == null || format.Id == -1)
                return false;

            if (snode.FreeSpace < format.Size)
            {
                Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Zlecenie umieszczenie formatu '{0}' na węźle {1} odrzucone ze względu na brak wolnego miejsca", uniqueId, nodeId));
                return false;
            }
            PushFormat(format, new int[]{snode.Id});
            return true;
        }
       
        public bool StartReplicaRepair()
        {
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie sprawdzenie liczby replik" ));
            return StartRepair(TaskType.FixReplication);
        }

        public bool  StartFormatRepair()
        {            
            Log.TraceMessage(TraceEventType.Information, GetName(), string.Format("Przyjęto zlecenie sprawdzenia formatów z błędami"));
            return StartRepair(TaskType.FixFormatErrors);
        }
    }
    
}
