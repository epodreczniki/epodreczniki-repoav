using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.TaskQueue;

namespace PSNC.RepoAV.Recoder
{
	public class UniversalTaskStateInfo : TaskStateInfo
	{
		protected string m_XmlTaskName;
		protected string m_CurrentOperation;
		protected SingleOperationResult[] m_OperationsPassed;
		protected bool m_WasCancelled;



		public bool WasCancelled
		{
			set { m_WasCancelled = value; }
			get { return m_WasCancelled; }
		}
		public string XmlTaskName
		{
			set { m_XmlTaskName = value; }
			get { return m_XmlTaskName; }
		}
		public string CurrentOperation
		{
			set { m_CurrentOperation = value; }
			get { return m_CurrentOperation; }
		}
		public SingleOperationResult[] OperationsPassed
		{
			set { m_OperationsPassed = value; }
			get { return m_OperationsPassed; }
		}

		public UniversalTaskStateInfo(long taskID, string name)
			: base(taskID)
		{
			m_XmlTaskName = name;
			m_CurrentOperation = "";
			m_OperationsPassed = null;
			m_WasCancelled = false;
		}


		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Zadanie nr {3}, typu '{0}', aktualna operacja: '{1}', anulowane:'{2}', progress:{4}.\r\n", m_XmlTaskName, m_CurrentOperation, m_WasCancelled, m_TaskID, m_Progress);
			sb.Append(" Lista wykonanych operacji:\r\n");
			foreach (var so in m_OperationsPassed)
				sb.AppendFormat("{0}\r\n", so.ToString());
			return sb.ToString();
		}
	}

	public class SingleOperationResult
	{
		protected string m_Name;
		protected bool m_Success;
		protected string m_Comments;


		public string Comments
		{
			set { m_Comments = value; }
			get { return m_Comments; }
		}
		public string Name
		{
			set { m_Name = value; }
			get { return m_Name; }
		}
		public bool Success
		{
			set { m_Success = value; }
			get { return m_Success; }
		}


		public SingleOperationResult()
			: this("")
		{
		}

		public SingleOperationResult(string name)
		{
			m_Name = name;
			m_Success = false;
		}

		public override string ToString()
		{
			return string.Format(" Operation '{0}', Result='{1}', Comment='{2}'.", m_Name, m_Success ? "SUCCESS" : "FAILURE", m_Comments);
		}
	}

}
