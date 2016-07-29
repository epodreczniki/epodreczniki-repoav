using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.TaskQueue
{
	public enum StandardErrors : int
	{
		Success = 1,
		InvalidParameter = -1,
		ExecutionTimeout = -2,				
		SystemOverloaded = -5,

		Runtime = -6,

		General = -9
	}
}
