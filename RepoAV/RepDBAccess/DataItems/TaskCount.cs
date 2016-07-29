using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class TaskCount : BaseObject
	{
		[SqlParameter]
		public int Id_Node { get; internal set; }

		[SqlParameter]
		public int Number { get; internal set; }
	}
}
