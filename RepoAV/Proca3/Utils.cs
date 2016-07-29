using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.Proca3
{
    public enum StartMode
    {
        Run,
        Console,
        Install,
        Uninstall,
        Help
    }

    class Utils
    {
        internal static StartMode GetStarModeFromCommandLine(string[] args)
        {
            if (args.Length == 0)
                return StartMode.Run;

            StartMode res = StartMode.Help;
            foreach(string arg in args)
            { 
                if(arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    string a = arg.Substring(1);
                    switch(a)
                    {
                        case "h": res = StartMode.Help;  break;
                        case "c": res = StartMode.Console; break;
                        case "i": res = StartMode.Install; break;
                        case "u": res = StartMode.Uninstall; break;
                    }
                }
            }
            return res;
        }


        internal static void PrintHelp()
        {
            Console.WriteLine("Proca3 - parametry wywołania:");
            Console.WriteLine("  -h - wyświtla pomoc");
            Console.WriteLine("  -c - uruchamia jako aplikację konsolową");
            Console.WriteLine("  -i - instalacja usługi");
            Console.WriteLine("  -u - odinstalowanie usługi");
        }
    }
}
