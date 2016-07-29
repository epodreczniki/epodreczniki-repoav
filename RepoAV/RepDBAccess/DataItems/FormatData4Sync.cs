using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class FormatData4Sync : BaseObject
	{
		[SqlParameter]
		public bool AllowDistribution { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string UniqueId {get; set;}

		[SqlParameter]
		public long Size { get; set; }

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public FormatStatus Status { get; set; }

		public FormatData4Sync()
			:base()
		{
			AllowDistribution = false;
			UniqueId = null;
			Size = -1;
			Status = FormatStatus.New;
		}

	}
}
