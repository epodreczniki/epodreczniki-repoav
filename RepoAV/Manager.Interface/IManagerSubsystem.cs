using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;
using PSNC.Proca3.Subsystem;

namespace PSNC.RepoAV.Manager
{

    [ServiceContract]
    public interface IManagerSubsystem : ISubsystemService
    {
        [OperationContract]
        [WebGet]
        string GetId();


        [OperationContract]
        [WebGet (UriTemplate="/RecodeMaterial/{materialId}")]
        bool? RecodeMaterial(string materialId);

        [OperationContract]
        [WebGet(UriTemplate = "/RemoveFormat/{uniqueId}")]
        bool RemoveFormat(string uniqueId);
        
        [OperationContract]
        [WebGet(UriTemplate = "/RemoveMaterial/{publicId}")]
        bool RemoveMaterial(string publicId);
        
        [OperationContract]
        [WebGet(UriTemplate = "/GetMaterialStatus/{publicId}")]
        string GetMaterialStatus(string publicId);

        [OperationContract]
        [WebGet(UriTemplate = "/ProcessTasks")]
        void ProcessTasks();

        [OperationContract]
        [WebGet(UriTemplate = "/ReplicateFormat/{uniqueId}")]
        bool ReplicateFormat(string uniqueId);

        [OperationContract]
        [WebGet(UriTemplate = "/ReplicateMaterial/{publicId}")]
        bool ReplicateMaterial(string publicId);

        [OperationContract]
        [WebGet(UriTemplate = "/PushFormatToNode?uniqueid={uniqueId}&nodeid={nodeId}")]
        bool PushFormatToNode(string uniqueId, int nodeId);

        [OperationContract]
        [WebGet(UriTemplate = "/RepairReplicas")]
        bool StartReplicaRepair();

        [OperationContract]
        [WebGet(UriTemplate = "/RepairFormats")]
        bool StartFormatRepair();

    }
}
