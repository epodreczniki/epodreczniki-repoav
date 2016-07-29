using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.TaskQueue;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.SNode
{
	public enum TransferState { Init, BeforeFirstData, AfterFirstData, Completed };

	public abstract class BaseDemanTask : BaseTask
	{
		protected TransferState m_CurrentExecState;
		protected string[] m_UniqueIds;
		//protected Dictionary<string, string> m_Content;
		protected long m_RepoTaskId;
		

		public override bool CanWaitForOtherTask { get { return true; } }//okresla, czy ma sens, zeby to zadanie czekalo na inne, czy ma w ogole nie sprawdzac
		public override bool IsMainEntryTask { get { return true; } }//okresla, czy jest to zadanie inicjowane bezposrednio z zewnatrz (czyli glowne)


		protected SNodeSubsystem DemanSubsys
		{
			get { return (SNodeSubsystem)Manager.Owner; }
		}
		protected PSNC.RepoAV.MaterialFormatDBAccess.MaterialFormatDBAccess DBAccess
		{
			get { return DemanSubsys.DBAccess; }
		}
		protected PSNC.RepoAV.RepDBAccess.RepDBAccess RepoDBAccess
		{
			get { return DemanSubsys.RepoDBAccess; }
		}
		public TransferState CurrentExecState
		{
			get { return m_CurrentExecState; }
			set { m_CurrentExecState = value; }
		}
		public string[] UniqueIds
		{
			get { return m_UniqueIds; }
			set { m_UniqueIds = value; }
		}
		//public Dictionary<string, string> Content
		//{
		//	get { return m_Content; }
		//	set { m_Content = value; }
		//}
		public long RepoTaskId
		{
			get { return m_RepoTaskId; }
			set { m_RepoTaskId = value; }
		}

		public BaseDemanTask(long repoTaskId)
			: base()
		{
			CurrentExecState = TransferState.Init;
			m_RepoTaskId = repoTaskId;
		}

		public BaseDemanTask()
			: base()
		{
			CurrentExecState = TransferState.Init;
			m_RepoTaskId = -1;
		}
		protected void FinishDemanTask(int error, string errorDesc)
		{
			m_ErrorCode = error;
			if (error == (int)StandardErrors.Success)
			{
				m_ErrorDesc = errorDesc;
				State = TaskState.WaitingForFinish;
			}
			else
			{
				FinishTaskWithError(error, errorDesc);
			}
		}

		protected override void GetDetailsAfterFinished(StringBuilder sb)
		//przygotowanie opisu zadania po jego zakonczeniu
		{
			if (sb == null)
				return;

			base.GetDetailsAfterFinished(sb);

			if (m_UniqueIds != null)
				sb.AppendFormat("\r\n    RepoTaskId={1}, UniqueIds={0},", string.Join(";", m_UniqueIds), m_RepoTaskId);
			else
				sb.AppendFormat("\r\n    RepoTaskId={0}, UniqueIds=NULL,", m_RepoTaskId);

		}


		protected override void AfterFinish()
		{
			base.AfterFinish();

			if (m_RepoTaskId > -1)
			{
				try
				{
					if (m_ErrorCode == (int)ErrorType.Success)
						RepoDBAccess.SetTaskResult(m_RepoTaskId, RepDBAccess.TaskStatus.Success, m_ErrorCode.ToString() + " : " + Result);
					else
						RepoDBAccess.SetTaskResult(m_RepoTaskId, RepDBAccess.TaskStatus.Failure, m_ErrorCode.ToString() + " : " +  m_ErrorDesc);
				}
				catch (Exception ei)
				{
					Manager.ReportError(string.Format("Błąd podczas komunikacji z bazą RepoDB. SNode nie zdołał powiadomić o wynikach zadania nr {1}. [UniqueId={0}].", m_UniqueIds[0], m_RepoTaskId), ei);
				}
			}
		}
	}

}
