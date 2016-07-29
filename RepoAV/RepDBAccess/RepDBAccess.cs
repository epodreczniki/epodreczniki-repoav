using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;
using System.Data;
using System.Data.SqlClient;

namespace PSNC.RepoAV.RepDBAccess
{
	public partial class RepDBAccess : BaseDBAccess
    {

		public RepDBAccess(string connectionString,	bool isCSEncrypted)
			: base(connectionString, 
					isCSEncrypted, 
					new byte[16] { 21, 31, 51, 124, 65, 7, 82, 124, 231, 12, 4, 6, 94, 70, 10, 33 }, 
					new byte[8] { 32, 211, 48, 51, 90, 34, 74, 129 })
		{
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

		//public bool RemoveGlobalData(string key)
		//{
		//	string data = string.Empty;
		//	try
		//	{
		//		using (SqlHelper sh = new SqlHelper(m_ConnectionString))
		//		{
		//			sh.AddParameterWithValue("@Key", key);

		//			sh.ExecuteNonQuery("dbo.RemoveGlobalData");
		//		}
		//		m_IsSqlServerOK = true;
		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		OnErrorReport(ex, string.Format("Błąd SQL podczas usuwania wartości globalnej dla klucza {0}.", key));
		//	}
		//	return false;
		//}


		//public DecimalTRes[] GetDecimalTest(DecimalT res)
		//{
		//	try
		//	{
		//		Dictionary<string, SqlParameter> pars = res.CreateSqlParameters();

		//		List<DecimalTRes> lst = ExecuteReaderToList<DecimalTRes>("dbo.GetDecimal", pars.Values.ToArray());


		//		res.DecimalOut = (decimal)pars["DecimalOut"].Value;
		//	}
		//	catch (Exception ex)
		//	{
		//		OnErrorReport(ex, "Błąd SQL podczas pobierania wartości globalnej dla klucza.");
		//	}
		//	return null;
		//}
    }
}
