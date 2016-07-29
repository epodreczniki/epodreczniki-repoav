using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.DBAccess
{
	public class SqlHelper : IDisposable
	{
		protected SqlConnection m_Conn;
		protected SqlCommand m_Comm;
		protected SqlDataReader m_Reader;
		protected SqlTransaction m_Tran;
		protected int m_CommandTimeout;

		public int CommandTimeout
		{
			set
			{
				m_CommandTimeout = value;
			}
			get
			{
				return m_CommandTimeout;
			}
		}

		public SqlParameterCollection Parameters
		{
			get { return m_Comm.Parameters; }
		}

		public SqlDataReader Reader
		{
			get { return m_Reader; }
		}

		public SqlTransaction Transaction
		{
			get { return m_Tran; }
			set
			{
				if (m_Tran != null)
					m_Tran.Rollback();
				m_Tran = value;
				m_Comm.Transaction = m_Tran;
			}
		}


		public SqlHelper(string ConnectionString)
		{
			m_CommandTimeout = -1;
			m_Conn = new SqlConnection(ConnectionString);
			m_Comm = new SqlCommand();
			m_Comm.CommandType = CommandType.StoredProcedure;
			m_Comm.Connection = m_Conn;
			m_Conn.Open();
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (m_Comm != null && m_Comm.Parameters != null)
				m_Comm.Parameters.Clear();

			if (m_Reader != null && !m_Reader.IsClosed)
			{
				m_Reader.Close();
			}
			m_Reader = null;

			if (m_Tran != null)
			{
				m_Tran.Rollback();
			}
			m_Tran = null;

			if (m_Conn != null && (m_Conn.State != ConnectionState.Closed || m_Conn.State != ConnectionState.Broken))
			{
				m_Conn.Close();
			}
			m_Conn = null;
		}

		#endregion

		public void SetCommandTimeout(int to)//w sekundach
		{
			if (m_Comm != null)
				m_Comm.CommandTimeout = to;
			m_CommandTimeout = to;
		}

		public void BeginTransaction()
		{

			m_Tran = m_Conn.BeginTransaction();
			m_Comm.Transaction = m_Tran;
		}

		public void Commit()
		{
			m_Tran.Commit();
			m_Tran = null;
		}

		public void Rollback()
		{
			m_Tran.Rollback();
			m_Tran = null;
		}

		public int ExecuteNonQuery(string storedProcedureName)
		{
			if (storedProcedureName == null || storedProcedureName.Length == 0)
				throw new ArgumentException("Nazwa procedury składowanej nie może być pusta", "storedProcedureName");

			m_Comm.CommandText = storedProcedureName;
			if (m_CommandTimeout > -1)
				m_Comm.CommandTimeout = m_CommandTimeout;
			else
				m_Comm.CommandTimeout = 30;
			return m_Comm.ExecuteNonQuery();
		}

		public object ExecuteReader(string storedProcedureName)
		{
			if (storedProcedureName == null || storedProcedureName.Length == 0)
				throw new ArgumentException("Nazwa procedury sładowanej nie może być pusta", "storedProcedureName");

			m_Comm.CommandText = storedProcedureName;
			if (m_CommandTimeout > -1)
				m_Comm.CommandTimeout = m_CommandTimeout;
			else//default
				m_Comm.CommandTimeout = 30;

			m_Reader = m_Comm.ExecuteReader();

			return m_Reader;
		}

		public void AddOutputParameter(string name, SqlDbType type)
		{
			m_Comm.Parameters.Add(name, type).Direction = ParameterDirection.Output;
		}

		public void AddOutputParameter(string name, SqlDbType type, int size)
		{
			m_Comm.Parameters.Add(name, type, size).Direction = ParameterDirection.Output;
		}

		public SqlParameter AddParameter(string name, SqlDbType type)
		{
			return m_Comm.Parameters.Add(name, type);
		}

		public SqlParameter AddParameter(string name, SqlDbType type, int size)
		{
			return m_Comm.Parameters.Add(name, type, size);
		}

		public SqlParameter AddParameterWithValue(string name, object val)
		{
			return m_Comm.Parameters.AddWithValue(name, val);
		}

		public void AddParametersFromDictionary(Dictionary<string, object> dictionary)
		{
			foreach (KeyValuePair<string, object> pSource in dictionary)
			{
				if (pSource.Value == null)
					AddParameterWithValue("@" + pSource.Key, DBNull.Value);
				else
					AddParameterWithValue("@" + pSource.Key, pSource.Value);
			}
		}

		public Dictionary<string, object> GetDictionaryFromCurrentReaderRow()
		{
			Dictionary<string, object> resp = new Dictionary<string, object>();

			if (m_Reader == null || m_Reader.IsClosed || !m_Reader.HasRows)
				return resp;

			for (int i = 0; i < m_Reader.FieldCount; i++)
			{
				if (m_Reader[i] != DBNull.Value)
					resp.Add(m_Reader.GetName(i), m_Reader[i]);
				else
					resp.Add(m_Reader.GetName(i), null);
			}
			return resp;
		}

		public void AddReturnParameter()
		{
			AddParameter("RETURN_VALUE", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
		}

		public void AddRangeParameters(int offset, int count)
		{
			SqlParameter p = AddParameter("@Offset", SqlDbType.Int);
			p.Direction = ParameterDirection.Input;
			p.Value = offset;

			p = AddParameter("@Count", SqlDbType.Int);
			p.Direction = ParameterDirection.Input;
			p.Value = count;

			p = AddParameter("@Total", SqlDbType.Int);
			p.Direction = ParameterDirection.Output;
		}

		public ErrorType GetReturnValue()
		{
			int intVal = (int)Parameters["RETURN_VALUE"].Value;

			if (Enum.IsDefined(typeof(ErrorType), intVal))
				return ((ErrorType)intVal);

			return ErrorType.Runtime;
		}

		public int GetTotalCount4Range()
		{
			return (int)Parameters["@Total"].Value;
		}

		public bool CanReachSqlServer()
		{
			try
			{
				m_Conn.Open();
				m_Conn.Close();
				return true;
			}
			catch
			{
			}
			return false;
		}

		public static Type GetClrType4SqlType(SqlDbType sqlType)
		{
			switch (sqlType)
			{
				case SqlDbType.BigInt:
					return typeof(Int64);
				case SqlDbType.Binary:
					return typeof(Byte[]);
				case SqlDbType.Bit:
					return typeof(Boolean);
				case SqlDbType.Char:
					return typeof(String);
				case SqlDbType.Date:
					return typeof(DateTime);
				case SqlDbType.DateTime:
					return typeof(DateTime);
				case SqlDbType.DateTime2:
					return typeof(DateTime);
				case SqlDbType.DateTimeOffset:
					return typeof(DateTimeOffset);
				case SqlDbType.Decimal:
					return typeof(Decimal);
				case SqlDbType.Float:
					return typeof(Double);
				case SqlDbType.Int:
					return typeof(Int32);
				case SqlDbType.Money:
					return typeof(Decimal);
				case SqlDbType.NChar:
					return typeof(String);
				case SqlDbType.NText:
					return typeof(String);
				case SqlDbType.NVarChar:
					return typeof(String);
				case SqlDbType.Real:
					return typeof(Single);
				case SqlDbType.SmallInt:
					return typeof(Int16);
				case SqlDbType.SmallMoney:
					return typeof(Decimal);
				case SqlDbType.Structured:
					return typeof(Object);
				case SqlDbType.Text:
					return typeof(String);
				case SqlDbType.Time:
					return typeof(TimeSpan);
				case SqlDbType.Timestamp:
					return typeof(Byte[]);
				case SqlDbType.TinyInt:
					return typeof(Byte);
				case SqlDbType.Udt:
					return typeof(Object);
				case SqlDbType.UniqueIdentifier:
					return typeof(Guid);
				case SqlDbType.VarBinary:
					return typeof(Byte[]);
				case SqlDbType.VarChar:
					return typeof(String);
				case SqlDbType.Variant:
					return typeof(Object);
				case SqlDbType.Xml:
					return typeof(SqlXml);
				default:
					return null;
			}
		}

		public static SqlDbType? GetSqlType4ClrType(Type type)
		{
			if (type == typeof(Int64))
				return SqlDbType.BigInt;
			else if (type == typeof(Byte[]))
				return SqlDbType.VarBinary;
			else if (type == typeof(Boolean))
				return SqlDbType.Bit;
			else if (type == typeof(String))
				return SqlDbType.NVarChar;
			else if (type == typeof(DateTime))
				return SqlDbType.DateTime;
			else if (type == typeof(DateTimeOffset))
				return SqlDbType.DateTimeOffset;
			else if (type == typeof(Decimal))
				return SqlDbType.Decimal;
			else if (type == typeof(Double))
				return SqlDbType.Float;
			else if (type == typeof(Int32))
				return SqlDbType.Int;
			else if (type == typeof(Single))
				return SqlDbType.Real;
			else if (type == typeof(Int16))
				return SqlDbType.SmallInt;
			else if (type == typeof(TimeSpan))
				return SqlDbType.Time;
			else if (type == typeof(Byte))
				return SqlDbType.TinyInt;
			else if (type == typeof(Guid))
				return SqlDbType.UniqueIdentifier;
			else if (type == typeof(SqlXml))
				return SqlDbType.Xml;
			else if (type == typeof(Int64?))
				return SqlDbType.BigInt;
			else if (type == typeof(Boolean?))
				return SqlDbType.Bit;
			else if (type == typeof(DateTime?))
				return SqlDbType.DateTime;
			else if (type == typeof(DateTimeOffset?))
				return SqlDbType.DateTimeOffset;
			else if (type == typeof(Decimal?))
				return SqlDbType.Decimal;
			else if (type == typeof(Double?))
				return SqlDbType.Float;
			else if (type == typeof(Int32?))
				return SqlDbType.Int;
			else if (type == typeof(Single?))
				return SqlDbType.Real;
			else if (type == typeof(Int16?))
				return SqlDbType.SmallInt;
			else if (type == typeof(TimeSpan?))
				return SqlDbType.Time;
			else if (type == typeof(Byte?))
				return SqlDbType.TinyInt;
			else if (type.IsEnum)
				return GetSqlType4ClrType(Enum.GetUnderlyingType(type));
			else if (type.IsArray)
				return SqlDbType.VarChar;

			return null;
		}

	}
}
