using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.RepDBAccess
{
	public enum NodeRole : short
	{
		Snode = 0,
		Recoder = 1,
		Manager = 2,
		SNodeRecoder = 3,
		Other = 10
	}
}
