using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class GlobalDataItem : BaseObject
	{
		public string Key { get; set; }
		public string Value { get; set; }
		public string Description { get; set; }

		public GlobalDataItem()
			: base()
		{
			Key = null;
			Value = null;
			Description = null;
		}
	}
}
