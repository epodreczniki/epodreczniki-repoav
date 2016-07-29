using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Data;
using PSNC.Util;

namespace PSNC.Proca3.Subsystem
{
    [Serializable]
    public class ParameterContainer: IEnumerable<ParameterBase>
    {
        private bool m_AutoSave;
        private Dictionary<string, ParameterBase> m_Dictionary;
        private string m_ParameterFileName;
        private ISubsystemService m_Subsystem;

        protected string SwitchName
        {
            get
            {
                if (m_Subsystem != null)
                    return m_Subsystem.GetName();

                return "General";
            }
        }


        public ParameterContainer(ISubsystemService ss)
        {
            m_Dictionary = new Dictionary<string, ParameterBase>();
            m_AutoSave = false;
            if (ss != null)
            {
                m_ParameterFileName = ss.GetName();
                m_Subsystem = ss;
            }
            else
                m_ParameterFileName = "parameters";
        }
        
        public bool AutoSave
        {
            get { return m_AutoSave; }
            set { m_AutoSave = value; }
        }

        internal void AddParameter(ParameterBase param)
        {
            m_Dictionary.Add(param.Name, param);
        }

        private string FileName
        {
            
            get 
            {
                string path = ConfigurationManager.AppSettings["ConfigurationDirectory"] as string;
                if (path != null)
                    return System.IO.Path.Combine(path, ParameterFileName + ".xml");
                else
					return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\" + ParameterFileName + ".xml"); 
            }
        }

        public string ParameterFileName
        {
            get { return m_ParameterFileName; }
            set { m_ParameterFileName = value; }
        }

        public void Save()
        {
            Save(FileName);
        }

        public void Save(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!Directory.Exists(fi.DirectoryName))
                Directory.CreateDirectory(fi.DirectoryName);

            FileStream cfgFile = new FileStream(fileName + ".new", FileMode.Create);
            XmlTextWriter writer = new XmlTextWriter(cfgFile, new UTF8Encoding());
            try
            {
                ParameterSetInfo[] save = new ParameterSetInfo[m_Dictionary.Count];
                int i = 0;
                foreach (KeyValuePair<string, ParameterBase> kp in m_Dictionary)
                {
                    if (kp.Value.Storage == ParameterBase.StoreAt.DoNotStore)
                        continue;
                    save[i++] = new ParameterSetInfo(kp.Key, kp.Value.ObjectValue);
                }
                XmlSerializer xmlSer = new XmlSerializer(typeof(ParameterSetInfo[]), GetExtraTypes());
                writer.Indentation = 2;
                writer.Formatting = Formatting.Indented;

                xmlSer.Serialize(writer, save);

                writer.Close();
                if (File.Exists(fileName))
                {
                    File.Replace(fileName + ".new", fileName, fileName + ".bck");
                }
                else
                {
                    File.Move(fileName + ".new", fileName);
                }
            }
            catch (Exception iop)
            {
                Log.TraceMessage(iop, this.SwitchName, "B³¹d podczas zapisu parametrów do pliku " + fileName);
                throw;
            }
            finally
            {
                writer.Close();
            }

            foreach (ParameterBase pb in m_Dictionary.Values)
            {
                pb.Modified = false;
            }
        }

        private System.Type[] GetExtraTypes()
        {
            List<Type> types = new List<Type>();
            foreach (ParameterBase param in m_Dictionary.Values)
            {
                if (param.ObjectValue != null)
                {
                    if (!types.Contains(param.ObjectValue.GetType()))
                    {
                        types.Add(param.ObjectValue.GetType());
                    }
                }
            }
            return types.ToArray();
        }

        public void Load()
        {
            if (File.Exists(FileName))
                Load(FileName);
            else
                Save();
        }

        public void Load(string fileName)
        {
            XmlSerializer xmlSer = new XmlSerializer(typeof(ParameterSetInfo[]), GetExtraTypes());
            XmlReader reader = new XmlTextReader(fileName);
            try
            {
                ParameterSetInfo[] arParams = (ParameterSetInfo[])xmlSer.Deserialize(reader);
                for (int i = 0; i < arParams.Length; i++)
                    if (m_Dictionary.ContainsKey(arParams[i].Name))
                        try
                        {
                            if (m_Dictionary[arParams[i].Name].Storage == ParameterBase.StoreAt.DoNotStore)
                                continue;
                            m_Dictionary[arParams[i].Name].SetValue(arParams[i].Value??"");
                        }
                        catch (Exception ex)
                        {
                            Log.TraceMessage(ex, this.SwitchName, "B³¹d podczas odczytu wartoœci parametru " + arParams[i].Name + " z pliku " + fileName);
                        }
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, this.SwitchName, "B³¹d podczas odczytu parametrów z pliku " + fileName);
            }
            finally
            {
                reader.Close();
            }

