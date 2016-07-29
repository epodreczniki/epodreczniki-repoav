using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PSNC.RepoAV.TaskQueue
{
	public class TaskThreadInfo
	{
		public Thread m_TaskThread;
		public bool m_ShouldWork;
		public DateTime m_StartRuningTask;
		public BaseTask m_CurrentTask;

		public TaskThreadInfo()
		{
			m_StartRuningTask = DateTime.MaxValue;
			m_CurrentTask = null;
		}
	}
}
