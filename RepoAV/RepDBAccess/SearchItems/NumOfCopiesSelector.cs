using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class NumOfCopiesSelector : RangeSelector
	{
		[SqlParameter]
		public int MinNumOfLocations {get; set;}


		public NumOfCopiesSelector()
			: base()
		{
			MinNumOfLocations = -1;
		}

		public NumOfCopiesSelector(int offset, int count)
			: base(offset, count)
		{
			MinNumOfLocations = -1;
		}
	}
}
