using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class FormatSource : BaseObject
	{
		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string UniqueId {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 500, Direction = System.Data.ParameterDirection.Output)]
		public string SourceUrl {get; set;}

		[SqlParameter(Direction = System.Data.ParameterDirection.Output)]
		public int SourceNodeId {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 50, Direction = System.Data.ParameterDirection.Output)]
		public string SourceNodeIP {get; set;}

		public FormatSource()
		{

		}
	}
}
