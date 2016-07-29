using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;


namespace PSNC.Proca3.Subsystem
{
    /// <summary>
    /// Szablon klasy parametru
    /// </summary>
    /// <remarks>Typ T musi byæ serializowalny przez XMLSerializer</remarks>
//    [Serializable]
    [DataContract]
    public class Parameter<T> : ParameterBase
    {
        public delegate T GetValue(Parameter<T> sender);
        private ParameterContainer m_Dictionary;

        /// <summary>
        /// Zdarzenie wprowadzenie nowej wartoœci parametru
        /// </summary>
        public event EventHandler<NewParameterValueArgs<T>> NewValueEvent;

        /// <summary>
        /// Po wpisaniu nowej wartoœci
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Implementacja pobierania wartoœci parametru
        /// </summary>
        public GetValue GetValueMethod;

        public Parameter(string Name, string Description, T Value, bool ReadOnly, ParameterContainer Dictionary)
            : base(Name, Description, ReadOnly)
        {
            m_Dictionary = Dictionary;
            this.m_Value = Value;
        }

        public Parameter(string Name, string Description, T Value, T MinValue, T MaxValue, bool ReadOnly, ParameterContainer Dictionary)
            : base(Name, Description, ReadOnly)
        {
            m_Dictionary = Dictionary;
            if (!(Value is IComparable<T>))
            {
                throw new ArgumentException("Typ wartoœci parametru nie implementuje interfejsu IComparable");
            }

            if (((IComparable<T>)MinValue).CompareTo(MaxValue) > 0)
            {
                throw new ArgumentException("MinValue > MaxValue");
            }

            this.m_MinValue = MinValue;
            this.m_MaxValue = MaxValue;
            this.RangeCheckingEnabled = true;
            this.m_Value = Value;
        }

        public T Value
        {
            get
            {
                if (GetValueMethod != null)
                    m_Value = GetValueMethod(this);
                return (T)m_Value;
            }
            set
            {
                T newVal = value;

                if (ReadOnly)
                {
                    throw new ApplicationException("Parametr " + this.Name + " jest tylko do odczytu");
                }

                if (m_Value == null)
                {
                    if (newVal == null)
                        return;
                }
                if (m_Value!=null && m_Value.Equals(newVal))
                    return;

                if (RangeCheckingEnabled && newVal!=null && newVal is IComparable)
                {
                    if (((IComparable)newVal).CompareTo(m_MinValue) < 0
                        || ((IComparable)newVal).CompareTo(m_MaxValue) > 0)
                        throw new ApplicationException("Nowa wartoœæ parametru z poza zakresu");
                }
                if (OnNewValueEvent(ref newVal))
                {
                    this.m_Value = newVal;
                    Modified = true;
                    OnValueChanged();
                }
                else
                {
                    throw new ApplicationException("B³¹d ustawiania wartoœci paramatru");
                }
            }
        }


        public override object ObjectValue
        {
            get
            {
                return Value;
            }
            set
            {
                Value = (T)Convert.ChangeType(value, typeof(T));
            }
        }

        public ParameterContainer Dictionary
        {
            get { return m_Dictionary; }
            internal set { m_Dictionary = value; }
        }

        protected bool OnNewValueEvent(ref T newVal)
        {
            if (this.NewValueEvent != null)
            {
                NewParameterValueArgs<T> args = new NewParameterValueArgs<T>(newVal);
                NewValueEvent(this, args);
                newVal = args.NewValue;
                return !args.Cancel;
            }
            return true;
        }

        protected void OnValueChanged()
        {
            if (ValueChanged != null)
            {
                m_Dictionary.OnParameterValueChanged(this);
                ValueChanged(this, EventArgs.Empty);
            }
        }


        public static implicit operator T (Parameter<T> p)
        {
            return (T)p.ObjectValue;
        }

        override public System.String ToString()
        {
            StringBuilder sb = new StringBuilder(Name);
            sb.Append("=").Append(ObjectValue.ToString());
            return sb.ToString();
        }

        public static string GetLocalNameFromGlobalName(string GlobalName)
        {
            string[] parts = GlobalName.Split(new char[] { '.' });
            if (parts.Length == 4)
                return parts[3];
            else
                throw new ArgumentException("Podany ³añcuch znaków nie jest poprawn¹ nazw¹", GlobalName);
        }


        internal override void SetValue(object value)
        {     
            m_Value = (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Oznacza, ¿e ten parametr jest tablic¹. Domyœlnie false.
        /// </summary>
        public bool IsArray = false;
    }

    public class NewParameterValueArgs<T> : EventArgs
    {
        private T m_newValue;
        private bool m_Cancel;

        public NewParameterValueArgs()
        { }

        public NewParameterValueArgs(T newValue)
        {
            m_newValue = newValue;
        }

        public T NewValue
        {
            get { return m_newValue; }
            set { m_newValue = value; }
        }

        public bool Cancel
        {
            get { return m_Cancel; }
            set { m_Cancel = value; }
        }

    }
}
