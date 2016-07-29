using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	internal class TaskData : BaseObject
	{
		internal long Id_Task { get; set; }
		internal string Key { get; set; }
		internal string Value { get; set; }

		public TaskData()
			:base()
		{

		}
	}
}
