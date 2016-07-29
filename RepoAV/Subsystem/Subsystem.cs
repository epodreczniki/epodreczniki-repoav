using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using PSNC.Util;


namespace PSNC.Proca3.Subsystem
{
    public abstract class Subsystem : ISubsystemService
    {
        private ParameterContainer m_Parameters;
        private SubsystemState m_State;

        public ParameterContainer Parameters
        {
            get { return m_Parameters; }
        }

        public bool Started
        {
            get { return m_State == SubsystemState.Started; }
        }

		public bool TimerActive { get; set; }

        public ILocalNodeSubsystem LocalNode { get; set; }

        public Subsystem()
        {
            m_Parameters = new ParameterContainer(this);
            m_Parameters.AutoSave = true;
            m_State = SubsystemState.Stopped;

			TimerActive = false;

            RegisterParameters();
        }

        public SubsystemState State {
            get { return m_State;  }
        }


        public abstract string GetName();

        public virtual void OnStart()
        {
            this.Parameters.Load();

            Log.TraceMessage(TraceEventType.Start, GetName(), "Subsystem started");

            Console.WriteLine("OnStart " + GetName());
        }

        public virtual void OnStop()
        {
            this.Parameters.Save();

            Log.TraceMessage(TraceEventType.Stop, GetName(), "Subsystem stopped");
            Console.WriteLine("OnStop " + GetName());
        }

        public virtual void OnTimer(long tick)
        {
        }


        public async Task StartSubsystemAsync()
        {
            await Task.Run(
                () =>
                {
                    m_State = SubsystemState.StartPending;

                    try
                    {
                        this.OnStart();
                        m_State = SubsystemState.Started;
                    }
                    catch (Exception ex)
                    {
                        Log.TraceMessage(ex, "StartSubsystemAsync " + this.GetName());
                        m_State = SubsystemState.Error;
                    }
                });
        }


        public async Task StopSubsystemAsync()
        {
            await Task.Run(
                () =>
                {
                    m_State = SubsystemState.StopPending;

                    try
                    {
                        this.OnStop();
                        m_State = SubsystemState.Stopped;
                    }
                    catch (Exception ex)
                    {
                        Log.TraceMessage(ex, "StartSubsystemAsync " + this.GetName());
                        m_State = SubsystemState.Error;
                    }
                });
        }

        protected void RegisterParameters()
        {
            Type t = GetType();
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                SubsystemParameterAttribute[] attr = (SubsystemParameterAttribute[])f.GetCustomAttributes(typeof(SubsystemParameterAttribute), true);
                if (attr != null && attr.Length != 0)
                {
                    try
                    {
                        ParameterBase p = (ParameterBase)Activator.CreateInstance(f.FieldType, new object[] { f.Name.StartsWith("m_") ? f.Name.Substring(2) : f.Name,
																						attr[0].Description,
																						attr[0].DefaultValue,
																						attr[0].IsReadOnly,
																						Parameters});


                        
                        f.SetValue(this, p);
                        if (attr[0].MinValue != null)
                            p.ObjectMinValue = attr[0].MinValue;
                        if (attr[0].MaxValue != null)
                            p.ObjectMaxValue = attr[0].MaxValue;

                        Parameters.AddParameter(p);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Błąd podczas rejetrowania parametru {0}: {1}", f.Name, ex.Message), ex);
                    }
                }
            }
        }

    }
}
