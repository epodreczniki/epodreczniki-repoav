using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class FormatSelector4IntStat : RangeSelector
	{
		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public FormatInternalStatus FormatInternalStatus {get; set;}


		public FormatSelector4IntStat()
			: base()
		{
			FormatInternalStatus = FormatInternalStatus.NotFound;
		}

		public FormatSelector4IntStat(int offset, int count)
			: base(offset, count)
		{
			FormatInternalStatus = FormatInternalStatus.NotFound;
		}
	}
}
