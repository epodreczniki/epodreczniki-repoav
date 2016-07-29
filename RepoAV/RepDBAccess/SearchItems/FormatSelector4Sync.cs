using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class FormatSelector4Sync : RangeSelector
	{
		[SqlParameter]
		public int Id_Node {get; set;}


		public FormatSelector4Sync()
			: base()
		{
			Id_Node = -1;
		}

		public FormatSelector4Sync(int offset, int count)
			: base(offset, count)
		{
			Id_Node = -1;
		}
	}
}
