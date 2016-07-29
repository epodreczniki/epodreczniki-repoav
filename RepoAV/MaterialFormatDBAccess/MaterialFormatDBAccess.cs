using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using PSNC.RepoAV.Common;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.MaterialFormatDBAccess
{
    public class MaterialFormatDBAccess : BaseDBAccess
    {
		public MaterialFormatDBAccess(string connectionString)
			: base(connectionString, false)
		{
		}

		public bool AddResponse2Send(Response2Send t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu Response2Send do metody AddResponse2Send.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"TaskId",
																			"ErrorCode",
																			"Result",
																			"TaskFinishDate");

			foreach (var de in pars)
				de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.AddResponse2Send", ps, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ErrorType.General, string.Format("Niespodziewany błąd DB podczas dodawania odpowiedzi. Sygnatura='{0}'.", t.GetSignature()));
			return false;
		}

		public bool RemoveResponse2Send(long id_task)
		{
			if (id_task < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody RemoveResponse2Send.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] { CreateSqlParameter("Id_Task", SqlDbType.BigInt, id_task) };

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveResponse2Send", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje odpowiedź dla zadania o Id={0}.", id_task));

			return false;
		}




		public bool AddFormat(FormatMetadata t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu do metody AddFormat.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"UniqueId",
																			"Location",
																			"Status",
																			"Size",
																			"RealSize",
																			"Mime",
																			"AllowDistribution");

			foreach (var de in pars)
				de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.AddFormat", ps, out ret);

			if (ret == ErrorType.Success)
			{
				return true;
			}
			else if (ret == ErrorType.AlreadyExists)
				OnErrorReport(ret, string.Format("Istnieje już format o UniqueId={0} lub Id={1}; dodanie nowego formatu niemożliwe.", t.UniqueId, t.Id));
			else
				OnErrorReport(ErrorType.General, string.Format("Niespodziewany błąd DB podczas dodawania formatu. Sygnatura='{0}'.", t.GetSignature()));
			return false;
		}

		public bool RemoveFormat(int id_Format)
		{
			if (id_Format < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody RemoveFormat.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id", SqlDbType.Int, id_Format),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, DBNull.Value)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveFormat", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o Id={0} - usunięcie niemożliwe.", id_Format));

			return false;
		}

		public bool RemoveFormat(string uniqueId)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody RemoveFormat.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id", SqlDbType.Int, DBNull.Value),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveFormat", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o PId={0} - usunięcie niemożliwe.", uniqueId));

			return false;
		}

		public FormatMetadata GetFormat(int id_Format)
		{
			if (id_Format < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody GetFormat.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<FormatMetadata>("Id",
																				"UniqueId",
																				"Location",
																				"Status",
																				"Size",
																				"Mime",
																				"AllowDistribution",
																				"CreatedDate",
																				"RealSize");

			foreach (var de in pars)
				if (de.Key == "Id" || de.Key == "UniqueId")
					de.Value.Direction = ParameterDirection.InputOutput;
				else
					de.Value.Direction = ParameterDirection.Output;
			pars["Id"].Value = id_Format;
			pars["UniqueId"].Value = DBNull.Value;

			SqlParameter[] ps = pars.Values.ToArray();

			ErrorType ret;
			ExecuteNonQuery("dbo.GetFormat", ps, out ret);

			if (ret == ErrorType.Success)
			{
				FormatMetadata t = CreateObject<FormatMetadata>(ps);
				return t;
			}
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje format o Id={0}.", id_Format));

			return null;
		}

		public FormatMetadata GetFormat(string uniqueId)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody GetFormat.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<FormatMetadata>("Id",
																				"UniqueId",
																				"Location",
																				"Status",
																				"Size",
																				"Mime",
																				"AllowDistribution",
																				"CreatedDate",
																				"RealSize");

			foreach (var de in pars)
				if (de.Key == "Id" || de.Key == "UniqueId")
					de.Value.Direction = ParameterDirection.InputOutput;
				else
					de.Value.Direction = ParameterDirection.Output;
			pars["Id"].Value = DBNull.Value;
			pars["UniqueId"].Value = uniqueId;

			SqlParameter[] ps = pars.Values.ToArray();

			ErrorType ret;
			ExecuteNonQuery("dbo.GetFormat", ps, out ret);

			if (pars["Id"].Value != DBNull.Value)
			{
				FormatMetadata t = CreateObject<FormatMetadata>(ps);
				return t;
			}
			return null;
		}

		public bool SetFormatStatus(int id_Format, FormatStatus status)
		{
			if (id_Format < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatStatus.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Status", SqlDbType.SmallInt, (short)status),
				CreateSqlParameter("Id", SqlDbType.Int, id_Format),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, DBNull.Value)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetFormatStatus", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o Id={0}.", id_Format));

			return false;
		}

		public bool SetFormatStatus(string uniqueId, FormatStatus status)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatStatus.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Status", SqlDbType.SmallInt, (short)status),
				CreateSqlParameter("Id", SqlDbType.Int, DBNull.Value),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetFormatStatus", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o UId={0}.", uniqueId));

			return false;
		}

		public bool SetFormatAllowDistribution(int id_Format, bool allowDistribution)
		{
			if (id_Format < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatAllowDistribution.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("AllowDistribution", SqlDbType.Bit, allowDistribution),
				CreateSqlParameter("Id", SqlDbType.Int, id_Format),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, DBNull.Value)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetFormatAllowDistribution", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o Id={0}.", id_Format));

			return false;
		}

		public bool SetFormatAllowDistribution(string uniqueId, bool allowDistribution)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatAllowDistribution.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("AllowDistribution", SqlDbType.Bit, allowDistribution),
				CreateSqlParameter("Id", SqlDbType.Int, DBNull.Value),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetFormatAllowDistribution", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o UniqueId={0}.", uniqueId));

			return false;
		}

		//public bool SetFormatValidTo(int id_Format, DateTime ValidTo)
		//{
		//	if (id_Format < 0)
		//	{
		//		OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatValidTo.");
		//		return false;
		//	}

		//	SqlParameter[] pars = new SqlParameter[] 
		//	{ 
		//		CreateSqlParameter("ValidTo", SqlDbType.DateTime, ValidTo),
		//		CreateSqlParameter("Id", SqlDbType.Int, id_Format),
		//		CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, DBNull.Value)
		//	};

		//	ErrorType ret;
		//	ExecuteNonQuery("dbo.SetFormatValidTo", pars, out ret);

		//	if (ret == ErrorType.Success)
		//		return true;
		//	else
		//		OnErrorReport(ret, string.Format("Nie istnieje format o Id={0}.", id_Format));

		//	return false;
		//}

		//public bool SetFormatValidTo(string uniqueId, DateTime ValidTo)
		//{
		//	if (string.IsNullOrEmpty(uniqueId))
		//	{
		//		OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatValidTo.");
		//		return false;
		//	}

		//	SqlParameter[] pars = new SqlParameter[] 
		//	{ 
		//		CreateSqlParameter("ValidTo", SqlDbType.DateTime, ValidTo),
		//		CreateSqlParameter("Id", SqlDbType.Int, DBNull.Value),
		//		CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
		//	};

		//	ErrorType ret;
		//	ExecuteNonQuery("dbo.SetFormatValidTo", pars, out ret);

		//	if (ret == ErrorType.Success)
		//		return true;
		//	else
		//		OnErrorReport(ret, string.Format("Nie istnieje format o UniqueId={0}.", uniqueId));

		//	return false;
		//}

		public bool SetFormatSize(string uniqueId, long size, long realSize)
		{
			if (string.IsNullOrEmpty(uniqueId) || size < 0 || realSize < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatSize.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Size", SqlDbType.BigInt, size),
				CreateSqlParameter("RealSize", SqlDbType.BigInt, realSize),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetFormatSize", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			//else
			//	OnErrorReport(ret, string.Format("Nie istnieje format o UniqueId={0}.", uniqueId));

			return false;
		}

		public bool UpdateFormat(string uniqueId, long size, long realSize, FormatStatus status, string mime, string location)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody UpdateFormat.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId),
				CreateSqlParameter("Size", SqlDbType.BigInt, size),
				CreateSqlParameter("RealSize", SqlDbType.BigInt, realSize),
				CreateSqlParameter("Status", SqlDbType.SmallInt, (short)status),
				CreateSqlParameter("Mime", SqlDbType.VarChar, 50, DBNull.Value),
				CreateSqlParameter("Location", SqlDbType.VarChar, 200, DBNull.Value)
				
			};

			if (!string.IsNullOrEmpty(mime))
				pars[4].Value = mime;

			if (!string.IsNullOrEmpty(location))
				pars[5].Value = location;

			ErrorType ret;
			ExecuteNonQuery("dbo.UpdateFormat", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o UId={0}.", uniqueId));

			return false;
		}


		public FormatMetadata[] GetAllFormats()
		{
			List<FormatMetadata> lst = ExecuteReaderToList<FormatMetadata>("dbo.GetAllFormats", null);
			return lst.ToArray();
		}

		public long GetTotalLoad() // w Bajtach
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("TotalLoad", SqlDbType.BigInt, ParameterDirection.Output)
			};

			ExecuteNonQuery("dbo.GetTotalLoad", pars);

			return (long)pars[0].Value;
		}

		public bool IsFileUsedByAnyMaterial(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawne parametry do metody IsFileUsedByAnyMaterial.");
				return false;
			}			

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("IsInUse", SqlDbType.Bit, ParameterDirection.Output),
				CreateSqlParameter("FilePath", SqlDbType.VarChar, 250, filePath)
			};

			ExecuteNonQuery("dbo.IsFileUsedByAnyMaterial", pars);

			if (pars[0].Value != DBNull.Value)
				return (bool)pars[0].Value;

			return true;
		}

		public Format2Remove[] GetFormatsPossibleToRemove(short olderThenDays)
		{
			SqlParameter[] pars = new SqlParameter[]
			{
				CreateSqlParameter("OlderThenDays", SqlDbType.SmallInt, olderThenDays)
			};
			List<Format2Remove> lst = ExecuteReaderToList<Format2Remove>("dbo.GetFormatsPossibleToRemove", pars);
			return lst.ToArray();
		}



		public string GetGlobalData(string key)
		{
			try
			{
				using (SqlHelper sh = new SqlHelper(m_ConnectionString))
				{
					sh.CommandTimeout = m_ExecutionTimeout;
					sh.AddParameterWithValue("@Key", key);
					sh.AddOutputParameter("@Value", SqlDbType.NVarChar, 500);
					sh.AddOutputParameter("@Description", SqlDbType.NVarChar, 500);

					sh.ExecuteNonQuery("[dbo].GetGlobalData");

					m_IsSqlServerOK = true;

					if (sh.Parameters["@Value"].Value == DBNull.Value)
						return null;
					else
						return (string)sh.Parameters["@Value"].Value;
				}
			}
			catch (Exception ex)
			{
				OnErrorReport(ex, string.Format("Błąd SQL podczas pobierania wartości globalnej dla klucza {0}.", key));
			}
			return null;
		}

		public string GetGlobalData(string key, out string description)
		{
			description = null;
			try
			{
				using (SqlHelper sh = new SqlHelper(m_ConnectionString))
				{
					sh.CommandTimeout = m_ExecutionTimeout;
					sh.AddParameterWithValue("@Key", key);
					sh.AddOutputParameter("@Value", SqlDbType.NVarChar, 500);
					sh.AddOutputParameter("@Description", SqlDbType.NVarChar, 500);

					sh.ExecuteNonQuery("[dbo].GetGlobalData");

					m_IsSqlServerOK = true;

					if (sh.Parameters["@Description"].Value == DBNull.Value)
						description = sh.Parameters["@Description"].Value.ToString();

					if (sh.Parameters["@Value"].Value == DBNull.Value)
						return null;
					else
						return (string)sh.Parameters["@Value"].Value;
				}
			}
			catch (Exception ex)
			{
				OnErrorReport(ex, string.Format("Błąd SQL podczas pobierania wartości globalnej dla klucza {0}.", key));
			}
			return null;
		}

		public bool SetGlobalData(string key, string value, string description = null)
		{
			try
			{
				using (SqlHelper sh = new SqlHelper(m_ConnectionString))
				{
					sh.CommandTimeout = m_ExecutionTimeout;
					sh.AddParameterWithValue("@Key", key);
					if (value != null)
						sh.AddParameterWithValue("@Value", value);
					else
					{
						sh.AddParameter("@Value", SqlDbType.NVarChar, 50);
						sh.Parameters["@Value"].Value = DBNull.Value;
					}
					if (description != null)
						sh.AddParameterWithValue("@Description", description);
					else
					{
						sh.AddParameter("@Description", SqlDbType.NVarChar, 500);
						sh.Parameters["@Description"].Value = DBNull.Value;
					}

					sh.ExecuteNonQuery("[dbo].SetGlobalData");
					m_IsSqlServerOK = true;

					return true;
				}
			}
			catch (Exception ex)
			{
				OnErrorReport(ex, string.Format("Błąd SQL podczas ustawiania wartości globalnej dla klucza {0}.", key));
			}
			return false;
		}



		public FormatAccess GetFormatAccess(string uniqueId)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano identyfikatora formatu do metody GetFormatAccess.");
				return null;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = CreateSqlParameters< FormatAccess>();
			pars["UniqueId"].Value = uniqueId;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.GetFormatAccess", ps, out ret);

			if (ret == ErrorType.Success)
			{
				FormatAccess fa = CreateObject<FormatAccess>(ps);
				return fa;
			}
			return null;
		}

		public string GetRepositoryPath(out int sizeInGB, out long totalLoadInMB, out byte repositoryMinFreeSpace)
		{
			sizeInGB = -1;
			totalLoadInMB = -1;
			repositoryMinFreeSpace = 0;

			SqlParameter[] pars = new SqlParameter[]
			{
				CreateSqlParameter("Size", SqlDbType.Int, ParameterDirection.Output),
				CreateSqlParameter("Path", SqlDbType.VarChar, 200, null, ParameterDirection.Output),
				CreateSqlParameter("TotalLoad", SqlDbType.BigInt, ParameterDirection.Output),
				CreateSqlParameter("RepositoryMinFreeSpace", SqlDbType.TinyInt, ParameterDirection.Output)				 
			};

			ExecuteNonQuery("dbo.GetRepositoryData", pars);

			if (pars[0].Value == DBNull.Value || pars[1].Value == DBNull.Value)
				OnErrorReport(ErrorType.WrongConfiguration, "Brak wpisów określających ścieżkę i wielkość repozytorium plików w DB.");
			else
			{
				sizeInGB = (int)pars[0].Value;
				totalLoadInMB = (long)pars[2].Value;
				repositoryMinFreeSpace = (byte)pars[3].Value;
				return (string)pars[1].Value;
			}
			return null;
		}

    }
}
