using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.DBAccess
{
	public class RangeSelector : BaseObject
	{
		public int Offset { get; set; }
		public int Count { get; set; }
		public int Total { get; set; }

		public RangeSelector()
			: this(0, 10)
		{
		}

		public RangeSelector(int offset, int count)
			: base()
		{
			Offset = offset;
			Count = count;
			Total = -1;
		}
		protected override void PrepareSignature(StringBuilder sb)
		{
			sb.AppendFormat("{0}{1}", Offset, Count);
		}

	}
}
