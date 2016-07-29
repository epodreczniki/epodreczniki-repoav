using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using PSNC.Proca3.Subsystem;
using PSNC.Util;

namespace PSNC.Proca3
{
    public partial class ProcaHost : ServiceBase
    {
        public ProcaHost()
        {
            InitializeComponent();
        }

        public void Run()
        {
            OnStart(null);
        }

        new public void Stop()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            string starttMsg = string.Format("Start systemu: {0}...", ConfigSection.GetConfiguration().Name);
            Console.WriteLine(starttMsg);
            Log.TraceMessage(starttMsg);
            SubsystemPlugins _subsystems = new SubsystemPlugins();
            AggregateCatalog _Catalog = new AggregateCatalog();
            CompositionContainer _Container = null;


            // load extension classes
            _Catalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)));
            _Catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            _Container = new CompositionContainer(_Catalog);
            _Container.ComposeParts(_subsystems);

            SubsystemCollection.Init();

            ILocalNodeSubsystem localNode = null;

            foreach (ISubsystemService oService in _subsystems.Subsystems)
            {
                if (oService is ILocalNodeSubsystem)
                    localNode = oService as ILocalNodeSubsystem;
            }

            if (localNode != null)
            {
                LocalNodeSubsytem ls = localNode as LocalNodeSubsytem;
                if (ls != null)
                {
                    ls.AllSubsystems = _subsystems.Subsystems;
                    ls._systemName = ConfigSection.GetConfiguration().Name;
                    ls._servicePort = Convert.ToInt32(ConfigSection.GetConfiguration().Port);

                    SubsystemCollection.InitSubsytem(ls);
                }
            }

            foreach (ISubsystemService oService in _subsystems.Subsystems)
            {
                if (localNode != oService)
                {
                    oService.LocalNode = localNode;
                    SubsystemCollection.InitSubsytem(oService);
                }
            }

        }

        protected override void OnStop()
        {
            SubsystemCollection.Done();
        }
    }
}
