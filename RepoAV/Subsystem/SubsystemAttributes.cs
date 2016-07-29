using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


namespace PSNC.Proca3.Subsystem
{
        public class SubsystemParameterAttribute : Attribute
        {
            public object DefaultValue;
            public string Description = "";
            public bool IsReadOnly = false;
			public object MinValue;
			public object MaxValue;

            public SubsystemParameterAttribute(string description)
            {
                this.Description = description;
            }
            public SubsystemParameterAttribute(string description, object defaultValue)
            {
                this.DefaultValue = defaultValue;
                this.Description = description;
            }
            public SubsystemParameterAttribute(string description, object defaultValue, bool isReadOnly)
            {
                this.DefaultValue = defaultValue;
                this.Description = description;
                this.IsReadOnly = isReadOnly;
            }
			public SubsystemParameterAttribute(string description, object defaultValue, bool isReadOnly, object min, object max)
			{
				this.DefaultValue = defaultValue;
				this.Description = description;
				this.IsReadOnly = isReadOnly;
				this.MinValue = min;
				this.MaxValue = max;
			}
        }



}
