using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.TaskQueue
{
	public class TaskIDGenerator
	{
		public short m_NextIDPart = 0;

		public TaskIDGenerator(short init)
		{
			m_NextIDPart = init;
		}

		public long GenerateTimeBasedID()
		{
			TimeSpan ts = DateTime.Now - new DateTime(2000, 1, 1, 0, 0, 0);
			long timeID = (long)(ts.TotalDays);
			timeID = timeID << 16;
			ts = DateTime.Now - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
			timeID = timeID + (long)ts.TotalMilliseconds;
			timeID = timeID << 16;

			lock (this)
			{
				timeID = timeID + m_NextIDPart;
				if (m_NextIDPart >= short.MaxValue - 1)
					m_NextIDPart = 0;
				else
					m_NextIDPart++;
			}
			return timeID;
		}

		//generator uniwersalny, zawsze dostępny
		private static TaskIDGenerator m_UniversalGenerator = new TaskIDGenerator(0);
		public static long GenerateTimeBasedIDFromUG()
		{
			return TaskIDGenerator.m_UniversalGenerator.GenerateTimeBasedID();
		}
	}

}
