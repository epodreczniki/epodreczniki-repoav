using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PSNC.RepoAV.TaskQueue
{
	public class ChildExecResult
	{
		public long m_ChildID;
		public int m_ErrorCode;
		public string m_ErrorDesc;
		public string[] m_AddInfo;
		public bool m_isPostponed;
		public string m_Result;

		public ChildExecResult()
		{
			m_ErrorCode = (int)StandardErrors.Success;
			m_isPostponed = false;
		}
	}
}
