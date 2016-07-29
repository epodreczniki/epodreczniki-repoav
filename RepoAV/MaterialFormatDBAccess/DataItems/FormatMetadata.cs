using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.MaterialFormatDBAccess
{
	public class FormatMetadata : BaseObject
	{
		[SqlParameter]
		public int Id {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string UniqueId {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 200)]
		public string Location {get; set;}

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public FormatStatus Status {get; set;}

		[SqlParameter]
		public long Size {get; set;}


		[SqlParameter]
		public long RealSize { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 50)]
		public string Mime {get; set;}

		[SqlParameter]
		public bool AllowDistribution {get; set;}

		public FormatMetadata()
			:base()
		{
			Id = -1;
			Size = -1;
			AllowDistribution = false;
			RealSize = -1;
		}
	}
}
