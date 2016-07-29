using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace PSNC.RepoAV.TaskQueue
{
	public delegate void ShowTextDel(string message, TraceEventType level);
	public delegate void ReportErrorDel(string message, Exception exc);
	public delegate void PeriodicEventForTasksDel();
	public delegate void TaskRelatedDel(BaseTask task);

    public class TaskQueueManager
    {
		public static readonly int LockWaitingTimeout = 10000;

		protected List<BaseTask> m_Tasks = null;//lista zadań aktualnie przetwarzanych
		protected Dictionary<long, BaseTask> m_FastSearchInMainQueue = null;//dodatek do szybkiego szukania w liscie zadan po ID
		protected ReaderWriterLock m_QueueLocker;	//synchronizuje dostęp do kolejki zadań	
		protected Thread m_WrokingThread = null;//uchwyt do watka przegladajacego kolejki
		protected AutoResetEvent m_WorkIsFinished = null;//ustawione na true gdy zakonczy sie obsluga zadan - przy zatrzymywaniu systemu
		protected bool m_StopWorking;//ture jeśli zarządca nie ma obslugiwac zadan dystrybucji poniewaz bedzie SDT zatrzymane		
		protected AutoResetEvent m_NewTaskAdded = new AutoResetEvent(false);
		protected List<TaskThreadInfo> m_ExecuteThreads = new List<TaskThreadInfo>();//lista wątków wykonawczych
		protected Queue m_QueueToExecute = null;//lista zadań przeznaczonych do przydzielenia wątku wykonawczego
		protected ManualResetEvent m_NewTaskWaitingForExecution;//event ustawiony, jesli nowe zadanie przyszlo do kolejki i czeka na wykonanie
		protected List<BaseTask> m_RunningTasks = null;//lista zadań, które mają w danej chwili przydzielony wątek wykonawczy - są w stanie Running
		protected int m_TaskWaitingForOtherTimeout;//czas po jakim zadanie oczekujace na inne zadanie zostanie wznowione (w sekundach)
		protected int m_TaskWaitingForAnswerTimeout;//czas po jakim zadanie oczekujace w stanie WaitingForAnswer zostanie zakończone z błędem (w sekundach)
		protected uint m_MaxNumberOfMainTasksInQueue;//max liczba zadan glownych, jakie system moze przetwarzac
		protected int m_MaxTimeOfWorkingThreadExecution;// max czas w sek jaki watek obslugujacy zadania moze sie wykonywac (w sekundach)
		protected long m_TotalNumOfFinishedTasksWithError = 0;
		protected long m_TotalNumOfFinishedTasks = 0;
		protected long m_TotalNumOfAbortedThreads = 0;
		protected long m_TotalNumOfTimeoutedTasks = 0;
		protected bool m_CheckIfRunningTasksAreLate;//flaga wł/wył sprawdzanie, czy zadania nie wykonują się już zbyt długo
		protected int m_QueueCheckingInterval;//okres, co jaki przeglądana jest kolejka zadań, jeżeli nie został ustawiony event o dodaniu nowego zadania - w milisekundach
		protected List<BaseTask> m_PostponedTasks;//kolekcja zadań uśpionych, posortowana po dacie obudzenia.Jest uznawana za część kolejki głównej (m.in. w licznikach)
		protected ReaderWriterLock m_PostponedQueueLocker;	//synchronizuje dostęp do kolejki zadań	uśpionych
		protected string m_CategoryName;//uzywane przy logowaniu (trace)		
		protected int m_NumOfMainTasks = 0;
		protected System.Timers.Timer m_Timer;
		protected uint m_NumOfWorkingThreads;//liczba watków wykonawczych
		protected object m_Owner;//dowolny obiekt, ktory faktycznie utrzymuje kolejke zadan (jest jej wlasicicielem)
		protected int m_DumpStateIterval;//odstep czasowy pomiedzy kolejnymi zrzutami stanu kolejki; -1 jesli funkcjonalnosc jest wyłączona. [sekundy]
		protected string m_DumpStateFilePath; //plik ze zrzucanym stanem kolejki
		protected DateTime m_DumpStateLastDate; //data ostaniego zapisu pliku ze zrzucanym stanem kolejki
		protected bool m_DumpStateInProgress; // ustawione na true, podczas, gdy wykonywany jest dump
		private bool m_CheckTasksInProgress; // ustawione na true, podczas, gdy wykonywane jest sprawdzenie zadań w kolejce (po katem timeoutow)
		protected TaskIDGenerator m_IDGenerator;

		public event ShowTextDel ShowTextEvent;//event uzywany do wyswietlania komunikatow
		public event ReportErrorDel ReportErrorEvent;//event uzywany do wyswietlania komunikatow o wyjatkach
		public event TaskRelatedDel TaskFinished; //wołane po zakończeniu przetwarzania zadania, a przed jego usunięciem

		public bool StopWorking
		{
			get
			{
				return m_StopWorking;
			}
		}
		public int TaskWaitingForOtherTimeout
		{
			get { return m_TaskWaitingForOtherTimeout; }
			set { m_TaskWaitingForOtherTimeout = value; }
		}
		public int TaskWaitingForAnswerTimeout
		{
			get { return m_TaskWaitingForAnswerTimeout; }
			set { m_TaskWaitingForAnswerTimeout = value; }
		}
		public int MaxTimeOfWorkingThreadExecution { set { m_MaxTimeOfWorkingThreadExecution = value; } }
		public long TotalNumOfAbortedThreads { get { return m_TotalNumOfAbortedThreads; } }
		public long TotalNumOfFinishedTasks { get { return m_TotalNumOfFinishedTasks; } }
		public long TotalNumOfTimeoutedTasks { get { return m_TotalNumOfTimeoutedTasks; } }
		public long TotalNumOfFinishedTasksWithError { get { return m_TotalNumOfFinishedTasksWithError; } }
		public uint NumOfWorkingThreads
		{
			get { return m_NumOfWorkingThreads; }
		}
		public bool CheckIfRunningTasksAreLate
		{
			set { m_CheckIfRunningTasksAreLate = value; }
			get { return m_CheckIfRunningTasksAreLate; }
		}
		public int QueueCheckingInterval
		{
			set { m_QueueCheckingInterval = value; }
			get { return m_QueueCheckingInterval; }
		}
		public int TotalNumberOfTasks
		{
			get
			{
				m_QueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
				try
				{
					return (m_Tasks.Count + m_PostponedTasks.Count);
				}
				finally
				{
					m_QueueLocker.ReleaseReaderLock();
				}
			}
		}
		public object Owner
		{
			get
			{
				return m_Owner;
			}
			set
			{
				m_Owner = value;
			}
		}
		public int DumpStateIterval
		{
			set { m_DumpStateIterval = value; }
			get { return m_DumpStateIterval; }
		}
		public string DumpStateFilePath
		{
			set { m_DumpStateFilePath = value; }
			get { return m_DumpStateFilePath; }
		}
		public int NumberOfTasks
		{
			get
			{
				m_QueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
				try
				{
					return m_Tasks.Count;
				}
				finally
				{
					m_QueueLocker.ReleaseReaderLock();
				}
			}
		}
		public int NumberOfPostponedTasks
		{
			get
			{
				m_PostponedQueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
				try
				{
					return m_PostponedTasks.Count;
				}
				finally
				{
					m_PostponedQueueLocker.ReleaseReaderLock();
				}
			}
		}
		public int NumberOfRunningTasks
		{
			get
			{
				return m_RunningTasks.Count;
			}
		}


		public string LogCategoryName
		{
			get
			{
				return m_CategoryName;
			}
		}


		public TaskQueueManager(string categoryName, object owner)
			: this(100, 1000, 25, categoryName, owner)
		{
		}

		public TaskQueueManager(uint initialTasksQueueSize,
								uint maxMainTasksInQueue,
								uint workingThreadsCount,
								string categoryName,
								object owner)
		{
			m_Owner = owner;
			m_IDGenerator = new TaskIDGenerator(256);
			m_CategoryName = categoryName;
			m_NumOfWorkingThreads = workingThreadsCount;
			if (m_NumOfWorkingThreads > 500)
				m_NumOfWorkingThreads = 500;
			m_MaxNumberOfMainTasksInQueue = maxMainTasksInQueue;
			m_QueueLocker = new ReaderWriterLock();
			m_Tasks = new List<BaseTask>((int)initialTasksQueueSize);
			m_WorkIsFinished = new AutoResetEvent(false);
			m_FastSearchInMainQueue = new Dictionary<long,BaseTask>((int)initialTasksQueueSize);
			m_MaxTimeOfWorkingThreadExecution = 100;
			m_TaskWaitingForAnswerTimeout = 90;
			m_TaskWaitingForOtherTimeout = 120;
			m_QueueCheckingInterval = 3000;
			m_QueueToExecute = new Queue(50);
			m_NewTaskWaitingForExecution = new ManualResetEvent(false);
			m_PostponedTasks = new List<BaseTask>((int)initialTasksQueueSize);
			m_PostponedQueueLocker = new ReaderWriterLock();
			m_RunningTasks = new List<BaseTask>(50);
			m_MaxNumberOfMainTasksInQueue = maxMainTasksInQueue;
			m_CheckIfRunningTasksAreLate = true;
			m_DumpStateFilePath = null;
			m_DumpStateIterval = -1;
			m_DumpStateLastDate = DateTime.Now;
			m_DumpStateInProgress = false;
			m_CheckTasksInProgress = false;
		}

		void m_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (m_StopWorking)
				return;

			ThreadPool.QueueUserWorkItem(CheckTasksInThread);//sprawdzenie timeoutow dla zadan			

			if (m_DumpStateIterval > 0 && !string.IsNullOrEmpty(m_DumpStateFilePath))
			{
				if (m_DumpStateLastDate > DateTime.MinValue)
				{
					TimeSpan ts = DateTime.Now - m_DumpStateLastDate;
					if (ts.TotalSeconds >= m_DumpStateIterval)
						ThreadPool.QueueUserWorkItem(DumpState);
				}
				else
					ThreadPool.QueueUserWorkItem(DumpState);
			}

			if (!m_StopWorking)
				m_Timer.Start();
		}

		public bool Start()
		{
			m_StopWorking = false;
			m_WorkIsFinished.Reset();
			m_QueueToExecute.Clear();
			m_NewTaskWaitingForExecution.Reset();
			m_RunningTasks.Clear();

			m_TotalNumOfAbortedThreads = 0;
			m_TotalNumOfFinishedTasks = 0;
			m_NumOfMainTasks = 0;

			bool res = InitExecuteMech(m_NumOfWorkingThreads);
			if (res)
			{
				m_WrokingThread = new Thread(new ThreadStart(WorkForTasks));
				m_WrokingThread.Priority = ThreadPriority.Normal;
				m_WrokingThread.IsBackground = true;
				m_WrokingThread.Start();

				m_Timer = new System.Timers.Timer(1000);
				m_Timer.AutoReset = false;
				m_Timer.Elapsed += new System.Timers.ElapsedEventHandler(m_Timer_Elapsed);
				m_Timer.Start();
			}
			return res;
		}

		public void SetNewDemandEvent()
		{
			m_NewTaskAdded.Set();
		}

		public bool SetMaxNumOfWorkingThreads(uint num)
		{
			if (m_NumOfWorkingThreads != num)
			{
				m_NumOfWorkingThreads = num;
				return InitExecuteMech(num);
			}
			return true;
		}

		protected bool InitExecuteMech(uint newThreadCount)
		{
			if (newThreadCount > 500)
				return false;
			lock (m_ExecuteThreads)
			{
				try
				{
					for (int i = 0; i < m_ExecuteThreads.Count; i++)
					{
						if (i < newThreadCount)
						{
							InitTaskThread(i);
						}
						else
						{
							if (m_ExecuteThreads[i] != null)
								m_ExecuteThreads[i].m_ShouldWork = false;

							m_ExecuteThreads.RemoveAt(i);
							i--;
						}
					}
					int alreadyAdded = m_ExecuteThreads.Count;
					for (int i = alreadyAdded; i < newThreadCount; i++)
					{
						TaskThreadInfo tti = new TaskThreadInfo();
						m_ExecuteThreads.Add(tti);

						InitTaskThread(m_ExecuteThreads.Count - 1);
					}

					ShowText(string.Format("Zainicjowano moduł wielowątkowej realizacji zadań - max liczba równoległych wątków wynosi {0}.", newThreadCount), TraceEventType.Verbose);
					return true;
				}
				catch (Exception ex)
				{
					ReportError("Błąd podczas zmiany maksymalnej liczby wątków wykonawczych obsługujących realizację zadań!", ex);
					return false;
				}
			}
		}

		protected void DeInitExecuteMech()
		{
			try
			{
				m_StopWorking = true;

				if (m_Timer != null)
				{
					m_Timer.Stop();
					m_Timer.Dispose();
					m_Timer = null;
				}

				lock (m_ExecuteThreads)
				{
					for (int i = 0; i < m_ExecuteThreads.Count; i++)
						if (m_ExecuteThreads[i] != null)
							m_ExecuteThreads[i].m_ShouldWork = false;

					m_ExecuteThreads.Clear();
				}

				ShowText("Zdeinicjalizowano moduł wielowątkowej realizacji zadań !", TraceEventType.Verbose);
			}
			catch (Exception ex)
			{
				ReportError("Błąd podczas zmiany maksymalnej liczby wątków wykonawczych obsługujących realizację zadań!", ex);
			}
		}

		public bool Clear()
		{
			return ClearMainQueue();
		}

		public bool ClearMainQueue()
		{
			bool result = true;

			m_PostponedQueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				m_PostponedTasks.Clear();
			}
			catch (Exception e)
			{
				ReportError("Błąd podczas czyszczenia kolejki zadań uśpionych!", e);
				result = false;
			}
			finally
			{
				m_PostponedQueueLocker.ReleaseWriterLock();
			}

			lock (m_RunningTasks)
				m_RunningTasks.Clear();

			m_QueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				m_Tasks.Clear();
				m_FastSearchInMainQueue.Clear();
			}
			catch (Exception e)
			{
				ReportError("Błąd podczas czyszczenia kolejki zadań!", e);
				result = false;
			}
			finally
			{
				m_QueueLocker.ReleaseWriterLock();
			}

			lock (m_RunningTasks)
				m_RunningTasks.Clear();

			return result;
		}

		private void ExecuteTaskInThread()
		{
			TaskThreadInfo tti = null;
			try
			{
				int idx = int.Parse(Thread.CurrentThread.Name);
				if (m_ExecuteThreads[idx] != null && m_ExecuteThreads[idx].m_TaskThread == Thread.CurrentThread)
					tti = m_ExecuteThreads[idx];

				if (tti != null)
				{
					while (tti.m_ShouldWork && !m_StopWorking)
					{
						tti.m_CurrentTask = null;
						tti.m_StartRuningTask = DateTime.MaxValue;

						m_NewTaskWaitingForExecution.WaitOne();

						BaseTask task = null;
						lock (m_QueueToExecute.SyncRoot)
						{
							if (m_QueueToExecute.Count > 0)
								task = (BaseTask)m_QueueToExecute.Dequeue();

							if (m_QueueToExecute.Count == 0)
								m_NewTaskWaitingForExecution.Reset();
						}

						if (task != null)
						{
							task.LastExecutionThreadAssignmentDate = DateTime.Now;
							task.LastExecutionThreadLeftDate = DateTime.MaxValue;
							task.ExecThreadOwningCount++;
							ShowText(string.Format("Wątek wykonawczy o ID='{0}' pobrał zadanie o ID='{1}' do wykonania!", Thread.CurrentThread.Name, task.ID), TraceEventType.Verbose);

							if (task.State != TaskState.Running && task.State != TaskState.WaitingForFinish)
								ShowText(string.Format("Wątek wykonawczy o ID='{0}' dostał do wykonania zadanie w złym stanie ({1}) - ID zadania '{2}'!", Thread.CurrentThread.Name, task.State, task.ID), TraceEventType.Warning);

							bool wasAborted = false;
							try
							{
								tti.m_StartRuningTask = DateTime.Now;
								tti.m_CurrentTask = task;


								if (task.State == TaskState.Running)
								{
									task.Realize();
								}

								tti.m_StartRuningTask = DateTime.Now;
								if (task.State == TaskState.WaitingForFinish)
									task.FinishTask();
							}
							catch (ThreadAbortException)
							{
								wasAborted = true;
								Thread.ResetAbort();
								tti.m_StartRuningTask = DateTime.MaxValue;
								task.State = TaskState.WaitingForFinish;
								task.FinishTaskWithError((int)StandardErrors.ExecutionTimeout, "Wątek wykonawczy został zatrzymany przez Zarządce kolejki zadań!");
								ShowText(string.Format("Wątek wykonawczy o ID='{0}' został zatrzymany! Możliwe, że wykonanie zadania trwało za długo!", Thread.CurrentThread.Name), TraceEventType.Warning);
							}
							catch (Exception ex)
							{
								task.State = TaskState.WaitingForFinish;
								if (task.ErrorDesc.Length < 1)
									task.ErrorDesc = ex.ToString();
								ReportError("Błąd w wątku wykonawczym zadania!", ex);
							}
							finally
							{
								try
								{
									tti.m_StartRuningTask = DateTime.MaxValue;
									task.LastExecutionThreadLeftDate = DateTime.Now;

									lock (m_RunningTasks)
										m_RunningTasks.Remove(task);

									tti.m_CurrentTask = null;
								}
								catch (Exception exI)
								{
									ReportError("Błąd przy usuwaniu zadania z listy aktualnie wykonywanych.", exI);
								}
							}
							if (wasAborted)
							{
								tti.m_TaskThread = null;
								return;
							}
						}
					}
				}
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				ShowText(string.Format("Wątek wykonawczy o ID='{0}' został zatrzymany!", Thread.CurrentThread.Name), TraceEventType.Warning);
				if (tti != null)
				{
					if (tti.m_CurrentTask != null)
						tti.m_CurrentTask.FinishTaskWithError((int)StandardErrors.ExecutionTimeout, string.Format("Wątek wykonawczy o ID='{0}' został zatrzymany.", Thread.CurrentThread.Name));
					tti.m_StartRuningTask = DateTime.MaxValue;
					if (tti.m_CurrentTask != null)
						tti.m_CurrentTask.LastExecutionThreadLeftDate = DateTime.Now;
				}
			}
			catch (Exception ex)
			{
				ReportError("Wyjątek w wątku wykonującym zadania!", ex);
			}
			try
			{
				if (tti != null)
				{
					tti.m_TaskThread = null;
					tti.m_StartRuningTask = DateTime.MaxValue;
				}

				ShowText(string.Format("Wątek wykonujący zadania o numerze {0} został zakończony.", Thread.CurrentThread.Name), TraceEventType.Verbose);
			}
			catch (Exception ex)
			{
				ReportError("Wyjątek w wątku wykonującym zadania (END)!", ex);
			}
		}

		private BaseTask[] GetAllRunningTasks()
		{
			List<BaseTask> ret = new List<BaseTask>();
			lock (m_RunningTasks)
			{
				foreach (BaseTask bt in m_RunningTasks)
					if (bt.State == TaskState.Running)
						ret.Add(bt);
			}
			return ret.ToArray();
		}

		protected internal bool EnqueueTaskForExecution(BaseTask task)
		{
			if (m_StopWorking)
				return false;

			try
			{
				ShowText(string.Format("Próba dodania do kolejki zadań przeznaczonych do wykonania nowego zadania o ID={0}, typu '{2}' i w stanie '{1}'.", task.ID, task.State, task.GetType().Name), TraceEventType.Verbose);

				if (task.State == TaskState.Running)
				{
					if (m_StopWorking || !task.CanStartRunning())
						return false;

					if (task.CanWaitForOtherTask)
					{
						BaseTask[] running = GetAllRunningTasks();
						if (running.Length > 0)
						{
							if (ShouldWaitForOtherTask(task, running))
							{
								//									ShowText("Zadanie nr : " + task.ID.ToString() + " będzie czekać na koniec zadania nr : " + task.OtherTaskID.ToString() + " !", TraceEventType.Information);
								return false;
							}
						}

						BaseTask[] tasks = GetAllTasksWaitingForAnswer();
						if (ShouldWaitForOtherTask(task, tasks))
							return false;

						lock (m_RunningTasks)
							m_RunningTasks.Add(task);

						if (m_StopWorking)
						{
							lock (m_RunningTasks)
								m_RunningTasks.Remove(task);
							return false;
						}

					}
					else
					{
						if (m_StopWorking)
							return false;

						lock (m_RunningTasks)
							m_RunningTasks.Add(task);
					}
				}
				else if (task.State == TaskState.WaitingForFinish)
				{
					if (m_StopWorking)
						return false;

					lock (m_RunningTasks)
						m_RunningTasks.Add(task);
				}
				else
				{
					return false;
				}

				lock (m_QueueToExecute.SyncRoot)
				{
					if (!m_QueueToExecute.Contains(task))
					{
						ShowText(string.Format("Dodano do kolejki zadań przeznaczonych do wykonania nowe zadanie o ID={0}, typu '{2}' i w stanie '{1}'.", task.ID, task.State, task.GetType().Name), TraceEventType.Verbose);
						m_QueueToExecute.Enqueue(task);
						m_NewTaskWaitingForExecution.Set();
					}
					else
					{
						lock (m_RunningTasks)
							m_RunningTasks.Remove(task);
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				ReportError("Błąd podczas kolejkowania zadania do wykonania!", ex);

				lock (m_RunningTasks)
					m_RunningTasks.Remove(task);

				return false;
			}
			return true;
		}

		public void ForceFinishTasks(long[] TaskIDs, string reason)
		{
			if (TaskIDs == null)
				return;

			for (int j = 0; j < TaskIDs.Length; j++)
			{
				BaseTask dem = FindTask(TaskIDs[j]);
				if (dem != null)
					dem.ForceFinishTask(reason);
			}
		}

		public bool ForceFinishChildTask(long childId, out int errorId, out string errorDesc)
		{
			errorId = (int)StandardErrors.Success;
			errorDesc = "";

			string reason = string.Format("Wymszone zakończenie zadania potomnego o ID='{0}', ze względu na wymuszone zakończenie zadania nadrzędnego.", childId);
			ShowText(reason, TraceEventType.Verbose);

			ForceFinishTasks(new long[] { childId }, reason);
			return true;
		}

		protected internal virtual void OnFinishTask(BaseTask task)
		{
			if (TaskFinished != null)
				TaskFinished.BeginInvoke(task, null, null);

			ShowText("Informuje inne zadania o zakończeniu zadania numer: " + task.ID.ToString(), TraceEventType.Verbose);
			task.InformOtherTasksWaitingForMe();

			Interlocked.Increment(ref m_TotalNumOfFinishedTasks);

			if (task.CodeOfError != (int)StandardErrors.Success)
				Interlocked.Increment(ref m_TotalNumOfFinishedTasksWithError);

			try
			{
				RemoveTask(task, false);
			}
			catch (ThreadAbortException)
			{
				ShowText("Wątek kończący zadanie został zatrzymany!", TraceEventType.Information);
				Thread.ResetAbort();
				return;
			}
			catch (Exception exc)
			{
				ReportError("Błąd podczas usuwania zakończonego zadania z kolejki zadań !", exc);
			}

			StringBuilder sb = new StringBuilder();
			task.GetDetailsAfterFinished(sb);
			string strShortInfo = sb.ToString();
			if (strShortInfo.Length > 0)
				ShowText(strShortInfo, TraceEventType.Information);
		}

		protected internal void IncreaseTotalNumOfTimeoutedTasks()
		{
			Interlocked.Increment(ref m_TotalNumOfTimeoutedTasks);
		}

		internal bool RemoveTask(BaseTask task, bool moveToPostponed)
		{
			if (task == null)
				return false;

			bool result = true;

			if (moveToPostponed)
			{
				AddTasksToPostponed(new BaseTask[] { task });
				m_QueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
				try
				{
					ShowText("Przesunięcie zadania o nr: " + task.ID.ToString() + " do kolejki zadań uśpionych!", TraceEventType.Verbose);
					m_Tasks.Remove(task);
				}
				finally
				{
					m_QueueLocker.ReleaseWriterLock();
				}
			}
			else
			{
				if (task.IsMainEntryTask)
					Interlocked.Decrement(ref m_NumOfMainTasks);

				if (task.State == TaskState.Postponed)
					RemoveTaskFromPostponed(task);

				m_QueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
				try
				{
					ShowText("Usunięcie z kolejki zakończonego zadania o nr: " + task.ID.ToString(), TraceEventType.Verbose);
					m_FastSearchInMainQueue.Remove(task.ID);
					if (task.State != TaskState.Postponed)
					{
						m_Tasks.Remove(task);
					}
				}
				finally
				{
					m_QueueLocker.ReleaseWriterLock();
				}
			}
			return result;
		}

		internal void AddTasksToPostponed(BaseTask[] tasks)
		{
			m_PostponedQueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				m_PostponedTasks.AddRange(tasks);
				m_PostponedTasks.Sort(this.CompareTaskByBeginDate);
			}
			catch (Exception exc)
			{
				ReportError("Błąd podczas dodawania zadań do kolejki zadań uśpionych !", exc);
			}
			finally
			{
				m_PostponedQueueLocker.ReleaseWriterLock();
			}
		}

		private void RemoveFirstTasksFromPostponed(int firstTasksCount)
		{
			if (firstTasksCount < 1)
				return;

			m_PostponedQueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				if (m_PostponedTasks.Count > 0)
					m_PostponedTasks.RemoveRange(0, Math.Min(firstTasksCount, m_PostponedTasks.Count));
			}
			finally
			{
				m_PostponedQueueLocker.ReleaseWriterLock();
			}
		}

		internal void RemoveTaskFromPostponed(BaseTask task)
		{
			if (task == null)
				return;

			m_PostponedQueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				m_PostponedTasks.Remove(task);
			}
			finally
			{
				m_PostponedQueueLocker.ReleaseWriterLock();
			}
		}

		internal BaseTask[] GetAndRemovePostponedTaskToRun()
		{
			List<BaseTask> ar = new List<BaseTask>();
			try
			{
				BaseTask[] allPostponed = GetAllPostponedTask();

				if (allPostponed != null && allPostponed.Length > 0)
				{
					int idx = 0;
					while (idx < allPostponed.Length)
					{
						BaseTask task = allPostponed[idx];
						if (task.BeginDate <= DateTime.Now)
						{
							task.State = TaskState.Waiting;
							ar.Add(task);
						}
						else
						{
							break;
						}
						idx++;
					}
					RemoveFirstTasksFromPostponed(ar.Count);
				}
			}
			catch (Exception exc)
			{
				ReportError("Błąd podczas pobierania zadań do uruchomienia z kolejki zadań uśpionych !", exc);
			}
			return ar.ToArray();
		}

		public void RemoveTasksFromMainQueue(long[] TaskIDs)
		{
			List<string> signatures = new List<string>();
			ArrayList sigToRemove = new ArrayList();
			m_QueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				int foundDem = 0;
				long id;
				for (int i = 0; i < m_Tasks.Count; i++)
				{
					id = m_Tasks[i].ID;
					for (int j = 0; j < TaskIDs.Length; j++)
					{
						if (TaskIDs[j] == id)
						{
							m_FastSearchInMainQueue.Remove(id);
							m_Tasks.RemoveAt(i);
							i--;

							foundDem++;
							break;
						}
					}
					if (foundDem == TaskIDs.Length)
						break;
				}
			}
			finally
			{
				m_QueueLocker.ReleaseWriterLock();
			}
		}

		protected bool AddRangeToTasksQueue(BaseTask[] Tasks)
		{
			m_QueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				m_Tasks.Clear();
				m_Tasks.AddRange(Tasks);
				foreach (BaseTask de in Tasks)
					m_FastSearchInMainQueue[de.ID] = de;
			}
			catch (Exception exc)
			{
				ReportError("Błąd podczas budowania nowej kolejki żądań po odczycie z pliku !", exc);
				return false;
			}
			finally
			{
				m_QueueLocker.ReleaseWriterLock();
			}
			return true;
		}

		public BaseTask AddTask(BaseTask Task)
		{
			string errorMsg;
			int errorId;

			return AddTask(Task, out errorMsg, out errorId);
		}
		public BaseTask AddTask(BaseTask Task, out string errorMsg, out int errorId)
		{
			errorId = (int)StandardErrors.Success;
			errorMsg = "";
			if (Task == null)
				return null;

			Task.m_ManagerObj = this;
			Task.OrygBeginDate = Task.BeginDate;

			if (Task.IsMainEntryTask)
			{
				if (m_NumOfMainTasks >= m_MaxNumberOfMainTasksInQueue)
				{
					errorMsg = "Nie można dodać więcej zadań - kolejka przepełniona!";
					ShowText(errorMsg, TraceEventType.Warning);
					errorId = (int)StandardErrors.SystemOverloaded;
					return null;
				}
				else
					Interlocked.Increment(ref m_NumOfMainTasks);
			}

			if (Task.ID <= 0)
				Task.ID = m_IDGenerator.GenerateTimeBasedID();
			Task.CalculatePriority();
			if (Task.BeginDate <= DateTime.Now)
			{
				Task.State = TaskState.Waiting;
			}
			else	 // z opóźnionym rozpoczęciem
			{
				Task.State = TaskState.Postponed;

				m_QueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
				try
				{
					m_FastSearchInMainQueue[Task.ID] = Task;
				}
				finally
				{
					m_QueueLocker.ReleaseWriterLock();
				}

				AddTasksToPostponed(new BaseTask[] { Task });
				return Task;
			}

			bool added = AddTasks(new BaseTask[] { Task });
			if (added)
			{
				return Task;
			}
			else
			{
				errorMsg = "Nie powidoło się dodanie nowego zadaia do kolejki zadań !";
				errorId = (int)StandardErrors.Runtime;
				ShowText(errorMsg, TraceEventType.Warning);
			}

			return null;
		}

		protected bool AddTasks(BaseTask[] Tasks)
		{

			m_QueueLocker.AcquireWriterLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				foreach (BaseTask Task in Tasks)
				{
					Task.m_ManagerObj = this;
					Task.State = TaskState.Waiting;
					m_FastSearchInMainQueue[Task.ID] = Task;
					int i = 0;
					bool added = false;
					while (i < m_Tasks.Count)
					{
						BaseTask localTask = (BaseTask)m_Tasks[i];
						if (localTask.Priority < Task.Priority)
						{
							m_Tasks.Insert(i, Task);
							added = true;
							break;
						}
						i++;
					}
					if (!added)
						m_Tasks.Add(Task);

					m_NewTaskAdded.Set();
				}
				return true;
			}
			catch (Exception exc)
			{
				ReportError("Błąd podczas dodawania zadania do kolejki zadań !", exc);
			}
			finally
			{
				m_QueueLocker.ReleaseWriterLock();
			}

			return false;
		}

		protected void CheckPostponedTasks()
		{
			BaseTask[] TasksToRun = GetAndRemovePostponedTaskToRun();
			if (TasksToRun != null && TasksToRun.Length > 0)
			{
				AddTasks(TasksToRun);
			}
		}

		private void CheckTasksInThread(Object state)
		{
			lock (this)
			{
				if (m_CheckTasksInProgress)
					return;
				m_CheckTasksInProgress = true;
			}

			if (m_StopWorking)
				return;

			lock (m_RunningTasks)
				m_RunningTasks.RemoveAll(bt => bt.State != TaskState.Running);

			if (m_StopWorking)
				return;

			if (m_CheckIfRunningTasksAreLate)
			{
				lock (m_ExecuteThreads)
				{
					try
					{
						for (int i = 0; i < m_ExecuteThreads.Count; i++)
						{
							if (m_ExecuteThreads[i] != null)
							{
								if (m_ExecuteThreads[i].m_StartRuningTask < DateTime.Now
									&& m_ExecuteThreads[i].m_TaskThread != null
									&& m_ExecuteThreads[i].m_CurrentTask != null)
								{
									TimeSpan ts = DateTime.Now - m_ExecuteThreads[i].m_StartRuningTask;
									if (ts.TotalSeconds > m_MaxTimeOfWorkingThreadExecution
										|| (m_ExecuteThreads[i].m_CurrentTask.ExecutionTimeoutAlternate > 0 && ts.TotalSeconds >= m_ExecuteThreads[i].m_CurrentTask.ExecutionTimeoutAlternate))
									{
										m_ExecuteThreads[i].m_CurrentTask.TotalRunningTime += ts.TotalSeconds;
										if (m_ExecuteThreads[i].m_CurrentTask.CanBeRunningLonger(m_ExecuteThreads[i].m_CurrentTask.TotalRunningTime))
										{
											ShowText(string.Format("Zadanie zadecydowało o przedłużeniu wykonywania (trwa już {0} s, ostatni przedział = {1} s przy max = {2}, start = {3}).",
																	m_ExecuteThreads[i].m_CurrentTask.TotalRunningTime,
																	ts.TotalSeconds,
																	m_MaxTimeOfWorkingThreadExecution,
																	m_ExecuteThreads[i].m_StartRuningTask),
													TraceEventType.Information);

											m_ExecuteThreads[i].m_StartRuningTask = DateTime.Now;
											continue;
										}

										ShowText(string.Format("Ubicie wątka wykonawczego [{4}] - wykonywanie zadania nr {5} trwa już zbyt długo (łącznie {0} s, ostatni przedział = {1} s przy max = {2}/{6} s, start = {3}).",
																m_ExecuteThreads[i].m_CurrentTask.TotalRunningTime,
																ts.TotalSeconds,
																m_MaxTimeOfWorkingThreadExecution,
																m_ExecuteThreads[i].m_StartRuningTask,
																m_ExecuteThreads[i].m_TaskThread.Name,
																m_ExecuteThreads[i].m_CurrentTask.ID,
																m_ExecuteThreads[i].m_CurrentTask.ExecutionTimeoutAlternate),
												TraceEventType.Error);
										m_TotalNumOfAbortedThreads++;
										m_ExecuteThreads[i].m_TaskThread.Abort();
										if (m_ExecuteThreads[i].m_CurrentTask != null)
										{
											lock (m_RunningTasks)
												m_RunningTasks.Remove(m_ExecuteThreads[i].m_CurrentTask);

											m_ExecuteThreads[i].m_CurrentTask.FinishTaskWithError((int)StandardErrors.ExecutionTimeout,
												string.Format("Wykonanie zdania trwało zbyt długo: {0} s na {1} s MAX.", m_ExecuteThreads[i].m_CurrentTask.TotalRunningTime,
															Math.Min(m_MaxTimeOfWorkingThreadExecution,
																	m_ExecuteThreads[i].m_CurrentTask.ExecutionTimeoutAlternate > 0 ? m_ExecuteThreads[i].m_CurrentTask.ExecutionTimeoutAlternate : Int32.MaxValue)));

											if (m_ExecuteThreads[i].m_CurrentTask.State != TaskState.Finished)
												m_ExecuteThreads[i].m_CurrentTask.State = TaskState.WaitingForFinish;

											m_ExecuteThreads[i].m_CurrentTask = null;
										}
										m_ExecuteThreads[i].m_TaskThread = null;
										InitTaskThread(i);
									}
								}
								else if (m_ExecuteThreads[i].m_TaskThread == null)
									InitTaskThread(i);
							}
							else
								InitTaskThread(i);
						}
					}
					catch (ThreadAbortException)
					{
						ShowText("Wątek przeglądający wątki wykonawcze zatrzymany!", TraceEventType.Information);
						Thread.ResetAbort();
						return;
					}
					catch (Exception exc)
					{
						ReportError("Błąd podczas sprawdzania, czy wątki wykonawncze nie działają zbyt długo!", exc);
					}
				}
			}


			lock (this)
			{
				m_CheckTasksInProgress = false;
			}
		}

		void InitTaskThread(int idxInThreadsTable)
		{
			if (m_ExecuteThreads[idxInThreadsTable] == null)
				m_ExecuteThreads[idxInThreadsTable] = new TaskThreadInfo();

			m_ExecuteThreads[idxInThreadsTable].m_StartRuningTask = DateTime.MaxValue;
			m_ExecuteThreads[idxInThreadsTable].m_CurrentTask = null;
			m_ExecuteThreads[idxInThreadsTable].m_ShouldWork = true;
			m_ExecuteThreads[idxInThreadsTable].m_TaskThread = new Thread(new ThreadStart(ExecuteTaskInThread));
			m_ExecuteThreads[idxInThreadsTable].m_TaskThread.IsBackground = true;
			m_ExecuteThreads[idxInThreadsTable].m_TaskThread.Name = idxInThreadsTable.ToString();
			m_ExecuteThreads[idxInThreadsTable].m_TaskThread.Start();
		}

		public BaseTask[] GetAllTasksInQueue(bool withoutRunningTasks, bool withPostponed)
		{
			List<BaseTask> arTmp = new List<BaseTask>();
			m_QueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				if (withoutRunningTasks)
				{
					for (int i = 0; i < m_Tasks.Count; i++)
					{
						if (m_Tasks[i].State != TaskState.Running && m_Tasks[i].State != TaskState.Unknown)
							arTmp.Add(m_Tasks[i]);
					}
				}
				else
					arTmp.AddRange(m_Tasks);
			}
			finally
			{
				m_QueueLocker.ReleaseReaderLock();
			}

			if (withPostponed)
			{
				m_PostponedQueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
				try
				{
					arTmp.AddRange(m_PostponedTasks);
				}
				finally
				{
					m_PostponedQueueLocker.ReleaseReaderLock();
				}
			}
			return arTmp.ToArray();
		}

		public BaseTask[] GetAllTasksWaitingForOther()
		{
			m_QueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				return m_Tasks.FindAll(bt => bt.State == TaskState.WaitingForOtherTask).ToArray();
			}
			finally
			{
				m_QueueLocker.ReleaseReaderLock();
			}
		}

		public BaseTask[] GetAllTasksWaitingForAnswer()
		{
			m_QueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				return m_Tasks.FindAll(bt => bt.State == TaskState.WaitingForAnswer).ToArray();
			}
			finally
			{
				m_QueueLocker.ReleaseReaderLock();
			}
		}

		public BaseTask[] GetAllTasksOfGivenType(string typeName)
		{
			List<BaseTask> arTemp = new List<BaseTask>();
			m_QueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				arTemp.AddRange(m_Tasks.FindAll(bt => bt.Type == typeName));
			}
			finally
			{
				m_QueueLocker.ReleaseReaderLock();
			}


			m_PostponedQueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				arTemp.AddRange(m_PostponedTasks.FindAll(bt => bt.Type == typeName));
			}
			finally
			{
				m_PostponedQueueLocker.ReleaseReaderLock();
			}
			return arTemp.ToArray();
		}

		public BaseTask[] GetAllTasksOfGivenType(Type type)
		{
			List<BaseTask> arTemp = new List<BaseTask>();
			m_QueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				arTemp.AddRange(m_Tasks.FindAll(bt => bt.GetType() == type));
			}
			finally
			{
				m_QueueLocker.ReleaseReaderLock();
			}


			m_PostponedQueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				arTemp.AddRange(m_PostponedTasks.FindAll(bt => bt.GetType() == type));
			}
			finally
			{
				m_PostponedQueueLocker.ReleaseReaderLock();
			}
			return arTemp.ToArray();
		}

		internal BaseTask[] GetAllPostponedTask()
		{
			BaseTask[] result = null;
			m_PostponedQueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				result = m_PostponedTasks.ToArray();
			}
			finally
			{
				m_PostponedQueueLocker.ReleaseReaderLock();
			}
			return result;
		}

		public int GetNumberOfTasks(out int postponed, out int finished, out int waiting, out int running, out int waitingForOther, out int waitingForAnswer)
		{
			int i = 0;
			postponed = 0;
			finished = 0;
			waiting = 0;
			waitingForOther = 0;
			waitingForAnswer = 0;
			running = 0;

			BaseTask[] Tasks = GetAllTasksInQueue(false, true);
			if (Tasks != null)
			{
				try
				{
					while (i < Tasks.Length)
					{
						BaseTask Task = (BaseTask)Tasks[i];
						if (Task != null)
						{
							if (Task.State == TaskState.Postponed)
								postponed++;
							else if (Task.State == TaskState.Finished
								|| Task.State == TaskState.WaitingForFinish)
								finished++;
							else if (Task.State == TaskState.Waiting)
								waiting++;
							else if (Task.State == TaskState.Running)
								running++;
							else if (Task.State == TaskState.WaitingForAnswer
								|| Task.State == TaskState.WaitingForOtherTask)
								waitingForAnswer++;
							else if (Task.State == TaskState.WaitingForOtherTask)
								waitingForOther++;
						}
						i++;
					}
					return Tasks.Length;
				}
				catch (Exception exc)
				{
					ReportError("Błąd podczas przeglądania kolejki zadań !", exc);
				}
			}
			return -1;
		}

		public int GetNumberOfTasksWithState(TaskState[] states)
		{
			int i = 0;
			int count = 0;

			BaseTask[] Tasks = GetAllTasksInQueue(false, true);
			if (Tasks != null)
			{
				try
				{
					while (i < Tasks.Length)
					{
						BaseTask Task = (BaseTask)Tasks[i];
						if (Task != null)
						{
							if (Array.IndexOf<TaskState>(states, Task.State) >= 0)
								count++;
						}
						i++;
					}
					return count;
				}
				catch (Exception exc)
				{
					ReportError("Błąd podczas przeglądania kolejki zadań !", exc);
				}
			}
			return -1;
		}

		public bool ShouldWaitForOtherTask(BaseTask askingTask, BaseTask[] tasks)
		{
			if (!askingTask.CanWaitForOtherTask)
				return false;

			try
			{
				foreach (BaseTask TaskTmp in tasks)
				{
					if (TaskTmp != null && TaskTmp != askingTask)
					{
						if (TaskTmp.State == TaskState.Running || TaskTmp.State == TaskState.WaitingForAnswer || TaskTmp.State == TaskState.Waiting)
						{
							if (TaskTmp.ShouldAskingTaskWaitForMe(askingTask))
							{
								if (TaskTmp.State == TaskState.Running || TaskTmp.State == TaskState.WaitingForAnswer || TaskTmp.State == TaskState.Waiting)
								{
									TaskTmp.AddOtherTaskIDWaitingForMe(askingTask.ID);
									askingTask.OtherTaskID = TaskTmp.ID;
									askingTask.State = TaskState.WaitingForOtherTask;
									ShowText(string.Format("Zadanie o nr {0} w stanie '{2}' będzie czekać na koniec zadania nr {1} w stanie '{3}'!", askingTask.ID, TaskTmp.ID, askingTask.State, TaskTmp.State), TraceEventType.Verbose);
									return true;
								}
							}
						}
					}
				}
			}
			catch (Exception exc)
			{
				ReportError("Błąd podczas przeszukiwania kolejki zadań w celu znalezienia zadania, na które powinno się zaczekać !", exc);
			}
			return false;
		}

		public void OnMessageForTask(MessageBody message)
		{
			if (message == null)
				return;

			if (message.m_ReceivingTaskID > -1)
			{
				BaseTask Task = FindTaskInMainQueue(message.m_ReceivingTaskID);

				if (Task != null && (Task.State != TaskState.Finished))
				{
					if (Task.OnMessageReceived(message))
						Task.State = TaskState.WaitingForFinish;
				}
			}
		}

		public BaseTask FindTask(long TaskID)
		{
			return FindTaskInMainQueue(TaskID);
		}

		public BaseTask FindTaskInMainQueue(long TaskID)
		{
			m_QueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
			try
			{
				int i = 0;
				BaseTask Task = (BaseTask)m_FastSearchInMainQueue[TaskID];
				if (Task == null || Task.State == TaskState.Unknown)
				{
					while (i < m_Tasks.Count)
					{
						if (m_Tasks[i] != null && m_Tasks[i].ID == TaskID && m_Tasks[i].State != TaskState.Unknown)
						{
							Task = m_Tasks[i];
							break;
						}
						i++;
					}
				}
				return Task;
			}
			catch (Exception exc)
			{
				ReportError("Błąd podczas przeglądania kolejki żądań !", exc);
			}
			finally
			{
				m_QueueLocker.ReleaseReaderLock();
			}
			return null;
		}

		public bool IsTaskStillWorking(long TaskID, out int errorId, out string errorDesc)
		{
			errorId = (int)StandardErrors.Success;
			errorDesc = "";

			ShowText(string.Format("Próba odpytania o stan zadania '{0}'!", TaskID), TraceEventType.Verbose);

			TaskStateInfo tsi = GetCurrentTaskState(TaskID);
			if (tsi != null)
			{
				ShowText(string.Format("Zadanie o ID={0} ma stan: Finished={1}, ErrorType={2}.", TaskID, tsi.IsFinished, errorId), TraceEventType.Verbose);

				errorDesc = tsi.ErrorDesc;
				errorId = tsi.ErrorCode;
				if (!tsi.IsFinished)
					return true;
			}
			else
			{
				errorDesc = string.Format("Zadanie potomne o ID={0} nie istnieje w systemie!", TaskID);
			}

			return false;
		}

		public TaskStateInfo GetCurrentTaskState(long taskID)
		{
			BaseTask task = FindTask(taskID);
			if (task != null)
			{
				TaskStateInfo tsi = new TaskStateInfo(taskID);
				tsi.ErrorDesc = task.ErrorDesc;
				tsi.IsFinished = task.State == TaskState.Finished;
				tsi.ErrorCode = task.CodeOfError;
				tsi.AddInfo = task.AddInfo;
				tsi.Result = task.Result;
				return tsi;
			}
			return null;
		}

		public bool StopWorkingNow()
		{
			string description = "";
			return StopWorkingNow(10000, out description);
		}

		public bool StopWorkingNow(int timeout, out string description)
		{
			description = "";
			bool result = true;
			try
			{
				m_StopWorking = true;
				m_NewTaskAdded.Set();

				ShowText("Zgłoszono rozkaz zatrzymania systemu kolejkowego! Nastąpi wygaszenie obsługi zadań.", TraceEventType.Information);

				DeInitExecuteMech();
				if (!m_WorkIsFinished.WaitOne(timeout, false))
					description = "System nie zakończył wszystkich wykonywanych zadan! Niektóre zadania mogły zaginąć!";
				else
					description = "Zakończono wszystkie wykonywane zadania!";

				ShowText(description, TraceEventType.Verbose);

				if (m_WrokingThread != null)
					if (!m_WrokingThread.Join(3000))
						m_WrokingThread.Abort();
			}
			catch (Exception ex)
			{
				ReportError("Błąd podczas zatrzymywania pracy zarządcy kolejki zadań!", ex);
				result = false;
			}
			ShowText("Systemu kolejkowy został zatrzymany!", TraceEventType.Information);
			return result;
		}

		private void WorkForTasks()
		{
			int i;
			while (true)
			{
				try
				{
					m_NewTaskAdded.WaitOne(m_QueueCheckingInterval, true);

					if (m_StopWorking)
					{
						m_WorkIsFinished.Set();
						return;
					}
					
					BaseTask[] arAllTasks = GetAllTasksInQueue(false, false);
					if (arAllTasks != null)
					{
						BaseTask[] finishing = arAllTasks.Where(rt => rt.State == TaskState.WaitingForFinish || rt.State == TaskState.Finished).ToArray();
						BaseTask[] restTasks = arAllTasks.Except(finishing).ToArray();

						int iWorkingCount = 0;
						try
						{
							bool isAvailable = true;
							if (finishing != null)
							{
								i = 0;
								while (i < finishing.Length)
								{
									BaseTask Task = (BaseTask)finishing[i];
									if (Task != null)
									{
										try
										{
											if (Task.IsWorking())
												iWorkingCount++;

											if (!m_StopWorking)
											{
												if (Task.State == TaskState.Finished)
												{
													RemoveTask(Task, false);
												}
												else if (Task.State == TaskState.WaitingForFinish)
												{
													if (!EnqueueTaskForExecution(Task))
													{
														Task.State = TaskState.WaitingForFinish;
														isAvailable = false;
													}
												}
											}
										}
										catch (ThreadAbortException taexc)
										{
											ReportError("Wątek przeglądający kolejkę zadań zatrzymany!", taexc);
											Thread.ResetAbort();
											return;
										}
										catch (Exception exc)
										{
											ReportError("Błąd w wątku przeglądającym kolejkę zadań przy rozpatrywaniu zadania o ID=" + Task.ID.ToString(), exc);
											Task.CodeOfError = (int)StandardErrors.Runtime;
											Task.ErrorDesc = "Błąd w wątku przeglądającym kolejkę zadań!";
											Task.State = TaskState.WaitingForFinish;
										}
									}
									i++;
								}
							}

							i = 0;
							while (i < restTasks.Length)
							{
								BaseTask Task = (BaseTask)restTasks[i];
								if (Task != null)
								{
									try
									{
										if (Task.IsWorking())
											iWorkingCount++;

										if (!m_StopWorking)
										{
											if (Task.State == TaskState.Postponed)
											{
												if (Task.BeginDate <= DateTime.Now)
													Task.State = TaskState.Waiting;
												else
													RemoveTask(Task, true);
											}

											if (Task.State != TaskState.Waiting
												&& Task.State != TaskState.Finished
												&& Task.State != TaskState.WaitingForFinish)
											{
												Task.ChangeStateIfTimeIsValid();
											}

											if (Task.State == TaskState.Finished)
											{
												RemoveTask(Task, false);
											}
											else if (Task.State == TaskState.Waiting)
											{
												if (isAvailable)
													Task.TryToRun();
											}
											else if (Task.State == TaskState.WaitingForFinish)
											{
												if (!EnqueueTaskForExecution(Task))
												{
													isAvailable = false; ;
													Task.State = TaskState.WaitingForFinish;
												}
											}
										}
									}
									catch (ThreadAbortException taexc)
									{
										ReportError("Wątek przeglądający kolejkę zadań zatrzymany!", taexc);
										Thread.ResetAbort();
										return;
									}
									catch (Exception exc)
									{
										ReportError("Błąd w wątku przeglądającym kolejkę zadań przy rozpatrywaniu zadania o ID=" + Task.ID.ToString(), exc);
										Task.CodeOfError = (int)StandardErrors.Runtime;
										Task.ErrorDesc = "Błąd w wątku przeglądającym kolejkę zadań!";
										Task.State = TaskState.WaitingForFinish;
									}
								}
								i++;
							}
						}
						catch (ThreadAbortException taexc)
						{
							ReportError("Wątek przeglądający kolejkę zadań zatrzymany!", taexc);
							Thread.ResetAbort();
							return;
						}
						catch (Exception exc)
						{
							ReportError("Błąd w obsłudze kolejki zadań !", exc);
						}
						if (m_StopWorking)
						{
							if (iWorkingCount == 0)
								m_WorkIsFinished.Set();
							return;
						}

					}

					CheckPostponedTasks();
				}
				catch (ThreadAbortException taexc)
				{
					ReportError("Wątek obsługujący zadania zatrzymany!", taexc);
					Thread.ResetAbort();
					return;
				}
				catch (Exception excMan)
				{
					ReportError("Błąd w wątku obsługującym kolejkę zadań z kolejki !", excMan);
				}
				if (m_StopWorking)
				{
					m_WorkIsFinished.Set();
					return;
				}
			}
		}

		public void ShowText(string msg, TraceEventType level)
		{
			if (ShowTextEvent != null)
				ShowTextEvent(msg, level);
		}

		public void ReportError(string msg, Exception ex)
		{
			if (ReportErrorEvent != null)
				ReportErrorEvent(msg, ex);
		}

		private int CompareTaskByBeginDate(BaseTask x, BaseTask y)
		{
			return x.BeginDate.CompareTo(y.BeginDate);
		}

		public void DumpState(Object state)
		{
			if (string.IsNullOrEmpty(m_DumpStateFilePath))
			{
				ShowText("Brak parametru okreslającego ścieżkę do pliku DUMP!", TraceEventType.Warning);
				return;
			}
			lock (this)
			{
				if (m_DumpStateInProgress)
					return;
				m_DumpStateInProgress = true;
			}
			try
			{
				ShowText("Zrzut stanu kolejki zadań do pliku!", TraceEventType.Verbose);
				StringBuilder sb = new StringBuilder(8000);

				sb.Append("------------- Zrzut stanu kolejki -------------\r\n");
				sb.AppendFormat(" Data: {0}\r\n", DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss.f"));
				sb.AppendFormat(" CategoryName = {0}\r\n", m_CategoryName);
				sb.AppendFormat(" NumOfWorkingThreads = {0}\r\n", m_NumOfWorkingThreads);
				sb.AppendFormat(" StopWorking = {0}\r\n", m_StopWorking);
				sb.AppendFormat(" TaskWaitingForOtherTimeout = {0} s\r\n", m_TaskWaitingForOtherTimeout);
				sb.AppendFormat(" TaskWaitingForAnswerTimeout = {0} s\r\n", m_TaskWaitingForAnswerTimeout);
				sb.AppendFormat(" MaxTimeOfWorkingThreadExecution = {0} s\r\n", m_MaxTimeOfWorkingThreadExecution);
				sb.AppendFormat(" TotalNumOfFinishedTasksWithError = {0}\r\n", m_TotalNumOfFinishedTasksWithError);
				sb.AppendFormat(" TotalNumOfFinishedTasks = {0}\r\n", m_TotalNumOfFinishedTasks);
				sb.AppendFormat(" TotalNumOfAbortedThreads = {0}\r\n", m_TotalNumOfAbortedThreads);
				sb.AppendFormat(" TotalNumOfTimeoutedTasks = {0}\r\n", m_TotalNumOfTimeoutedTasks);
				sb.AppendFormat(" NumOfMainTasks = {0}\r\n", m_NumOfMainTasks);
				sb.AppendFormat(" CheckIfRunningTasksAreLate = {0}\r\n", m_CheckIfRunningTasksAreLate);
				sb.AppendFormat(" DumpStateIterval = {0} s\r\n", m_DumpStateIterval);



				object[] queue4RunningTasks = null;
				lock (m_QueueToExecute.SyncRoot)
					queue4RunningTasks = m_QueueToExecute.ToArray();
				sb.AppendFormat(" > Lista zadań przenaczonych do pobrania przez wątki wykonawcze ({0}):\r\n", queue4RunningTasks.Count());
				foreach (BaseTask bt in queue4RunningTasks)
					sb.AppendFormat("  #ID = {0}, State = {1}, Type = {5}, HandlingStarted = {2}, LastExecStart = {3}, LastExecEnd = {4}, ExecCount = {6} #\r\n", bt.ID, bt.State, bt.HandlingStarted.ToString("dd/MM/yyyy  HH:mm:ss.f"), bt.LastExecutionThreadAssignmentDate.ToString("HH:mm:ss.f"), bt.LastExecutionThreadLeftDate.ToString("HH:mm:ss.f"), bt.Type, bt.ExecThreadOwningCount);
				sb.Append(" <\r\n");




				BaseTask[] runningTasks = null;
				lock (m_RunningTasks)
					runningTasks = m_RunningTasks.ToArray();
				sb.AppendFormat(" > Lista zadań posiadających wątki wykonawcze ({0}):\r\n", runningTasks.Count());
				foreach (var bt in runningTasks)
					sb.AppendFormat("  #ID = {0}, State = {1}, Type = {5}, HandlingStarted = {2}, LastExecStart = {3}, LastExecEnd = {4}, ExecCount = {6} #\r\n", bt.ID, bt.State, bt.HandlingStarted.ToString("dd/MM/yyyy  HH:mm:ss.f"), bt.LastExecutionThreadAssignmentDate.ToString("HH:mm:ss.f"), bt.LastExecutionThreadLeftDate.ToString("HH:mm:ss.f"), bt.Type, bt.ExecThreadOwningCount);
				sb.Append(" <\r\n");

				BaseTask[] postponed = null;
				m_PostponedQueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
				try
				{
					postponed = m_PostponedTasks.ToArray();
				}
				finally
				{
					m_PostponedQueueLocker.ReleaseReaderLock();
				}
				sb.AppendFormat(" > Lista zadań uśpionych ({0}):\r\n", postponed.Count());
				foreach (var bt in postponed)
					sb.AppendFormat("   #ID = {0}, State = {1}, Type = {4}, BeginDate = {2}, LastExecEnd = {3} #\r\n", bt.ID, bt.State, bt.BeginDate.ToString("dd/MM/yyyy  HH:mm:ss.f"), bt.LastExecutionThreadAssignmentDate.ToString("HH:mm:ss.f"), bt.Type);
				sb.Append(" <\r\n");


				BaseTask[] allTasks;
				m_QueueLocker.AcquireReaderLock(TaskQueueManager.LockWaitingTimeout);
				try
				{
					allTasks = m_Tasks.ToArray();
				}
				finally
				{
					m_QueueLocker.ReleaseReaderLock();
				}
				sb.AppendFormat(" > Cała kolejka zadań ({0}):\r\n", allTasks.Count());
				foreach (var bt in allTasks)
					bt.DumpState(sb);
				sb.Append(" <\r\n");
				sb.Append("------------------------------------------------------------------------------------------------------------------------------\r\n");


				System.IO.FileInfo fi = new System.IO.FileInfo(m_DumpStateFilePath);
				if (fi.Exists)
				{
					try
					{
						fi.CopyTo(m_DumpStateFilePath + "_bak", true);
					}
					catch (Exception exI)
					{
						ReportError(string.Format("Błąd podczas tworzenia kopii pliku '{0}' ze stanem kolejki.", m_DumpStateFilePath), exI);
					}
				}

				using (System.IO.FileStream fs = new System.IO.FileStream(m_DumpStateFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
				{
					System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);
					sw.Write(sb.ToString());
					sw.Flush();
					sw.Close();
				}
			}
			catch (Exception ex)
			{
				ReportError(string.Format("Błąd podczas zrzutu stanu kolejki do pliku {0}.", m_DumpStateFilePath), ex);
			}

			m_DumpStateLastDate = DateTime.Now;

			lock (this)
				m_DumpStateInProgress = false;

		}

		public void CancelAllRunningTasks(long exceptTaskID)
		{
			ShowText(string.Format("Ubicie wątków wykonawczych wszystkich obecnie wykonywanych zadań za wyjątkeim zadania o ID={0}.", exceptTaskID), TraceEventType.Information);
			lock (m_ExecuteThreads)
			{
				try
				{
					for (int i = 0; i < m_ExecuteThreads.Count; i++)
					{
						if (m_ExecuteThreads[i] != null)
						{
							if (m_ExecuteThreads[i].m_StartRuningTask < DateTime.Now
								&& m_ExecuteThreads[i].m_TaskThread != null
								&& m_ExecuteThreads[i].m_CurrentTask != null
								&& m_ExecuteThreads[i].m_CurrentTask.ID != exceptTaskID)
							{
								m_TotalNumOfAbortedThreads++;
								m_ExecuteThreads[i].m_TaskThread.Abort();
								if (m_ExecuteThreads[i].m_CurrentTask != null)
								{
									lock (m_RunningTasks)
										m_RunningTasks.Remove(m_ExecuteThreads[i].m_CurrentTask);

									if (m_ExecuteThreads[i].m_CurrentTask.State == TaskState.Running)
									{
										m_ExecuteThreads[i].m_CurrentTask.State = TaskState.WaitingForFinish;
									}
								}
								m_ExecuteThreads[i].m_ShouldWork = true;
								m_ExecuteThreads[i].m_TaskThread = new Thread(new ThreadStart(ExecuteTaskInThread));
								m_ExecuteThreads[i].m_TaskThread.IsBackground = true;
								m_ExecuteThreads[i].m_TaskThread.Name = i.ToString();
								m_ExecuteThreads[i].m_CurrentTask = null;
								m_ExecuteThreads[i].m_TaskThread.Start();
							}
						}
						else
							break;
					}
				}
				catch (Exception exc)
				{
					ReportError(string.Format("Błąd podczas ubijania wątków właśnie wykonujących zadania o ID różnym od {0}.", exceptTaskID), exc);
				}
			}
		}

		public void CancelAllRunningTasksOfType(Type[] taskTypes)
		{
			ShowText(string.Format("Ubicie wątków wykonawczych wszystkich obecnie wykonywanych zadań typu: {0}.", string.Join(",", taskTypes.Select( tt => tt.ToString()).ToArray())), TraceEventType.Information);
			lock (m_ExecuteThreads)
			{
				try
				{
					for (int i = 0; i < m_ExecuteThreads.Count; i++)
					{
						if (m_ExecuteThreads[i] != null)
						{
							if (m_ExecuteThreads[i].m_StartRuningTask < DateTime.Now
								&& m_ExecuteThreads[i].m_TaskThread != null
								&& m_ExecuteThreads[i].m_CurrentTask != null)
							{
								if (!taskTypes.Contains(m_ExecuteThreads[i].m_CurrentTask.GetType()))
									continue;

								m_TotalNumOfAbortedThreads++;
								m_ExecuteThreads[i].m_TaskThread.Abort();
								if (m_ExecuteThreads[i].m_CurrentTask != null)
								{
									lock (m_RunningTasks)
										m_RunningTasks.Remove(m_ExecuteThreads[i].m_CurrentTask);

									if (m_ExecuteThreads[i].m_CurrentTask.State == TaskState.Running)
										m_ExecuteThreads[i].m_CurrentTask.State = TaskState.WaitingForFinish;
								}
								m_ExecuteThreads[i].m_ShouldWork = true;
								m_ExecuteThreads[i].m_TaskThread = new Thread(new ThreadStart(ExecuteTaskInThread));
								m_ExecuteThreads[i].m_TaskThread.IsBackground = true;
								m_ExecuteThreads[i].m_TaskThread.Name = i.ToString();
								m_ExecuteThreads[i].m_CurrentTask = null;
								m_ExecuteThreads[i].m_TaskThread.Start();
							}
						}
						else
							break;
					}
				}
				catch (Exception exc)
				{
					ReportError("Błąd podczas ubijania wątków właśnie wykonujących zadania określonych typów.", exc);
				}
			}
		}

		public bool CancelRunningTask(long taskID)
		{
			ShowText(string.Format("Ubicie wątka wykonawczego obecnie wykonywanego zadania o ID={0}.", taskID), TraceEventType.Information);
			lock (m_ExecuteThreads)
			{
				try
				{
					for (int i = 0; i < m_ExecuteThreads.Count; i++)
					{
						if (m_ExecuteThreads[i] != null)
						{
							if (m_ExecuteThreads[i].m_StartRuningTask < DateTime.Now
								&& m_ExecuteThreads[i].m_TaskThread != null
								&& m_ExecuteThreads[i].m_CurrentTask != null)
							{
								if (m_ExecuteThreads[i].m_CurrentTask.ID == taskID)
								{
									m_TotalNumOfAbortedThreads++;
									m_ExecuteThreads[i].m_TaskThread.Abort();
									if (m_ExecuteThreads[i].m_CurrentTask != null)
									{
										lock (m_RunningTasks)
											m_RunningTasks.Remove(m_ExecuteThreads[i].m_CurrentTask);

										if (m_ExecuteThreads[i].m_CurrentTask.State == TaskState.Running)
											m_ExecuteThreads[i].m_CurrentTask.State = TaskState.WaitingForFinish;
									}
									m_ExecuteThreads[i].m_ShouldWork = true;
									m_ExecuteThreads[i].m_TaskThread = new Thread(new ThreadStart(ExecuteTaskInThread));
									m_ExecuteThreads[i].m_TaskThread.IsBackground = true;
									m_ExecuteThreads[i].m_TaskThread.Name = i.ToString();
									m_ExecuteThreads[i].m_CurrentTask = null;
									m_ExecuteThreads[i].m_TaskThread.Start();
									return true;
								}
							}
						}
						else
							break;
					}
				}
				catch (Exception exc)
				{
					ReportError("Błąd podczas ubijania wątka właśnie wykonującego zadanie o ID=" + taskID, exc);
				}
				return false;
			}
		}

		public int GetNumberOfTasksWithRunninThread()
		{
			int count = 0;

			try
			{
				TaskThreadInfo[] tti = null;
				lock (m_ExecuteThreads)
					tti = m_ExecuteThreads.ToArray();

				for (int i = 0; i < tti.Length; i++)
				{
					if (tti[i] != null)
					{
						if (tti[i].m_StartRuningTask < DateTime.Now
							&& tti[i].m_TaskThread != null
							&& tti[i].m_CurrentTask != null)
						{
							count++;
						}
					}
					else
						break;
				}
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return -1;
			}
			catch (Exception exc)
			{
				ReportError("Błąd podczas liczenia ile zadań posiada wątek wykonawczy!", exc);
			}

			return count;
		}
    }
}
