using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.RepDBAccess
{
	public enum NodeMode : short
	{
		Run = 0,
		ReadOnly = 1,
		Disconnected = 2
	}
}
