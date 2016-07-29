using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;

namespace PSNC.Proca3.Subsystem
{
    [InheritedExport]
    [ServiceContract]
    public interface ILocalNodeSubsystem : ISubsystemService
    {
        string SystemName { get;  }

        IEnumerable<ISubsystemService> GetAllServices();

        ISubsystemService GetSubsystem(string subsystemName);

        string GetSubsystemAddress(string subsystemName);

		string NodeId { get; }

		int NodeIdAsInt {get;}


        string GetGlobalParameter(string parameterName);

        // oeracje www
        [WebGet]
        [OperationContract]
        System.IO.Stream Config();

        [WebGet]
        [OperationContract]
        System.IO.Stream Start(string name);

        [WebGet]
        [OperationContract]
        System.IO.Stream Stop(string name);

        [WebGet]
        [OperationContract]
        System.IO.Stream Subsystem(string name);

        [OperationContract]
        [WebInvoke(Method = "POST")]
        Stream ChangeParam(Stream data);

        [WebGet]
        [OperationContract]
        System.IO.Stream Css();


		
    }
}
 