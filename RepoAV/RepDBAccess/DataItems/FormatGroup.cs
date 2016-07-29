using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class FormatGroup : BaseObject
	{
		[SqlParameter]
		public int Id {get; set;}

		[SqlParameter]
		public int MaterialId { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string PublicId { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string SubtitleId { get; set; }

		[SqlParameter (System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string SourceId { get; set; }

		[SqlParameter]
		public int AudioId { get; set; }


		public FormatGroup()
			:base()
		{
			AudioId = -1;
			Id = -1;
			MaterialId = -1;
		}
	}
}
