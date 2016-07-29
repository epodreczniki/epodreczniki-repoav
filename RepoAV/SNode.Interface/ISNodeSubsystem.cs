using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;
using PSNC.Proca3.Subsystem;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.SNode
{
	public delegate void OnTaskFinishedDel(long taskId, ErrorType et, string errorDesc, string result, string[] addInfo, long repoTaskId, long orderingTaskId);


    [ServiceContract]
    public interface ISNodeSubsystem : ISubsystemService
    {
        [OperationContract]
        [WebGet]
        string GetId();

		ErrorType OrderInsertFormatTask(long orderingTaskId, string uniqueId, string publicId, string mime, string fileExt, string fileHash, string downloadUrl, out long taskId);

		string GetFormatLocation(string uniqueId);

		string GetFormatXmlForFile(string publicId, string location, string defaultAccessChannel, out ulong duration, out string mime);

		string GetExtension4MimeType(string mime);

		bool IsTaskStillRunning(long taskId, out bool isKnown);

		event OnTaskFinishedDel OnTaskFinished;
    }
}
