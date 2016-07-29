using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace PSNC.Proca3.Subsystem
{
	[DataContract]
    public class ParameterSetInfo
    {
        [DataMember]
        string m_Name;
        [DataMember]
        object m_Value;

        [XmlAttribute]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public object Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public ParameterSetInfo()
        { }

        public ParameterSetInfo(string Name, object Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }
}
