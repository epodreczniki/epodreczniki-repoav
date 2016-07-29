using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;
using PSNC.Proca3.Subsystem;

namespace PSNC.RepoAV.Recoder
{

    [ServiceContract]
    public interface IRecoderSubsytem : ISubsystemService
    {
        [OperationContract]
        [WebGet]
        string GetId();

    }
}
