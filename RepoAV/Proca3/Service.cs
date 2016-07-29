using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Configuration.Install;

namespace PSNC.Proca3
{


    static class Service
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            StartMode startMode = Utils.GetStarModeFromCommandLine(args);

            if(startMode == StartMode.Help)
            {
                Utils.PrintHelp();
                return;
            }
			try 
			{ 
				ProcaHost oServiceToRun = new ProcaHost();

				//// service entry point as window service
				if (startMode == StartMode.Run)
				{
					ServiceBase.Run(oServiceToRun);
				}

				// run as console app (debug)
				if (startMode == StartMode.Console)
				{
					oServiceToRun.Run();

					Console.WriteLine("Press <ENTER> to quit...");
					Console.ReadLine();

					oServiceToRun.Stop();
				}

                //// install as service
                if (startMode == StartMode.Install)
                {
                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                }

                //// uninstall
                if (startMode == StartMode.Uninstall)
                {
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                }
            }
            catch(InvalidOperationException ioex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Instalacja i dezinstalacja wymaga uruchomienia z prawami adminnistratora!");
                Console.ResetColor();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

 
        }
    }
}
