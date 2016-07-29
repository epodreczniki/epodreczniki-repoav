using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.ServiceProcess;
using PSNC.Proca3.Subsystem;

namespace PSNC.Proca3  
{
    class SubsystemPlugins
    {
        [ImportMany(typeof(ISubsystemService))]
        public List<ISubsystemService> Subsystems
        {
            get;
            set;
        }
    }
}
