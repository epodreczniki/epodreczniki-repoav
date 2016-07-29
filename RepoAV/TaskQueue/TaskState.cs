using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.TaskQueue
{
	public enum TaskState : short
	{
		Waiting = 0,
		Running = 1,
		Postponed = 2,
		Finished = 3,
		WaitingForAnswer = 4,
		WaitingForOtherTask = 5,
		Unknown = 6,
		WaitingForFinish = 7
	};
}
