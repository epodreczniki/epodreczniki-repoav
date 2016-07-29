using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;

namespace PSNC.Proca3.Subsystem
{
    [DataContract]
    public class ChangeParamRequest
    {
        public string name { get; set; }
        public string key { get; set; }
    }
}
