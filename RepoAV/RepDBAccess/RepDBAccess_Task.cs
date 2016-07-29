using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;
using System.Data;
using System.Data.SqlClient;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.RepDBAccess
{
	public partial class RepDBAccess : BaseDBAccess
    {
		public bool AddTask(TaskAdd t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu TaskAdd do metody AddTask.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"UniqueId",
																			"PublicId",
																			"Type",
																			"SupervisorId",
																			"BeginDate",
																			"PreferredNodeIds",
																			"CanSkipPreferredNodes",
																			"TaskSubtype");

			foreach (var de in pars)
				if (de.Key == "Id")
					de.Value.Direction = ParameterDirection.Output;
				else
					de.Value.Direction = ParameterDirection.Input;

			SqlHelper sh = new SqlHelper(m_ConnectionString);
			try
			{
				sh.BeginTransaction();
				ExecuteNonQueryInTransaction("dbo.AddTask", pars.Values.ToArray(), out ret, sh);

				if (ret == ErrorType.Success)
				{
					t.Id = (long)pars["Id"].Value;

					sh.Parameters.Clear();

					if (t.Content != null && t.Content.Count > 0)
					{
						SqlParameter[] ps = new SqlParameter[]
						{
							CreateSqlParameter("Id_Task", SqlDbType.BigInt, t.Id, ParameterDirection.Input),
							CreateSqlParameter("Key", SqlDbType.VarChar, 50, null, ParameterDirection.Input),
							CreateSqlParameter("Value", SqlDbType.NVarChar, -1, null, ParameterDirection.Input),
						};

						foreach (var de in t.Content)
						{
							ps[1].Value = de.Key;
							ps[2].Value = de.Value;

							ExecuteNonQueryInTransaction("dbo.AddTaskData", ps, out ret, sh);

							if (ret == ErrorType.Success)
								continue;
							else if (ret == ErrorType.InvalidParameter)
								OnErrorReport(ret, "Jeden z kluczy w danych zadania jest NULL. Dodanie zadania nie jest możliwe.");
							else if (ret == ErrorType.NotFound)
								OnErrorReport(ret, string.Format("Próba wprowadzenia danych dla nieistniejącego zadania o Id={0}.", t.Id));
							else if (ret == ErrorType.AlreadyExists)
								OnErrorReport(ret, string.Format("Próba wprowadzenia dwukrotnie danych z tym samym kluczem '{1}' dla zadania o Id={0}.", t.Id, de.Key ?? "NULL"));

							sh.Rollback();
							return false;
						}
					}

					sh.Commit();
					sh.Dispose();
					sh = null;
					return true;
				}
				else if (ret == ErrorType.MaterialNotFound)
					OnErrorReport(ret, string.Format("Nie udało się dodać zadania, gdyż nie istnieje material o Id={0}.", t.PublicId ?? "NULL"));
				else if (ret == ErrorType.FormatNotFound)
					OnErrorReport(ret, string.Format("Nie udało się dodać zadania, gdyż nie istnieje format o Id={0}.", t.UniqueId ?? "NULL"));
                else if (ret == ErrorType.FormatExists)
                    OnErrorReport(ret, string.Format("Nie udało się dodać zadania, gdyż istnieje już format o Id={0}.", t.UniqueId ?? "NULL"));
				else if (ret == ErrorType.AlreadyExists)
					OnErrorReport(ret, "Nie udało się dodać zadania, gdyż istnieje już inne zadanie tego samego typu w stanie New lub Executing.");
				else
					OnErrorReport(ret, string.Format("Niespodziewany błąd podczas dodawania zdadania. Sygnatura='{0}'.", t.GetSignature()));
			}
			catch(Exception ex)
			{
				sh.Rollback();
				throw ex;
			}

			return false;
		}

		public bool RemoveTask(long id_task)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody RemoveTask.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] { CreateSqlParameter("Id", SqlDbType.BigInt, id_task) };

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveTask", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje zadanie o Id={0}.", id_task));

			return false;
		}

		public bool SetTaskStatus(long id_task, TaskStatus oldStatus, TaskStatus newStatus)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody SetTaskStatus.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Task", SqlDbType.BigInt, id_task),
				CreateSqlParameter("NewStatus", SqlDbType.SmallInt, newStatus), 
				CreateSqlParameter("OldStatus", SqlDbType.SmallInt, oldStatus) 
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetTaskStatus", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje zadanie o Id={0}.", id_task));
			else// if (ret == ErrorType.StateChangedMeanwhile)
				OnErrorReport(ret, string.Format("Stan zadania o Id={0} został zmieniony w międzyczasie.", id_task));

			return false;
		}

		public bool TryToSetTaskStatus(long id_task, TaskStatus oldStatus, TaskStatus newStatus)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody SetTaskStatus.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Task", SqlDbType.BigInt, id_task),
				CreateSqlParameter("NewStatus", SqlDbType.SmallInt, newStatus), 
				CreateSqlParameter("OldStatus", SqlDbType.SmallInt, oldStatus) 
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetTaskStatus", pars, out ret);

			return true;
		}

		public bool SetTaskResult(long id_task, TaskStatus finishStatus, string result)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody SetTaskResult.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Task", SqlDbType.BigInt, id_task),
				CreateSqlParameter("Result", SqlDbType.NVarChar, -1, DBNull.Value), 
				CreateSqlParameter("Status", SqlDbType.SmallInt, finishStatus) 
			};

			if (!string.IsNullOrEmpty(result))
				pars[1].Value = result;

			ErrorType ret;
			ExecuteNonQuery("dbo.SetTaskResult", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje zadanie o Id={0}.", id_task));
			else// if (ret == ErrorType.StateChangedMeanwhile)
				OnErrorReport(ret, string.Format("Stan zadania o Id={0} został zmieniony w międzyczasie.", id_task));

			return false;
		}

		public bool FinishTask(long id_task, string result, bool success)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody FinishTask.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Task", SqlDbType.BigInt, id_task),
				CreateSqlParameter("Result", SqlDbType.NVarChar, -1, DBNull.Value), 
				CreateSqlParameter("Success", SqlDbType.Bit, success) 
			};

			if (!string.IsNullOrEmpty(result))
				pars[1].Value = result;


			ErrorType ret;
			ExecuteNonQuery("dbo.FinishTask", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje zadanie o Id={0}.", id_task));

			return false;
		}

		public bool RenewTask(long id_task, int newExecutingNodeId = -1)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody RenewTask.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Task", SqlDbType.BigInt, id_task),
				CreateSqlParameter("ExecutingNodeId", SqlDbType.Int, DBNull.Value), 
			};

			if (newExecutingNodeId > -1)
				pars[1].Value = newExecutingNodeId;


			ErrorType ret;
			ExecuteNonQuery("dbo.RenewTask", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.NodeNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0}.", newExecutingNodeId));
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje zadanie o Id={0}.", id_task));

			return false;
		}

		public bool SetTaskResultProcessedFlag(long id_task, bool resultProcessed)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody SetTaskResultProcessedFlag.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Task", SqlDbType.BigInt, id_task),
				CreateSqlParameter("ResultProcessed", SqlDbType.Bit, resultProcessed)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetTaskResultProcessed", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else// if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje zadanie o Id={0}.", id_task));

			return false;
		}


		public TaskCount[] GetTasksCount(TaskType? taskType, TaskStatus[] stats)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Type", SqlDbType.SmallInt, DBNull.Value),
				CreateSqlParameter("Statuses", SqlDbType.VarChar, 50, DBNull.Value)
			};

			if (taskType.HasValue)
				pars[0].Value = taskType;

			if (stats != null && stats.Length > 0)
			{
				string fullString = string.Join(";", stats.Select(s => ((short)s).ToString()).ToArray());
				pars[1].Value = fullString;
			}

			List<TaskCount> lst = ExecuteReaderToList<TaskCount>("dbo.GetTasksCount", pars);

			return lst.ToArray();
		}

		public TaskShort[] GetTasks2Execute(int id_Node, TaskTypeComplex[] taskTypes, int maxTaskCount)
		{
			if (id_Node < 0 || maxTaskCount < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawne parametry do metody GetTasks2Execute.");
				return null;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Node", SqlDbType.Int, id_Node),
				CreateSqlParameter("Count", SqlDbType.Int, maxTaskCount),
				CreateSqlParameter("Types", SqlDbType.VarChar, 100, DBNull.Value)
			};

			if (taskTypes != null)
				pars[2].Value = string.Join(";", taskTypes.Select(tt => string.Format("{0}:{1}", (short)tt.Type, tt.TaskSubtype ?? "")).ToArray());


			ErrorType et;
			List<TaskShort> lst = ExecuteReaderToList<TaskShort>("dbo.GetTasks2Execute", pars, out et);

			if (et == ErrorType.Success)
			{
				if (lst != null)
				{
					if (lst.Count > 0)
					{
						string allTaskIds = string.Join(";", lst.Select(tt => tt.Id.ToString()).ToArray());

						pars = new SqlParameter[] 
						{ 
							CreateSqlParameter("TaskIds", SqlDbType.VarChar, 250, allTaskIds)
						};

						List<TaskContent> tcs = ExecuteReaderToList<TaskContent>("dbo.GetTasksContent", pars, null);
						if (tcs != null)
						{
							foreach (var tc in tcs)
							{
								TaskShort ts = lst.FirstOrDefault(t => t.Id == tc.Id_Task);
								ts.Content.Add(tc.Key, tc.Value);
							}
						}

					}
					return lst.ToArray();
				}
			}
			else if (et == ErrorType.NodeNotFound)
				OnErrorReport(ErrorType.NodeNotFound, string.Format("Próba pobrania listy zadań dla nieistniejącego węzła o Id={0}.", id_Node));
			else
				OnErrorReport( et, string.Format("Niespodziewany błąd podczas pobierania listy zadań dla węzła o Id={0}.", id_Node));

			return null;
		}

		public Task GetTask(long id_task)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody GetTask.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<Task>(		"Id",
																					"Status",
																					"Result",
																					"PublicId",
																					"ExecutingNodeId",
																					"CreatedDate",
																					"TakenDate",
																					"FinishDate",
																					"Type",
																					"SupervisorId",
																					"ResultProcessed",
																					"BeginDate",
																					"UniqueId",
																					"PreferredNodeIds",
																					"CanSkipPreferredNodes",
																					"FormatId",
																					"TaskSubtype");


			foreach (var de in pars)
				if (de.Key == "Id")
					de.Value.Direction = ParameterDirection.Input;
				else
					de.Value.Direction = ParameterDirection.Output;
			pars["Id"].Value = id_task;

			SqlParameter[] ps = pars.Values.ToArray();

			ErrorType ret;
			Dictionary<string, string> cont = ExecuteReaderToDictionary("dbo.GetTask", ps, true, out ret, null);

			if (ret == ErrorType.Success)
			{
				Task t = CreateObject<Task>(ps);
				t.Content = cont;
				return t;
			}
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje zadanie o Id={0}.", id_task));

			return null;
		}

		public TaskShort[] GetManagerTasks4ResultProcessing(int id_Node)
		{
			if (id_Node < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody GetManagerTasks4ResultProcessing.");
				return null;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Node", SqlDbType.Int, id_Node)
			};

			List<TaskShort> lst = ExecuteReaderToList<TaskShort>("dbo.GetManagerTasks4ResultProcessing", pars);

			return lst.ToArray();
		}

		public TaskShort[] GetManagerTasks2Execute(int id_Node)
		{
			if (id_Node < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody GetManagerTasks2Execute.");
				return null;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Node", SqlDbType.Int, id_Node)
			};

			List<TaskShort> lst = ExecuteReaderToList<TaskShort>("dbo.GetManagerTasks2Execute", pars);

			return lst.ToArray();
		}

		public TaskShort[] GetTasksOfTypeShort(TaskType? taskType, string subtype = null)
		{
			SqlParameter[] pars = new SqlParameter[] 
            { 
                CreateSqlParameter("Type", SqlDbType.SmallInt, DBNull.Value) ,
				CreateSqlParameter("TaskSubtype", SqlDbType.VarChar, 50, DBNull.Value) 
            };
			if (taskType.HasValue)
				pars[0].Value = taskType.Value;

			if (!string.IsNullOrEmpty(subtype))
				pars[1].Value = subtype;

			List<TaskShort> lst = ExecuteReaderToList<TaskShort>("dbo.GetTasksOfType", pars);
			return lst.ToArray();
		}

		public Task[] GetTasksOfType(TaskType? taskType, string subtype = null)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Type", SqlDbType.SmallInt, DBNull.Value),
				CreateSqlParameter("TaskSubtype", SqlDbType.VarChar, 50, DBNull.Value) 
			};

			if (taskType.HasValue)
				pars[0].Value = (short)taskType.Value;
			if (!string.IsNullOrEmpty(subtype))
				pars[1].Value = subtype;

			List<Task> lst = ExecuteReaderToList<Task>("dbo.GetTasksOfType", pars);

			if (lst != null)
			{
				string ids = string.Join(";", lst.Select(t => t.Id.ToString()).ToArray());
				pars = new SqlParameter[] 
				{ 
					CreateSqlParameter("TaskIds", SqlDbType.VarChar, -1, ids)
				};

				List<TaskData> lstTD = ExecuteReaderToList<TaskData>("dbo.GetContent4Tasks", pars);

				if (lstTD != null)
				{
					foreach(var td in lstTD)
					{
						if (td.Id_Task > -1 && !string.IsNullOrEmpty(td.Key))
						{
							Task t = lst.Find(tt => tt.Id == td.Id_Task);
							if (t != null)
								t.Content.Add(td.Key, td.Value);
						}
					}
				}
			}

			return lst.ToArray();
		}

		public TaskShort[] GetTasksWithStatus(TaskStatus? taskStatus)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Status", SqlDbType.SmallInt, DBNull.Value)
			};

			if (taskStatus.HasValue)
				pars[0].Value = taskStatus.Value;

			List<TaskShort> lst = ExecuteReaderToList<TaskShort>("dbo.GetTasksWithStatus", pars);

			return lst.ToArray();
		}

		public bool UpdateTaskLastActivityDate(long id_task)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody UpdateTaskLastActivityDate.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Task", SqlDbType.BigInt, id_task)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.UpdateTaskLastActivityDate", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else// if (ret == ErrorType.StateChangedMeanwhile)
				OnErrorReport(ret, string.Format("Stan zadania o Id={0} został zmieniony w międzyczasie lub zadanie już nie istnieje.", id_task));

			return false;
		}

		public Task[] GetTasksOfType4Format(TaskType? taskType, string uniqueId)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Type", SqlDbType.SmallInt, DBNull.Value),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, DBNull.Value)
			};

			if (taskType.HasValue)
				pars[0].Value = (short)taskType.Value;
			if (!string.IsNullOrEmpty(uniqueId))
				pars[1].Value = uniqueId;

			List<Task> lst = ExecuteReaderToList<Task>("dbo.GetTasksOfType4Format", pars);

			if (lst != null)
			{
				string ids = string.Join(";", lst.Select(t => t.Id.ToString()).ToArray());
				pars = new SqlParameter[] 
				{ 
					CreateSqlParameter("TaskIds", SqlDbType.VarChar, -1, ids)
				};

				List<TaskData> lstTD = ExecuteReaderToList<TaskData>("dbo.GetContent4Tasks", pars);

				if (lstTD != null)
				{
					foreach (var td in lstTD)
					{
						if (td.Id_Task > -1 && !string.IsNullOrEmpty(td.Key))
						{
							Task t = lst.Find(tt => tt.Id == td.Id_Task);
							if (t != null)
								t.Content.Add(td.Key, td.Value);
						}
					}
                    List<TaskPreferredNodes> lstPN = ExecuteReaderToList<TaskPreferredNodes>("dbo.GetPreferredNodes4Tasks", pars);
                    if(lstPN != null)
                        foreach(Task task in lst)
                        {
                            int[] nodeList =  lstPN.Where(pn => pn.Id_Task == task.Id).Select(pn => pn.NodeId).ToArray();
                            if(nodeList.Length > 0)
                                task.PreferredNodeIds = nodeList;                            
                        }
				}
			}

			return lst.ToArray();
		}

		public void RemoveOldTasks(bool keepTasksWithError = true )
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("KeepTasksWithError", SqlDbType.Bit, keepTasksWithError)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveOldTasks", pars, out ret);
		}
    }
}
