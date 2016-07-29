using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using System.Configuration;
using PSNC.Proca3.Subsystem;
using PSNC.Util;
using System.Data.SqlClient;
using System.Data;

namespace PSNC.Proca3
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class LocalNodeSubsytem : ILocalNodeSubsystem
    {
        private ParameterContainer m_Parameters;

        internal List<ISubsystemService> AllSubsystems { get; set; }
        internal string _systemName;
        internal int _servicePort;
        private SubsystemState m_State;
		DateTime m_LastChecktimeSending = DateTime.MinValue;
		bool m_SendChecktime = false;

        public SubsystemState State
        {
            get { return m_State; }
        }

		public bool TimerActive { get; set; }

        public string SystemName
        {
            get { return this._systemName;  } 
        }

        public int ServicePort
        {
            get { return this._servicePort; }
        }

		public string NodeId
		{
			get { return this.LocalNode.GetGlobalParameter("NodeId"); }
		}

		public int NodeIdAsInt
		{
			get 
			{
				string tmp = this.LocalNode.GetGlobalParameter("NodeId");
				if (!string.IsNullOrEmpty(tmp))
					return int.Parse(tmp);

				return -1;
			}
		}

        public LocalNodeSubsytem()
        {
            this.AllSubsystems = null;
            this._systemName = string.Empty;
            this._servicePort = 0;
            this.m_State = SubsystemState.Stopped;

			TimerActive = false;

            m_Parameters = new ParameterContainer(this);
            m_Parameters.AutoSave = true;

            string gpValue = ConfigurationManager.AppSettings["GlobalParameters"];
            if (!string.IsNullOrEmpty(gpValue))
            {
                string[] globalParameters = gpValue.Split(',');
                foreach (string paramName in globalParameters)
                    m_Parameters.AddNewParameter<string>(paramName, "Parametr " + paramName, "", true);
            }


        }

        public string GetName()
        {
            return "LocalNode";
        }


        public IEnumerable<ISubsystemService> GetAllServices()
        {
            return this.AllSubsystems;
        }

        public void OnStart()
        {
			m_State = SubsystemState.StartPending;

            Console.ForegroundColor  = ConsoleColor.Green;
            Console.WriteLine(string.Format("Konfiguracja: http://localhost:{0}/Localnode/Config", _servicePort));
            Console.ResetColor();

            m_Parameters.Load();

			m_SendChecktime = false;
			string cnnString = GetGlobalParameter("StateUpdateDBConnection");
			if (!string.IsNullOrEmpty(cnnString))
			{
				cnnString = GetGlobalParameter("StateUpdateProcedure");
				m_SendChecktime = !string.IsNullOrEmpty(cnnString);
			}

			m_State = SubsystemState.Started;
        }

        public void OnStop()
        {
            m_Parameters.Save();
        }

        public void OnTimer(long tick)
        {
			if (m_SendChecktime && (DateTime.Now - m_LastChecktimeSending).TotalSeconds >= 30)
			{
				m_LastChecktimeSending = DateTime.Now;

				bool allStarted = true;
				foreach(var ss in this.AllSubsystems)
				{
					if (ss.State != SubsystemState.Started)
					{
						allStarted = false;
						break;
					}
				}

				if (allStarted == true)
					SendChecktime(GetGlobalParameter("StateUpdateProcedure"), GetGlobalParameter("StateUpdateDBConnection"));

				m_LastChecktimeSending = DateTime.Now;
			}
        }

		void SendChecktime(string storedProcedureName, string cs)
		{
			try
			{
                Log.TraceMessage("SendChecktime");

				using (SqlConnection conn = new SqlConnection(cs))
				{
					SqlCommand comm = new SqlCommand();
					comm.CommandType = CommandType.StoredProcedure;
					comm.Connection = conn;

					conn.Open();

					comm.CommandText = storedProcedureName;
					comm.CommandTimeout = 30;

					SqlParameter par = comm.CreateParameter();
					par.Direction = ParameterDirection.Input;
					par.ParameterName = "@Id_Node";
					par.SqlDbType = SqlDbType.Int;
					par.Value = NodeId;
					comm.Parameters.Add(par);

					comm.ExecuteNonQuery();

					conn.Close();
				}
			}
			catch(Exception ex)
			{
				Log.TraceMessage(ex, "Błąd podczas okresowego potwierdzenia bycia online.");
			}

		}


        public ILocalNodeSubsystem LocalNode 
        {
            get { return this;  }
            set {} 
        }


        public ISubsystemService GetSubsystem(string subsystemName)
        {
            if(this.AllSubsystems == null)
                return null;

            return (from s in this.AllSubsystems where s.GetName() == subsystemName select s).FirstOrDefault();
        }


        public string GetSubsystemAddress(string subsystemName)
        {
            ISubsystemService service = this.GetSubsystem(subsystemName);

            if (service == null)
                return null;

            return string.Format("http://localhost:{0}/{1}", this.ServicePort, service.GetName());
        }

        public string GetGlobalParameter(string parameterName)
        {
			if (m_Parameters.FirstOrDefault(p => p.Name ==parameterName) != null)
				return m_Parameters[parameterName].ObjectValue as string;
			return null;
        }


        public Stream Config()
        {
            return ReturnConfigPage();
        }

        public Stream Subsystem(string name)
        {
            return ReturnSubsystemPage(name);
        }

        public Stream Start(string name)
        {
			
            try
            {
                ISubsystemService srv = this.GetSubsystem(name);
                if (srv != null)
                {
                    Subsystem.Subsystem subs = this.GetSubsystem(name) as Subsystem.Subsystem;
					if (subs != null)
						Task.Run(() => subs.StartSubsystemAsync());
					else
						srv.OnStart();
                }
				m_State = SubsystemState.Stopped;
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex);
            }
            return RedirectToConfigPage();
        }

        public Stream Stop(string name)
        {
            try
            {
                ISubsystemService srv = this.GetSubsystem(name);
                if (srv != null)
                {
                    Subsystem.Subsystem subs = this.GetSubsystem(name) as Subsystem.Subsystem;
                    if (subs != null)
                        Task.Run(() => subs.StopSubsystemAsync());
                    else
                        srv.OnStop();
                }
            }
            catch(Exception ex)
            {
                Log.TraceMessage(ex);
            }
            return RedirectToConfigPage();
        }

        public Stream Css()
        {
            string result = string.Empty;
            Assembly _assembly = Assembly.GetExecutingAssembly();
            using (StreamReader _textStreamReader = new StreamReader(_assembly.GetManifestResourceStream("PSNC.Proca3.Resources.procastyle.css")))
            {

                result = _textStreamReader.ReadToEnd();
            }

            byte[] resultBytes = Encoding.UTF8.GetBytes(result);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/css";
            return new MemoryStream(resultBytes);
        }



        public Stream  ChangeParam(Stream request)
        {
            string name = string.Empty;
            string param = string.Empty;
            string val = string.Empty;

            using (StreamReader reader = new StreamReader(request))
            {
                // Read StreamReader data as string
                List<string> parameters = reader.ReadToEnd().Split('&').ToList();

                foreach(string p in parameters)
                {
                    string[] pv = p.Split('=');
                    if(pv.Length > 1)
                    {
                        if(pv[0] == "subsystem")
                            name = pv[1];
                        if (pv[0] == "val")
                            val = pv[1];
                        if (pv[0] == "param")
                            param = pv[1];
                    }
                }

                ISubsystemService srv = this.GetSubsystem(name);
                if(srv != null)
                {
                    PSNC.Proca3.Subsystem.Subsystem subs = srv as PSNC.Proca3.Subsystem.Subsystem;
                    if(subs != null)
                    {
                        ParameterSetInfo info = new ParameterSetInfo(param, val);

                        subs.Parameters.SetValues(new List<ParameterSetInfo>() { info });
                    }
                }

            }

            return ReturnSubsystemPage(name);
        }


        private Stream ReturnConfigPage()
        {
            string result = string.Empty;
            Assembly _assembly = Assembly.GetExecutingAssembly();
            using (StreamReader _textStreamReader = new StreamReader(_assembly.GetManifestResourceStream("PSNC.Proca3.Resources.Config.html")))
            {

                result = _textStreamReader.ReadToEnd();
                result = result.Replace("[##System##]", this.SystemName);

                string subsystems = string.Empty;
                foreach (ISubsystemService srv in this.AllSubsystems)
                {
                    string startstop = string.Empty;
                    if ((srv as PSNC.Proca3.Subsystem.Subsystem) != null)
                    {
                        PSNC.Proca3.Subsystem.Subsystem subs = srv as PSNC.Proca3.Subsystem.Subsystem;

                        if (subs.State == SubsystemState.StartPending)
                            startstop = "startowanie...";
                        else if (subs.State == SubsystemState.StopPending)
                            startstop = "zatrzymywanie...";
                        else
                        {
                            string operation = subs.Started ? "Stop" : "Start";
                            startstop = string.Format("<a href='/LocalNode/{1}?name={0}' class='btn1 btn-primary btn-xs'>{1}</a>", srv.GetName(), operation);
                        }
                    }
                    string subsytem_addr = string.Format("/LocalNode/Subsystem?name={0}", srv.GetName());
                    subsystems += Environment.NewLine + string.Format("<tr><td><a href='{3}'>{0}</a></td><td>{1}</td><td>{2}</td></tr>", srv.GetName(), srv.LocalNode.GetSubsystemAddress(srv.GetName()), startstop, subsytem_addr);
                }

                result = result.Replace("[##Subsystems##]", subsystems);

            }

            byte[] resultBytes = Encoding.UTF8.GetBytes(result);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return new MemoryStream(resultBytes);
        }


        private string  ReturnSubsystemPageString(string name)
        {
            string result = string.Empty;
            Assembly _assembly = Assembly.GetExecutingAssembly();
            using (StreamReader _textStreamReader = new StreamReader(_assembly.GetManifestResourceStream("PSNC.Proca3.Resources.Subsystem.html")))
            {
                result = _textStreamReader.ReadToEnd();
                result = result.Replace("[##Subsystem##]", name);

                string parameters = string.Empty;
                ISubsystemService srv = this.GetSubsystem(name);
                PSNC.Proca3.Subsystem.Subsystem subs = srv as PSNC.Proca3.Subsystem.Subsystem;
                if (subs != null)
                {
                    foreach (ParameterBase param in subs.Parameters)
                    {
                        
                        parameters += Environment.NewLine + string.Format("<tr><td>{0}</td><td>", param.Name)
                            + string.Format("<form action='/LocalNode/ChangeParam' method='post'><input name='val' type='text' value='{1}' class='form-control pull-left' {2}/>", name, param.ObjectValue, param.ReadOnly ? "readonly" : "")
                            + string.Format(" <input name='subsystem'  type='hidden' value='{0}'/><input name='param'  type='hidden' value='{1}'/>", name, param.Name)
                            + string.Format("<button type='submit' class='btn btn-primary pull-right' {0}>Zapisz</button>", param.ReadOnly ? "disabled='disabled'" : "")
                            + "</form>"
                            + "</td></tr>";
                    }

                }

                result = result.Replace("[##Parameters##]", parameters);
            }
            return result;
        }

        private Stream ReturnSubsystemPage(string name)
        {
            string result = ReturnSubsystemPageString(name);
            byte[] resultBytes = Encoding.UTF8.GetBytes(result);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return new MemoryStream(resultBytes);
        }

        private Stream RedirectToConfigPage()
        {
            string redirectTo = "Config";
            string result = "";
            byte[] resultBytes = Encoding.UTF8.GetBytes(result);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.TemporaryRedirect;
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Location", redirectTo);

            return new MemoryStream(resultBytes);

        }
    }
}
