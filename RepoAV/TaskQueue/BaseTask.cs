using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace PSNC.RepoAV.TaskQueue
{
	public abstract class BaseTask
	{
		[NonSerialized]
		protected internal TaskQueueManager m_ManagerObj;

		protected long m_ID;
		protected double m_Priority;
		protected string m_ErrorDesc;//opis bledu podczas wykonania zadania
		protected int m_ErrorCode;
		protected TaskState m_CurrentState;
		protected string m_Signature = "";
		private DateTime m_SuspendedTime;//czas przejscia w stan oczekiwania na inne zadanie
		private DateTime m_StartWaitingTime;//czas rozpoczecia oczekiwania na zadanie potomne
		protected List<string> m_AddInfo;//dodatkowe info o sposobie wykonania zadania
		protected DateTime m_HandlingStarted;//data rozpoczęcia wykonywania (pierwszego podejscia)
		protected DateTime m_OrygBeginDate;//data rozpoczęcia zadania (zgłoszona) - faktyczna moze byc pozniej zmieniona (np. Postpone)
		protected DateTime m_FinishDate;//data zakończenia obsługi zadania
		protected long m_OtherTaskID;//dentyfikator zadania, na które to zadanie czeka
		protected long m_ParentID = -1;//zadanie nadrzedne
		protected DateTime m_BeginDate;//żądana data rozpoczęcia wykonania
		protected float m_Progress;
		protected DateTime m_CreationDate;//data zgłośzenia żądania (utworzenia obiektu zadania)
		protected List<long> m_OtherTasksWaitingForMe;//identyfikatory innych zadań oczekujących na to zadanie
		protected bool m_GoPostponedAfterLastChild;
		protected DateTime m_PostponedUntil;
		protected List<long> m_ChildrenIDs;
		protected bool m_FinishAfterLastChild;
		private DateTime m_LastExecutionThreadAssignmentDate;//data ostatniego przydzielenie wątka wykonawczego
		private DateTime m_LastExecutionThreadLeftDate;//data ostatniego oddania wątka wykonawczego
		private double m_TotalRunningTime = 0.0;
		private double m_TotalWaitingTime = 0.0;
		protected string m_Result; // opis rezultatu wykonania zadania (bedzie wysłany do parenta i tam zapamietany jako jeden z elementow wpisu w m_ChildResults) 
		private int m_ExecThreadOwningCount;//licznik okreslający ile ray zadanie miało przydzielony wątek wykonawczy
		protected float m_LastCheckingProgress = 0; // progress podczas ostaniego sprawdzenia, czy zadanie nie wykonuje sie zbyt długo
		protected int m_ExecutionTimeoutAlternate;//osobny timeout dla wykonania zadania - domyślnie ma wartość Int.Max i w takim wypadku znaczenie ma 
		protected bool m_OwnsExecutingThread = false; // flaga usdtawiona na true gdy zadanie jest wykonywane przez wątek robotniczek

		[NonSerialized]
		protected Dictionary<long, ChildExecResult> m_ChildResults; //zebrane informacje od zadan potomnych po ich zakończeniu



		public long ID { get { return m_ID; } set { m_ID = value; } }
		public double Priority { get { return m_Priority; } set { m_Priority = value; } }
		public bool OwnsExecutingThread { get { return m_OwnsExecutingThread; } }
		public string ErrorDesc { get { return m_ErrorDesc; } set { m_ErrorDesc = value; } }
		public virtual TaskState State
		{
			get { return m_CurrentState; }
			set
			{
				m_CurrentState = value;
				if (m_CurrentState == TaskState.WaitingForOtherTask)
					m_SuspendedTime = DateTime.Now;
				else if (m_CurrentState == TaskState.WaitingForAnswer)
					m_StartWaitingTime = DateTime.Now;
				else if (m_CurrentState == TaskState.Finished)
					m_Progress = 100;
				else if (m_CurrentState == TaskState.Waiting || m_CurrentState == TaskState.Running)
					m_GoPostponedAfterLastChild = false;
			}
		}
		public string[] AddInfo { get { return m_AddInfo.ToArray(); } set { m_AddInfo.Clear(); m_AddInfo.AddRange(value); } }
		public long ParentID { get { return m_ParentID; } set { m_ParentID = value; } }
		public DateTime BeginDate { get { return m_BeginDate; } set { m_BeginDate = value; } }
		protected internal long OtherTaskID
		{
			get { return m_OtherTaskID; }
			set
			{
				m_OtherTaskID = value;
			}
		}
		protected internal long[] OtherTasksWaitingForMe
		{
			get
			{
				lock (m_OtherTasksWaitingForMe)
					return m_OtherTasksWaitingForMe.ToArray();
			}
		}
		public string Type
		{
			get { return GetType().Name; }
		}
		public float Progress
		{
			get { return m_Progress; }
			//set { m_Progress = value; }
		}
		public int CodeOfError { get { return m_ErrorCode; } set { m_ErrorCode = value; } }
		protected internal DateTime CreationDate { get { return m_CreationDate; } set { m_CreationDate = value; } }
		public DateTime FinishDate { get { return m_FinishDate; } set { m_FinishDate = value; } }
		protected internal DateTime OrygBeginDate { get { return m_OrygBeginDate; } set { m_OrygBeginDate = value; } }
		protected internal int NumOfChilds
		{
			get
			{
				lock (m_ChildrenIDs)
					return m_ChildrenIDs.Count;
			}
		}
		protected internal long[] ChildrenIDs
		{
			get
			{
				lock (m_ChildrenIDs)
					return m_ChildrenIDs.ToArray();
			}
		}
		protected internal int ChildrenResultsCount
		{
			get
			{
				lock (m_ChildResults)
					return m_ChildResults.Count;
			}
		}
		protected internal double TotalRunningTime
		{
			get
			{
				return m_TotalRunningTime;
			}
			set
			{
				m_TotalRunningTime = value;
			}
		}
		protected internal double TotalWaitingTime
		{
			get
			{
				return m_TotalWaitingTime;
			}
		}
		public string Result
		{
			get
			{
				return m_Result;
			}
			set
			{
				m_Result = value;
			}
		}
		protected internal int ExecThreadOwningCount
		{
			get
			{
				return m_ExecThreadOwningCount;
			}
			set
			{
				m_ExecThreadOwningCount = value;
			}
		}
		protected internal DateTime PostponedUntil { get { return m_PostponedUntil; } }
		protected internal DateTime StartWaitingTime { get { return m_StartWaitingTime; } }
		protected internal DateTime SuspendedTime { get { return m_SuspendedTime; } }
		protected internal int ExecutionTimeoutAlternate
		{
			get
			{
				return m_ExecutionTimeoutAlternate;
			}
		}

		internal DateTime LastExecutionThreadAssignmentDate { get { return m_LastExecutionThreadAssignmentDate; } set { m_LastExecutionThreadAssignmentDate = value; } }
		internal DateTime LastExecutionThreadLeftDate { get { return m_LastExecutionThreadLeftDate; } set { m_LastExecutionThreadLeftDate = value; } }
		protected internal DateTime HandlingStarted { get { return m_HandlingStarted; } set { m_HandlingStarted = value; } }

		public virtual bool CanWaitForOtherTask { get { return false; } }//okresla, czy ma sens, zeby to zadanie czekalo na inne, czy ma w ogole nie sprawdzac
		public virtual bool IsMainEntryTask { get { return false; } }//okresla, czy jest to zadanie inicjowane bezposrednio z zewnatrz (czyli glowne)

		protected TaskQueueManager Manager { get { return m_ManagerObj; } }

		public BaseTask()
		{
			m_ID = -1;
			m_Priority = 1.0;
			State = TaskState.Waiting;
			m_AddInfo = new List<string>();
			m_HandlingStarted = DateTime.MinValue;
			m_OtherTaskID = -1;
			m_ParentID = -1;
			m_BeginDate = DateTime.Now;
			m_OrygBeginDate = m_BeginDate;
			m_Progress = 0;
			m_ErrorDesc = "";
			m_ErrorCode = (int)StandardErrors.Success;
			m_CreationDate = DateTime.Now;
			m_OtherTasksWaitingForMe = new List<long>();
			m_FinishDate = DateTime.MaxValue;
			m_GoPostponedAfterLastChild = false;
			m_PostponedUntil = DateTime.MaxValue;
			m_ChildrenIDs = new List<long>();
			m_ChildResults = new Dictionary<long, ChildExecResult>();
			m_FinishAfterLastChild = true;
			m_TotalRunningTime = 0.0;
			m_ExecThreadOwningCount = 0;
			m_LastExecutionThreadAssignmentDate = DateTime.MaxValue;
			m_LastExecutionThreadLeftDate = DateTime.MaxValue;
			m_ExecutionTimeoutAlternate = -1;
		}
		public BaseTask(DateTime dtStart)
			: this()
		{
			m_BeginDate = dtStart;
			m_OrygBeginDate = dtStart;
		}

		public BaseTask(DateTime dtStart, long parentTaskID)
			: this(dtStart)
		{
			m_ParentID = parentTaskID;
		}

		public BaseTask(long parentTaskID)
			: this()
		{
			m_ParentID = parentTaskID;
		}

		protected BaseTask AddChildTask2LocalQueue(BaseTask childTask)
		{
			if (childTask == null)
				return null;


			BaseTask child = Manager.AddTask(childTask);
			if (child != null)
				AddChild(childTask.ID);

			return child;
		}

		protected BaseTask AddChildTask2LocalQueue(BaseTask childTask, out string errorMsg, out int errorId)
		{
			errorMsg = "";
			errorId = (int)StandardErrors.Success;

			if (childTask == null)
			{
				errorMsg = "Próba dodania zadania podrzędnego o wartości NULL!";
				errorId = (int)StandardErrors.InvalidParameter;
				return null;
			}

			if (childTask.ParentID < 0)
				childTask.ParentID = ID;

			BaseTask child = Manager.AddTask(childTask, out errorMsg, out errorId);
			if (child != null)
				AddChild(childTask.ID);

			return child;
		}

		protected void AddChild(long taskID)
		{
			lock (m_ChildrenIDs)
			{
				lock (m_ChildResults)
				{
					if (m_ChildResults.ContainsKey(taskID))//sprawdzenie, czy zadanie potomne zdazylo sie zakonczyc wczesniej
						return;
				}
				m_ChildrenIDs.Add(taskID);
			}
		}
		protected int RemoveChild(long taskID)
		{
			lock (m_ChildrenIDs)
			{
				m_ChildrenIDs.Remove(taskID);
				return m_ChildrenIDs.Count;
			}
		}
		protected internal void ClearChildrenList()
		{
			lock (m_ChildrenIDs)
				m_ChildrenIDs.Clear();

			lock (m_ChildResults)
				m_ChildResults.Clear();
		}
		protected ChildExecResult[] GetChildResultsShanpshot()
		{
			lock (m_ChildResults)
				return m_ChildResults.Values.ToArray();
		}

		protected internal virtual void GetDetailsAfterFinished(StringBuilder sb)
		//przygotowanie opisu zadania po jego zakonczeniu
		{
			if (sb == null)
				return;

			sb.AppendFormat("Zakończono zadanie o ID={1} typu \"{2}\" z wynikiem: \"{3}:{0}\"\r\n", m_ErrorDesc, m_ID, Type, m_ErrorCode);
			sb.AppendFormat("    Prio=\"{0}\", Signature=\"{1}\",\r\n", m_Priority, m_Signature);
			sb.AppendFormat("    Result=\"{0}\",\r\n", m_Result);
			sb.AppendFormat("    AddInfo=\"{0}\",\r\n", string.Join("|", m_AddInfo.ToArray()));
			sb.AppendFormat("    BeginDate=\"{0}\",\r\n", m_OrygBeginDate);
			sb.AppendFormat("    CreateDate=\"{0}\"\r\n", m_CreationDate);
			sb.AppendFormat("    Rozpoczęcie wykonywania=\"{0}\"", m_HandlingStarted);
		}
		protected internal virtual void ChangeStateIfTimeIsValid()
		{
			if (m_CurrentState == TaskState.WaitingForOtherTask)
			{
				if (Manager.TaskWaitingForAnswerTimeout > -1)
				{
					int maxSeconds = Manager.TaskWaitingForOtherTimeout;
					TimeSpan ts = DateTime.Now - m_SuspendedTime;
					if (ts.TotalSeconds > maxSeconds)
					{
						Manager.ShowText(string.Format("Zadanie czeka już {0}s na zakończenie innego zadania o ID={2} - a max czas to {1}s. Zadanie zostanie wznowione.", ts.TotalSeconds, maxSeconds, m_OtherTaskID), System.Diagnostics.TraceEventType.Warning);
						OtherTaskID = -1;
						State = TaskState.Waiting;
					}
				}
			}
			else if (m_CurrentState == TaskState.WaitingForAnswer)
			{
				if (Manager.TaskWaitingForAnswerTimeout > -1)
				{
					int maxSeconds = Manager.TaskWaitingForAnswerTimeout;
					TimeSpan ts = DateTime.Now - m_StartWaitingTime;
					if (ts.TotalSeconds >= maxSeconds)
					{
						m_TotalWaitingTime += ts.TotalSeconds;
						if (!CanBeWaitingLonger(m_TotalWaitingTime, maxSeconds))
						{
							Manager.IncreaseTotalNumOfTimeoutedTasks();
							FinishTaskWithError((int)StandardErrors.ExecutionTimeout, string.Format("Zadanie czeka już {0}s na odpowiedź, a max czas to {1}s. Zadanie zostanie zakończone.", ts.TotalSeconds, maxSeconds), TraceEventType.Warning);
						}
						else
							m_StartWaitingTime = DateTime.Now;
					}
				}
			}
		}
		protected internal virtual bool CanBeWaitingLonger(double totalWaitingSeconds, int maxPeriodSeconds) //zwraca true, jeśli trzeba jeszcze dac zadaniu poczekać dluzej
		{
			if (NumOfChilds > 0)
			{
				long[] childrenTasks;
				lock (m_ChildrenIDs)
					childrenTasks = m_ChildrenIDs.ToArray();

				bool isChildOK = false;
				int errorId;
				string errorDesc;
				StringBuilder sbWait = new StringBuilder();
				foreach (long demDesc in childrenTasks)
				{
					if (m_CurrentState != TaskState.WaitingForAnswer)
						return true;

					isChildOK = true;
					try
					{
						if (Manager.IsTaskStillWorking(demDesc, out errorId, out errorDesc))
						{
							sbWait.AppendFormat("{0},", demDesc);
							AddErrorDesc(errorId, errorDesc, " ");
						}
						else
						{
							isChildOK = false;
						}
					}
					catch (Exception ex)
					{
						Manager.ReportError(string.Format("Błąd podczas weryfikacji zadania potomnego '{0}'!", demDesc), ex);
						isChildOK = false;
					}
					if (!isChildOK)
					{
						RemoveChild(demDesc);
						AddErrorDesc((int)StandardErrors.ExecutionTimeout, string.Format("Brak odpowiedzi na zlecone zadanie '{0}'.", demDesc), " ");
					}
				}
				if (sbWait.Length > 0)//sa jakies wykonywane podzadania, na ktore warto poczekac
				{
					Manager.ShowText(string.Format("Zadanie będzie jeszcze czekać na następujące podzadania: {0}", sbWait.ToString()), TraceEventType.Verbose);
					return true;
				}
			}

			if (m_CurrentState == TaskState.WaitingForAnswer)
			{
				string tmp = string.Format("Zadanie czeka już {0}s na odpowiedź - a max czas to {1}s. Zadanie zostanie zakończone.",
										totalWaitingSeconds, maxPeriodSeconds);
				AddErrorDesc((int)StandardErrors.ExecutionTimeout, tmp, " ");
				Manager.ShowText(tmp, TraceEventType.Verbose);
			}
			else
				return true;

			return false;
		}

		protected internal virtual bool CanStartRunning()//zwraca true, jeśli zadanie może rozpocząć swoje wykonywanie
		{
			return true;
		}

		protected internal virtual bool CanBeRunningLonger(double runningTime)
		//zwraca true, jeśli zadanie powinno jeszcze być wykonywane, a upłynął już domyślny max czas ,
		// jaki zadanie może być wykonywane (Running).
		//runningTime - w sekundach
		{
			if (m_LastCheckingProgress == m_Progress)
				return false;

			m_LastCheckingProgress = m_Progress;

			return true;
		}

		public virtual bool OnMessageReceived(MessageBody mb)
		{
			switch (mb.m_Type)
			{
				case "ShiftStateMsg":
					{
						try
						{
							Manager.ShowText(string.Format("Otrzymano informację o zakończeniu potomnego zadania o ID={0}.", mb.m_SendingTaskID), TraceEventType.Verbose);
							ChildExecResult cer = new ChildExecResult();
							cer.m_ChildID = mb.m_SendingTaskID;
							cer.m_ErrorCode = mb.m_ErrorType;
							cer.m_ErrorDesc = mb.m_ErrorDesc;

							lock (m_ChildResults)
								m_ChildResults[cer.m_ChildID] = cer;

							if (mb.m_Postpone)
							{
								Manager.ShowText("Zadanie potomne zgłosiło konieczność uśpienia tego zadania.", TraceEventType.Verbose);
								m_GoPostponedAfterLastChild = true;
								DateTime dt = mb.m_PostponeTill;
								if (dt < m_PostponedUntil)
									m_PostponedUntil = dt;
								cer.m_isPostponed = true;
							}
							else
							{
								if (mb.m_ErrorType != (int)StandardErrors.Success)
									AddErrorDesc(mb.m_ErrorType, mb.m_ErrorDesc, "\r\n ");
							}

							cer.m_Result = mb.m_Result;
							cer.m_AddInfo = mb.m_AddInfo;
						}
						catch (Exception ex)
						{
							Manager.ReportError("Błąd uspójniania informacji po zakończeniu zadania potomnego!", ex);
						}


						int numOfChilds = RemoveChild(mb.m_SendingTaskID);

						if (m_CurrentState == TaskState.WaitingForAnswer)
						{
							string childDetails = "";
							lock (m_ChildrenIDs)
							{
								childDetails = string.Join(";", m_ChildrenIDs.ToArray());
							}

							Manager.ShowText(string.Format("Zadanie {1} oczekuje jeszcze na zakończenie {0} zadań potomnych: {2}.",
															numOfChilds, ID, childDetails),
											TraceEventType.Verbose);
							if (numOfChilds < 1)
							{
								if (m_GoPostponedAfterLastChild)
								{
									Postpone(m_PostponedUntil);
									if (IsMainEntryTask)
									{
										Manager.ShowText("Zadanie przejdzie w stan uśpienia!", TraceEventType.Verbose);
										return false;
									}
									Manager.ShowText("Zadanie zostanie zakończone, a zadanie nadrzędne przejdzie w stan uśpienia.", TraceEventType.Verbose);
								}
								if (m_FinishAfterLastChild)
									return true;
								else
								{
									TryToRun();
									return false;
								}
							}
						}
					}
					break;
				default:
					Manager.ShowText(string.Format("Wiadomość od zadania od ID='{5}' nie jest przeznaczona dla tego zadania o ID={4} ! Treść: [{0}, {1}, {2}]",
													mb.m_Type,
													mb.m_ErrorType,
													mb.m_ErrorDesc,
													this.ID,
													mb.m_SendingTaskID),
									TraceEventType.Warning);
					break;
			}
			return false;
		}
		protected internal virtual bool OnOtherTaskFinished(BaseTask task)
		{
			return ForceInformOtherTaskFinished(task.ID);
		}
		public bool ForceInformOtherTaskFinished(long taskID)
		{
			if (State == TaskState.WaitingForOtherTask && m_OtherTaskID == taskID)// na to zadanie wlasnie czekal
			{
				OtherTaskID = -1;
				//new ThreadStart(TryToRun).BeginInvoke(null, null);
				State = TaskState.Waiting;
				return true;
			}
			return false;
		}
		protected internal virtual bool ShouldAskingTaskWaitForMe(BaseTask askingTask)
		{
			return false;
		}
		public bool IsWorking()
		{
			if (m_CurrentState == TaskState.WaitingForAnswer
				|| m_CurrentState == TaskState.Running)
				return true;
			else
				return false;
		}
		protected internal virtual void CalculatePriority()
		{
		}

		internal void Realize()
		{
			try
			{
				m_OwnsExecutingThread = true;

				m_HandlingStarted = DateTime.Now;

				m_TotalRunningTime = 0.0;

				DoSpecificAction();

				m_OwnsExecutingThread = false;

				if (State == TaskState.Running)
					State = TaskState.WaitingForFinish;
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				FinishTaskWithError((int)StandardErrors.ExecutionTimeout, "Wątek wykonawczy został zatrzymany przez Zarządce kolejki zadań.");
			}
			catch (Exception ex)
			{
				FinishTaskWithError(ex);
			}
		}

		protected abstract void DoSpecificAction();

		protected virtual void BeforeFinish()
		{
			ForceFinishAllChilds(string.Format("Zadanie nadrzędne o ID={0} zakończyło wykonywanie!", ID));

			if (m_ParentID > -1)
			{
				MessageBody mb = new MessageBody("ShiftStateMsg",
												m_ParentID, 
												ID,
												m_ErrorCode,
												m_ErrorDesc,
												(m_CurrentState == TaskState.Postponed),
												m_BeginDate,
												m_Result,
												AddInfo,
												null);

				BeforeSendingMessage(ref mb);
				Manager.OnMessageForTask(mb);
				m_Progress = 100;
			}
		}

		protected virtual void AfterFinish()
		{
		}

		internal void FinishTask()
		{
			m_TotalRunningTime = 0.0;

			BeforeFinish();

			lock (this)
			{
				if (m_CurrentState == TaskState.Finished && m_FinishDate < DateTime.Now)//juz bylo zakonczone
					return;
				//if (State != TaskState.Postponed)
				State = TaskState.Finished;

				m_FinishDate = DateTime.Now;
			}

			Manager.OnFinishTask(this);

			AfterFinish();
		}

		protected void FinishTaskWithError(int codeOfError, string errorMsg, TraceEventType lev)
		{
			if (!string.IsNullOrEmpty(errorMsg))
				Manager.ShowText(errorMsg, lev);
			AddErrorDesc(codeOfError, errorMsg, " ");
			State = TaskState.WaitingForFinish;
			//FinishTask();
		}

		protected virtual void FinishTaskWithError(Exception exc)
		{
			m_ErrorCode = (int)StandardErrors.Runtime;
			m_ErrorDesc = string.Format("Błąd podczas wykonywania zadania: {0} [{1}]", m_ID, exc.Message);
			Manager.ReportError("Błąd podczas wykonywania zadania !", exc);
			//FinishTask();
			State = TaskState.WaitingForFinish;
		}

		protected internal void FinishTaskWithError(int codeOfError, string errorMsg)
		{
			FinishTaskWithError(codeOfError, errorMsg, System.Diagnostics.TraceEventType.Error);
		}

		protected virtual void BeforeSendingMessage(ref MessageBody mb)
		{
		}

		protected void AddErrorDesc(int errorId, string errorDesc, string separator)
		{
			if (m_ErrorCode == (int)StandardErrors.Success)
				m_ErrorCode = errorId;

			if (!string.IsNullOrEmpty(errorDesc))
			{
				if (string.IsNullOrEmpty(m_ErrorDesc))
					m_ErrorDesc = errorDesc;
				else
				{
					if (!m_ErrorDesc.Contains(errorDesc))
						m_ErrorDesc += separator + errorDesc;
				}
			}
		}

		protected void SetErrorDescIfNone(int errorId, string errorDesc)
		{
			if (m_ErrorCode == (int)StandardErrors.Success)
				m_ErrorCode = errorId;
			if (m_ErrorDesc.Length < 1)
				m_ErrorDesc = errorDesc;
		}

		protected internal void TryToRun()
		{
			bool enqueue = false;
			TaskState oldState = State;
			lock (this)
			{
				if (oldState != TaskState.Running && oldState != TaskState.Finished)
				{
					State = TaskState.Running;
					enqueue = true;
				}
			}

			if (enqueue)
			{
				if (!Manager.EnqueueTaskForExecution(this))
				{
					State = oldState;
				}
			}
		}

		protected void Postpone(long numOfMiliseconds)
		{
			Postpone(DateTime.Now.AddMilliseconds(numOfMiliseconds));
		}

		protected void Postpone(DateTime tillDate)
		{
			if (State != TaskState.WaitingForFinish && State != TaskState.Finished)
			{
				m_BeginDate = tillDate;
				State = TaskState.Postponed;

				InformOtherTasksWaitingForMe();
			}
			else
				throw new ApplicationException("Próba uśpienia zadania, które zostało już wykonane albo ubite.");
		}

		protected internal void AddOtherTaskIDWaitingForMe(long TaskID)
		{
			lock (m_OtherTasksWaitingForMe)
			{
				m_OtherTasksWaitingForMe.Add(TaskID);
			}
		}
		protected internal void RemoveOtherTaskIDWaitingForMe(long TaskID)
		{
			lock (m_OtherTasksWaitingForMe)
			{
				m_OtherTasksWaitingForMe.Remove(TaskID);
			}
		}
		protected internal void InformOtherTasksWaitingForMe()
		{
			long[] arTasks = null;
			if (m_OtherTasksWaitingForMe != null)
			{
				lock (m_OtherTasksWaitingForMe)
				{
					arTasks = m_OtherTasksWaitingForMe.ToArray();
					m_OtherTasksWaitingForMe.Clear();
				}
				Array.ForEach(arTasks, delegate(long taskID)
				{
					BaseTask dd = Manager.FindTaskInMainQueue(taskID);
					if (dd != null)
						dd.OnOtherTaskFinished(this);
				});
			}
		}

		protected internal long[] GetOtherTasksWaitingForMe()
		{
			long[] arTasks = null;
			if (m_OtherTasksWaitingForMe != null)
			{
				lock (m_OtherTasksWaitingForMe)
				{
					arTasks = m_OtherTasksWaitingForMe.ToArray();
				}
			}
			return arTasks;
		}

		protected void ForceFinishAllChilds(string reason)
		{
			if (NumOfChilds > 0)
			{
				long[] childrenTasks;
				lock (m_ChildrenIDs) 
					childrenTasks = m_ChildrenIDs.ToArray();

				int errorId;
				string errorDesc;
				foreach (long childTaskId in childrenTasks)
				{
					try
					{
						if (!Manager.ForceFinishChildTask(childTaskId, out errorId, out errorDesc))
							AddErrorDesc(errorId, errorDesc, " ");
						else
							RemoveChild(childTaskId);
					}
					catch (Exception ex)
					{
						Manager.ReportError(string.Format("Błąd podczas wymuszonego zakończenia zadania potomnego '{0}'.", childTaskId), ex);
					}
				}
			}
		}

		public void ForceFinishTask(string reason)
		{
			ForceFinishAllChilds(reason);

			Manager.CancelRunningTask(ID);

			m_ErrorCode = (int)StandardErrors.General;
			if (string.IsNullOrEmpty(reason))
				m_ErrorDesc = "Zakończenie zadania wymuszone przez administratora systemu!";
			else
				m_ErrorDesc = "Zakończenie zadania wymuszone z powodu: " + reason;
			State = TaskState.WaitingForFinish;
		}

		protected internal virtual void DumpState(StringBuilder sb)
		{
			sb.AppendFormat("   # Zadanie o ID = {0}, State = {1}, Type = {2}\r\n", ID, m_CurrentState, Type);
			sb.AppendFormat("       Priority = {0}, Progress = {3}, ErrorType = {1}, ErrorDesc = {2}\r\n", m_Priority, m_ErrorCode, m_ErrorDesc, m_Progress);
			sb.AppendFormat("       CreationDate = {0}\r\n", m_CreationDate.ToString("dd/MM HH:mm:ss.f"));
			sb.AppendFormat("       OrygBeginDate = {0}, BeginDate = {1}\r\n", m_OrygBeginDate.ToString("dd/MM HH:mm:ss.f"), m_BeginDate.ToString("dd/MM HH:mm:ss.f"));
			sb.AppendFormat("       SuspendedTime = {0}, StartWaitingTime = {1}\r\n", m_SuspendedTime.ToString("dd/MM HH:mm:ss.f"), m_StartWaitingTime.ToString("dd/MM HH:mm:ss.f"));
			sb.AppendFormat("       HandlingStarted = {0}, FinishDate = {1}\r\n", m_HandlingStarted.ToString("dd/MM HH:mm:ss.f"), m_FinishDate.ToString("dd/MM HH:mm:ss.f"));
			sb.AppendFormat("       GoPostponedAfterLastChild = {0}, PostponedUntil={1}\r\n", m_GoPostponedAfterLastChild, m_PostponedUntil.ToString("dd/MM HH:mm:ss.f"));
			sb.AppendFormat("       ParentID = {0}\r\n", m_ParentID);
			sb.AppendFormat("       ChildrenIDs = {0}\r\n", string.Join(",", m_ChildrenIDs.Select( ci => ci.ToString()).ToArray()));
			if (m_ChildResults != null)
				sb.AppendFormat("       ChildResultsCount = {0}\r\n", m_ChildResults.Count);
			sb.AppendFormat("       FinishAfterLastChild = {0}\r\n", m_FinishAfterLastChild);
			long[] otherTasks = GetOtherTasksWaitingForMe();
			if (otherTasks != null && otherTasks.Length > 0)
				sb.AppendFormat("       OtherTaskID = {0}, OtherDemandsWaitingForMe = [{1}]\r\n", m_OtherTaskID, string.Join(",", otherTasks.Select(ot => ot.ToString()) ));
			sb.AppendFormat("       LastExecutionThreadAssignmentDate = {0}, LastExecutionThreadLeftDate = {1}, ExecThreadOwningCount = {2}\r\n", m_LastExecutionThreadAssignmentDate.ToString("dd/MM HH:mm:ss.f"), m_LastExecutionThreadLeftDate.ToString("dd/MM HH:mm:ss.f"), m_ExecThreadOwningCount);
			sb.AppendFormat("       TotalRunningTime = {0}, TotalWaitingTime = {1}\r\n", m_TotalRunningTime, m_TotalWaitingTime);
			sb.AppendFormat("       Result = {0}\r\n", m_Result == null ? "NULL" : m_Result);
			if (m_AddInfo != null && m_AddInfo.Count > 0)
				sb.AppendFormat("       AddInfo = [{0}]\r\n", string.Join(",", AddInfo));
			else
				sb.Append("       AddInfo = \r\n");
			sb.Append("   #\r\n");
		}
	}
}
