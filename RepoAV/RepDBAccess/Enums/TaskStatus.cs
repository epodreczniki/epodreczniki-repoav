using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.RepDBAccess
{
	public enum TaskStatus
	{
		New = 0,
		Executing = 5,
		Success = 10,
		Failure = 66
	}
}