            try
            {
                bool bSave = false;
                foreach (ParameterBase pb in m_Dictionary.Values)
                {
                    if (pb.Modified == true)
                    {
                        bSave = true;
                        break;
                    }
                }
                if (bSave)
                    Save();
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, this.SwitchName, "B³¹d podczas akcji po odczycie parametrów z pliku " + fileName);
            }

        }

        internal void ParameterChanged(ParameterBase Parameter)
        {
            if (AutoSave)
                Save(FileName);
        }

        public ParameterBase[] GetParameterBaseArray()
        {
            List<ParameterBase> resp = new List<ParameterBase>(this.m_Dictionary.Count);
            foreach (KeyValuePair<string, ParameterBase> kv in m_Dictionary)
            {
                ParameterBase param = kv.Value.GetParameterBase();
                resp.Add(param);
            }
            return resp.ToArray();
        }

        /// <summary>
        /// Pobiera listê wartoœci parametrów zmodyfikowanych
        /// </summary>
        public List<ParameterSetInfo> GetChanges()
        {
            List<ParameterSetInfo> changes = new List<ParameterSetInfo>();
            foreach (KeyValuePair<string, ParameterBase> kv in m_Dictionary)
            {
                if (kv.Value.Modified)
                {
                    changes.Add(new ParameterSetInfo(kv.Key, kv.Value.ObjectValue));
                }
            }
            return changes;
        }

        public ParameterBase[] ToArray()
        {
            ParameterBase[] items = new ParameterBase[m_Dictionary.Values.Count];
            m_Dictionary.Values.CopyTo(items, 0);
            return items;
        }

        /// <summary>
        /// Zeruje flagi zmian parametrów
        /// </summary>
        public void ResetChanges()
        {
            foreach (KeyValuePair<string, ParameterBase> kv in m_Dictionary)
            {
                kv.Value.Modified = false;
            }
        }

        public void SetValues(List<ParameterSetInfo> changes)
        {
            foreach (ParameterSetInfo info in changes)
            {
                if (m_Dictionary.ContainsKey(info.Name))
                {
                    try
                    {
                        m_Dictionary[info.Name].ObjectValue = info.Value;
                    }
                    catch (Exception ex)
                    {
                        Log.TraceMessage(ex, this.SwitchName, "B³¹d podczas ustawiania wartoœci parametru " + info.Name);
                    }
                }
                ResetChanges();
                if (AutoSave)
                    Save();
            }
        }

        public Parameter<T> AddNewParameter<T>(string Name, string Description, T Value, bool ReadOnly)
        {
            Parameter<T> param = new Parameter<T>(Name, Description, Value, ReadOnly, this);
            AddParameter(param);
            return param;
        }

        public Parameter<T> AddNewParameter<T>(string Name, string Description, T Value, T MinValue, T MaxValue, bool ReadOnly)
        {
            Parameter<T> param =new Parameter<T>(Name, Description, Value, MinValue, MaxValue, ReadOnly, this);
            AddParameter(param);
            return param;
        }


		public void RemoveParameter(ParameterBase par)
		{
			m_Dictionary.Remove(par.Name);
		}

        #region IEnumerable<ParameterBase> Members

        public IEnumerator<ParameterBase> GetEnumerator()
        {
            return m_Dictionary.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Dictionary.Values.GetEnumerator();
        }

        #endregion

        public ParameterBase this[string ParameterName]
        {
            get
            {
                ParameterBase param;
                m_Dictionary.TryGetValue(ParameterName, out param);
                return param;
            }
        }

        internal void OnParameterValueChanged(ParameterBase parameter)
        {
            //if (m_Subsystem != null && m_Subsystem.OwnerLocalNode != null)
            //    m_Subsystem.OwnerLocalNode.OnSubsystemParameterValueChanged(m_Subsystem, parameter);
        }


    }

}
