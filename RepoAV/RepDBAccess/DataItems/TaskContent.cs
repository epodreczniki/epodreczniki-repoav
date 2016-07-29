using PSNC.RepoAV.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.RepDBAccess
{
	public class TaskContent : BaseObject
	{
		public long Id_Task { get; set; }

		public string Key { get; set; }

		public string Value { get; set; }
	}
}
