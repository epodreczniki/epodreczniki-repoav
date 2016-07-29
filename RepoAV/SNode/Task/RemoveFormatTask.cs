using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.MaterialFormatDBAccess;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.Common;
using PSNC.RepoAV.TaskQueue;
using System.Net;

namespace PSNC.RepoAV.SNode
{
	public class RemoveFormatTask : BaseDemanTask
	{
		protected bool m_ForceDelete;
		public bool ForceDelete
		{
			get { return m_ForceDelete; }
			set { m_ForceDelete = value; }
		}


		public RemoveFormatTask(long repoTaskId, string uniqueId, bool forceDelete = false)
			: base(repoTaskId)
		{
			m_ForceDelete = forceDelete;
			CurrentExecState = TransferState.Init;
			Priority = 3.0;
			m_UniqueIds = new string[] { uniqueId };
		}

		protected override void GetDetailsAfterFinished(StringBuilder sb)
		//przygotowanie opisu zadania po jego zakonczeniu
		{
			if (sb == null)
				return;

			base.GetDetailsAfterFinished(sb);
			sb.AppendFormat("\r\n    ForceDelete={0}", m_ForceDelete);
		}
		protected override bool ShouldAskingTaskWaitForMe(BaseTask askingTask)
		{
			if (askingTask is InsertFormatTask)
			{
				InsertFormatTask ift = askingTask as InsertFormatTask;

				if (ift.UniqueIds != null && m_UniqueIds != null)
				{
					if (ift.UniqueIds.Intersect(m_UniqueIds).ToArray().Length > 0)
						return true;
				}
			}
			else if (askingTask is RemoveFormatTask)
			{
				RemoveFormatTask ift = askingTask as RemoveFormatTask;

				if (ift.UniqueIds != null && m_UniqueIds != null)
				{
					if (ift.UniqueIds.Intersect(m_UniqueIds).ToArray().Length > 0)
						return true;
				}
			}

			return base.ShouldAskingTaskWaitForMe(askingTask);
		}

		protected override void DoSpecificAction()
		{
			try
			{
				Manager.ShowText(string.Format("Obsługa zadania usunięcia formatów w liczbie {0} z repozytorium [TaskId={2}, ForceDelete={1}].", m_UniqueIds.Length, m_ForceDelete, ID), System.Diagnostics.TraceEventType.Verbose);

				if (m_RepoTaskId > -1)
					DemanSubsys.RepoDBAccess.UpdateTaskLastActivityDate(m_RepoTaskId);


				foreach(string uniqueId in m_UniqueIds)
				{
					string errorDesc;
					if (!DemanSubsys.RemoveFormat(uniqueId, m_ForceDelete, out errorDesc))
					{
						if (CodeOfError == (int)ErrorType.Success)
						{
							CodeOfError = (int)ErrorType.FileDeleteFailed;
							ErrorDesc = errorDesc;
						}
					}
				}

				//RepoDBAccess.SetTaskResult(m_RepoTaskId, (CodeOfError == (int)ErrorType.Success) ? RepDBAccess.TaskStatus.Success : RepDBAccess.TaskStatus.Failure, ErrorDesc ?? "");

				State = TaskState.WaitingForFinish;
			}
			catch (Exception ex)
			{
				FinishTaskWithError(ex);
			}
		}
	}
}
