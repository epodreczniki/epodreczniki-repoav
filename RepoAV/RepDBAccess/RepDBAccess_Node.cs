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
		public bool AddNode(NodeMod t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu NodeMod do metody AddNode.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"Role",
																			"ExternalAddress",
																			"InternalAddress",
																			"Url",
																			"Enabled",
																			"Name",
                                                                            "ProcaPortNumber");

			foreach (var de in pars)
                de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.AddNode", ps, out ret);

			if (ret == ErrorType.Success)
			{
				return true;
			}
			else
				OnErrorReport(ErrorType.General, string.Format("Niespodziewany błąd DB podczas dodawania węzła. Sygnatura='{0}'.", t.GetSignature()));			
			return false;
		}

		public bool RemoveNode(int id_Node)
		{
			if (id_Node < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody RemoveNode.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] { CreateSqlParameter("Id", SqlDbType.Int, id_Node) };

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveNode", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0}.", id_Node));

			return false;
		}

		public bool ChangeNode(NodeMod t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu NodeMod do metody ChangeNode.");
				return false;
			}
			if (t.Id < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody ChangeNode.");
				return false;
			}


			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters("Id",
																			"Role",
																			"ExternalAddress",
																			"InternalAddress",
																			"Url",
																			"Name",
                                                                            "ProcaPortNumber");

			foreach (var de in pars)
				if (de.Key == "Id")
					de.Value.Direction = ParameterDirection.Output;
				else
					de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.ChangeNode", ps, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ErrorType.General, string.Format("Nie isntieje węzeł o Id = {0}. Modyfikacja niemożliwa.", t.Id));
			return false;
		}

		public bool UpdateNodeCheckTime(int id_Node)
		{
			if (id_Node < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody SetNodeCheckTime.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Node", SqlDbType.Int, id_Node)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.UpdateNodeCheckTime", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else// if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0}.", id_Node));

			return false;
		}

		public Node GetNode(int id_Node)
		{
			if (id_Node < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody GetNode.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<Node>("Id",
																				"Role",
																				"ExternalAddress",
																				"InternalAddress",
																				"Url",
																				"Enabled",
																				"FreeSpace",
																				"IsOnline",
																				"Enabled",
																				"Name",
																				"TotalSpace",
                                                                                "ProcaPortNumber");


			pars["Id"].Value = id_Node;

			foreach (var de in pars)
				if (de.Key != "Id")
					de.Value.Direction = ParameterDirection.Output;
				else
					de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();

			ErrorType ret;
			ExecuteNonQuery("dbo.GetNode", ps, out ret);

			if (ret == ErrorType.Success)
			{
				Node t = CreateObject<Node>(ps);
				t.Id = id_Node;
				return t;
			}
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0}.", id_Node));

			return null;
		}

		public Node[] GetNodes(NodeRole? role)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Role", SqlDbType.SmallInt, DBNull.Value)
			};

			if (role.HasValue)
				pars[0].Value = role;

			List<Node> lst = ExecuteReaderToList<Node>("dbo.GetNodesInRole", pars);

			return lst.ToArray();
		}

		public bool SetNodeRepositorySpace(int id_Node, long totalSpace, long freeeSpace)
		{
			if (id_Node < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody SetNodeRepositorySpace.");
				return false;
			}
			if (freeeSpace < 0)
				freeeSpace = 0;

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Node", SqlDbType.Int, id_Node),
				CreateSqlParameter("FreeSpace", SqlDbType.BigInt, freeeSpace),
				CreateSqlParameter("TotalSpace", SqlDbType.BigInt, totalSpace)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetNodeRepositorySpace", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else// if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0}.", id_Node));

			return false;
		}

		public bool SetNodeEnabled(int id_Node, bool enabled)
		{
			if (id_Node < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody SetNodeEnabled.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Node", SqlDbType.Int, id_Node),
				CreateSqlParameter("Enabled", SqlDbType.Bit, enabled)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetNodeEnabled", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else// if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0}.", id_Node));

			return false;
		}

		public void UpdateRepositoryFreeSpace4Node(int id_Node, long freeSpace)
		{
			if (id_Node < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody UpdateRepositoryFreeSpace4Node.");
				return;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Node", SqlDbType.Int, id_Node),
				CreateSqlParameter("FreeSpace", SqlDbType.BigInt, freeSpace)
			};

			ExecuteNonQuery("dbo.UpdateRepositoryFreeSpace4Node", pars);
		}
    }
}
