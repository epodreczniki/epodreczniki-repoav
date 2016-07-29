using System.ComponentModel.Composition;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace PSNC.Proca3.Subsystem
{

    public enum SubsystemState { Stopped = 0, Started = 1, StartPending = 2, StopPending = 3, Error = 4}

    [InheritedExport]
    [ServiceContract]
    public interface ISubsystemService
    {
        SubsystemState State { get; }

		bool TimerActive { get; set;  }

        [OperationContract]
        [WebGet]
        string GetName();

        void OnStart();

        void OnStop();

        void OnTimer(long tick);

        ILocalNodeSubsystem LocalNode { get; set; }
    }
}
