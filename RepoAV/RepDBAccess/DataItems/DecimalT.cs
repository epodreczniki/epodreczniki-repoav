using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class DecimalT : BaseObject
	{
		[SqlParameter(System.Data.SqlDbType.Decimal, Direction = System.Data.ParameterDirection.Output)]
		public decimal DecimalOut { get; set; }

		public DecimalT()
			:base()
		{
			DecimalOut = -1M;			
		}
	}

	public class DecimalTRes : BaseObject
	{
		[SqlParameter]
		public decimal DecimalVal { get; set; }

		public DecimalTRes()
			: base()
		{
			DecimalVal = -1M;
		}
	}
}
