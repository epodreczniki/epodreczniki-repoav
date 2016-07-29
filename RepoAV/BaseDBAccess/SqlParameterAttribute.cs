using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace PSNC.RepoAV.DBAccess
{
	[AttributeUsageAttribute(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class SqlParameterAttribute : Attribute
	{
		public bool TreatMinusOneAsNull { get; set; }

		public SqlDbType? ProperSQLDBType { get; set; }

		public ParameterDirection Direction { get; set; }

		public string ArrayElementsSeparator { get; set; }

		public short MaxLength { get; set; }

		public SqlParameterAttribute()
		{
			TreatMinusOneAsNull = true;
			ProperSQLDBType = null;
			Direction = ParameterDirection.Input;
			ArrayElementsSeparator = ";";
			MaxLength = -2;//undefined (a -1 oznacza MAX)
		}

		public SqlParameterAttribute(SqlDbType properSQLDBType)
		{
			TreatMinusOneAsNull = true;
			ProperSQLDBType = properSQLDBType;
			Direction = ParameterDirection.Input;
			ArrayElementsSeparator = ";";
			MaxLength = -2;//undefined (a -1 oznacza MAX)
		}
	}

}
