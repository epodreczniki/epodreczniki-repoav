using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{

    internal class TaskPreferredNodes : BaseObject
    {
        internal long Id_Task { get; set; }
        internal int NodeId { get; set; }

        public TaskPreferredNodes()
            : base()
        {
        }
    }
}
