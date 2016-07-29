using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;
using System.Data;

namespace PSNC.RepoAV.MaterialFormatDBAccess
{
	public class FormatAccess : BaseObject
	{
		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150, Direction = ParameterDirection.Input)]
		public string UniqueId { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 200, Direction=ParameterDirection.Output)]
		public string Location { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 200, Direction = ParameterDirection.Output)]
		public string VirtualDir { get; set; }

		[SqlParameter(Direction=ParameterDirection.Output)]
		public bool AllowDistribution { get; set; }

	}
}
