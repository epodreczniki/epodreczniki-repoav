using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class Format : FormatMod
	{
		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public FormatStatus Status {get; set;}


		[SqlParameter]
		public DateTime CreatedDate { get; set; }


		[SqlParameter]
		public bool AllowDistribution { get; set; }

		public Format()
			:base()
		{
			Status = FormatStatus.New;			
		}
	}
}
