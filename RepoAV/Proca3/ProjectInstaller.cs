using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;


namespace PSNC.Proca3
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            this.serviceInstaller.ServiceName = ConfigSection.GetConfiguration().Name;
            this.serviceInstaller.DisplayName = "Proca 3 - " + this.serviceInstaller.ServiceName;
        }
    }
}
