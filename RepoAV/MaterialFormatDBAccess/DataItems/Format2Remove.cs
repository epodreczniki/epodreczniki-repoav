using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.MaterialFormatDBAccess
{
	public class Format2Remove : BaseObject
	{
		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string UniqueId {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 200)]
		public string Location {get; set;}

		[SqlParameter]
		public long Size {get; set;}

		[SqlParameter]
		public long RealSize { get; set; }

		public Format2Remove()
			:base()
		{
			Size = -1;
			RealSize = -1;
		}
	}
}
