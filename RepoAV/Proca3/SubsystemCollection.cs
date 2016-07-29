using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using PSNC.Proca3.Subsystem;
using PSNC.Util;


namespace PSNC.Proca3
{
    class SubsystemCollection
    {
        const int TimerPeriod = 10; // 10 s

        protected static System.Timers.Timer Timer;
        protected static long TimerTick = 0;

        static List<ServiceHost> _services = new List<ServiceHost>();

        internal static void Init()
        {
            Timer = new System.Timers.Timer(TimerPeriod * 1000);
            Timer.AutoReset = false;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Enabled = true;
        }


        internal static List<ServiceHost> AllServices
        {
            get { return _services;  }
        }


        internal static async Task<bool> InitSubsytem(ISubsystemService oService)
        {
            try
            {

                string portConfiguration  = ConfigSection.GetConfiguration().Port;

                ServiceHost sh = new ServiceHost(oService);


                string baseUri = string.Format("http://localhost:{0}/{1}", portConfiguration, oService.GetName());


                Type type = oService.GetType();
                Type contractType = type.GetInterface("ISubsystemService");
                Type[] contractInterfaces = type.GetInterfaces();

                foreach (Type contractInterface in contractInterfaces)
                {
                    if (contractInterface.GetCustomAttributes(typeof(ServiceContractAttribute), true).Length > 0
                        && contractInterface != type.GetInterface("ISubsystemService"))
                    {
                        contractType = contractInterface;
                    }
                }

                ServiceEndpoint se = sh.AddServiceEndpoint(contractType, new WebHttpBinding(), baseUri);

                WebHttpBehavior behav = new WebHttpBehavior();
                behav.DefaultOutgoingResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json;
                se.Behaviors.Add(behav);

                sh.Open();

                if(sh.State == CommunicationState.Opened)
                    foreach(ServiceEndpoint ep in sh.Description.Endpoints)
                    {
                        Console.WriteLine("Uruchoniono usługę: " + ep.ListenUri.ToString());
                    }


                PSNC.Proca3.Subsystem.Subsystem subs = oService as PSNC.Proca3.Subsystem.Subsystem;
                if (subs != null)
                    await subs.StartSubsystemAsync();
                else
                    oService.OnStart();

                _services.Add(sh);

                return true;
            }
            catch(Exception ex)
            {
                Log.TraceMessage(ex, "InitSubsytem");
            }

            return false;
        }

     

        internal static async void Done()
        {
            foreach (ServiceHost sh in _services)
                if (sh.State == CommunicationState.Opened)
                {
                    ISubsystemService oService = sh.SingletonInstance as ISubsystemService;
                    if(oService != null)
                    {
                        oService.OnStop();
                    }
                    sh.Close();
                }
        }


        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
				List<Task> lstTask2Wait = new List<Task>();
                foreach (ServiceHost sh in _services)
                    if (sh.State == CommunicationState.Opened)
                    {
                        TimerTick++;
                        ISubsystemService oService = sh.SingletonInstance as ISubsystemService;
                        if (oService != null)
                        {
							if (oService.TimerActive)
								continue;

							Task t = new Task(delegate() 
														{
															lock (oService)
															{
																if (oService.TimerActive)
																	return;
																oService.TimerActive = true;
															}

															try
															{
																oService.OnTimer(TimerTick);
															}
															catch (Exception ex)
															{
																Log.TraceMessage(ex, "OnTimer dla " + (oService.GetName() ?? "NULL"));
															}
															finally
															{
																lock (oService)
																{
																	oService.TimerActive = false;
																}
															}
														});
                            lstTask2Wait.Add(t);
							t.Start();

                        }
                    }
				//if (lstTask2Wait.Count > 0)
				//	Task.WaitAll(lstTask2Wait.ToArray());
            }
            catch (Exception ex)
            {
				Log.TraceMessage(ex, "OnTimer");
            }
            finally
            {
                Timer.Enabled = true;
            }
        }


    }
}
