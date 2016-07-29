using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace PSNC.Proca3.Subsystem
{

    /// <summary>
    /// Klasa bazowa parametru
    /// </summary>
    [DataContract]
    public class ParameterBase
    {
        public enum StoreAt { LocalContainer = 0, GlobalContainer, DoNotStore };

        [DataMember]
        private string m_Name;
        [DataMember]
        private StoreAt m_Storage = StoreAt.GlobalContainer;
        [DataMember]
        private string m_Description;

        [DataMember]
        private bool m_RangeChecking = false;

        [DataMember]
        private bool m_Modified = false;
        [DataMember]
        private bool m_ReadOnly = false;

        protected object m_Value;
        [DataMember]
        protected object m_MinValue;
        [DataMember]
        protected object m_MaxValue;


        public ParameterBase()
        {
        }

        public ParameterBase(string Name, string Description, bool ReadOnly)
        {
            m_Name = Name;
            m_Description = Description;
            m_ReadOnly = ReadOnly;
        }

        public StoreAt Storage
        {
            get { return m_Storage; }
            set { m_Storage = value; }
        }
        public string Name
        {
            get { return m_Name; }
        }

        public string Description
        {
            get { return m_Description; }
        }

        public object ObjectMinValue
        {
            get
            {
                return m_MinValue;
            }
			set
			{
				m_MinValue = value;
			}
        }

        public object ObjectMaxValue
        {
            get
            {
                return m_MaxValue;
            }
			set
			{
				m_MaxValue = value;
			}
        }

        [DataMember]
        public virtual object ObjectValue
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (m_Value != value)
                    Modified = true;
                m_Value = value;
            }
        }

        public bool Modified
        {
            get { return m_Modified; }
            set { m_Modified = value; }
        }

        public bool RangeCheckingEnabled
        {
            get { return m_RangeChecking; }
            protected set { m_RangeChecking = value; }
        }

        public  bool ReadOnly
        {
            get { return m_ReadOnly; }
            protected set { m_ReadOnly = value; }
        }

        public ParameterBase GetParameterBase()
        {
            ParameterBase p = new ParameterBase(Name, Description, ReadOnly);
            p.m_MinValue = m_MinValue;
            p.m_MaxValue = m_MaxValue;
            p.m_Value = ObjectValue;
            p.RangeCheckingEnabled = RangeCheckingEnabled;
            p.Storage = Storage;

            return p;
        }  

        internal virtual void SetValue(object value)
        {
            m_Value = value;
        }
    }
}
