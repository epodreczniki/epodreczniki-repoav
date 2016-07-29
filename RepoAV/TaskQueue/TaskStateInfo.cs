using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.TaskQueue
{
	[Serializable]
	public class TaskStateInfo
	{
		protected long m_TaskID;
		protected int m_ErrorCode;
		protected string m_ErrorDesc;
		protected string[] m_AddInfo;
		protected float m_Progress;
		protected bool m_IsFinished;
		protected string m_Result;


		public long TaskID
		{
			get { return m_TaskID; }
			set { m_TaskID = value; }
		}
		public int ErrorCode
		{
			get { return m_ErrorCode; }
			set { m_ErrorCode = value; }
		}
		public string ErrorDesc
		{
			get { return m_ErrorDesc; }
			set { m_ErrorDesc = value; }
		}
		public string[] AddInfo
		{
			get { return m_AddInfo; }
			set { m_AddInfo = value; }
		}
		public float Progress
		{
			get { return m_Progress; }
			set { m_Progress = value; }
		}
		public bool IsFinished
		{
			get { return m_IsFinished; }
			set { m_IsFinished = value; }
		}
		public string Result
		{
			get { return m_Result; }
			set { m_Result = value; }
		}


		public TaskStateInfo(long taskID)
		{
			m_TaskID = taskID;
			m_ErrorCode = (int) StandardErrors.Success;
			m_ErrorDesc = "";
			m_AddInfo = null;
			m_Progress = 0;
			m_IsFinished = false;
			m_Result = "";
		}

		public TaskStateInfo()
			: this(-1)
		{
		}
	}

}
