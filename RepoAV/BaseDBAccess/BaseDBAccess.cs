using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Security;
using System.IO;
using System.Reflection;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.DBAccess
{
    public abstract class BaseDBAccess
    {
		byte[] m_RC2_Key = new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
		byte[] m_RC2_IV = new byte[8] { 2, 2, 2, 2, 2, 2, 2, 2 };

		protected string m_ConnectionString;
		protected int m_ExecutionTimeout;
		protected bool m_IsSqlServerOK;


		public static BaseDBAccess StaticWorker = null;

		public int ExecutionTimeout
		{
			set
			{
				m_ExecutionTimeout = value;
			}
			get
			{
				return m_ExecutionTimeout;
			}
		}
		public string ConnectionString
		{
			set { m_ConnectionString = value; }
			get { return m_ConnectionString; }
		}
		public bool IsSqlServerOk
		{
			get { return m_IsSqlServerOK; }
		}
		public string ServerName
		{
			get
			{
				if (!string.IsNullOrEmpty(m_ConnectionString))
				{
					string tmp = m_ConnectionString.Replace(" ", "").ToLower();
					int idx = tmp.IndexOf("datasource=");
					if (idx > -1)
					{
						tmp = tmp.Substring(idx + "datasource=".Length);
						int endIdx = tmp.IndexOf(";");
						if (endIdx > -1)
							return tmp.Substring(0, endIdx);
					}
				}
				return null;
			}
		}


		public BaseDBAccess(string connectionString,
							bool isCSEncrypted,
							byte[] RC2_Key = null,
							byte[] RC2_IV = null)
		{
			m_ExecutionTimeout = -1;
			if (isCSEncrypted)
			{
				if (RC2_Key != null)
					m_RC2_Key = RC2_Key;
				if (RC2_IV != null)
					m_RC2_IV = RC2_IV;
				m_ConnectionString = DecryptConnectionString(connectionString);
			}
			else
				m_ConnectionString = connectionString;

			m_IsSqlServerOK = true;
		}


		protected void OnErrorReport(string message)
		{
			OnErrorReport(null, 0, message);
		}

		protected void OnErrorReport(ErrorType errorCode, string message)
		{
			OnErrorReport(null, errorCode, message);
		}

		protected void OnErrorReport(Exception ex, string message)
		{
			OnErrorReport(ex, 0, message);
		}

		protected void OnErrorReport(Exception ex, ErrorType errorCode, string message)
		{
			if (ex != null)
			{
				if (ex is SqlException)
					m_IsSqlServerOK = false;
			}

			DBAccessException dbe = null;
			if (ex != null)
				dbe = new DBAccessException(ex, errorCode, message);
			else
				dbe = new DBAccessException(errorCode, message);
			throw dbe;

		}

		string BuildErrorDesc4Procedure(string storedProcedureName, SqlParameter[] pars)
		{
			StringBuilder sb = new StringBuilder();


			sb.AppendLine(string.Format("Błąd podczas wykonania procedury: {0}.", storedProcedureName ?? "NULL"));
			if (pars != null)
			{
				string value = "";
				foreach (SqlParameter sp in pars)
				{
					if (sp.Value == DBNull.Value || sp.Value == null)
						value = "NULL";
					else
					{
						if (sp.SqlDbType == SqlDbType.Binary || sp.SqlDbType == SqlDbType.Image || sp.SqlDbType == SqlDbType.VarBinary)
							value = "{Binary}";
						else if (sp.SqlDbType == SqlDbType.Structured)
							value = "{Structured}";
						else if (sp.SqlDbType == SqlDbType.Udt)
							value = "{UserType}";
						else if (sp.SqlDbType == SqlDbType.Xml)
							value = "{Xml}";
						else if (sp.SqlDbType == SqlDbType.VarChar || sp.SqlDbType == SqlDbType.Text || sp.SqlDbType == SqlDbType.NVarChar || sp.SqlDbType == SqlDbType.NText || sp.SqlDbType == SqlDbType.NChar || sp.SqlDbType == SqlDbType.Char)
						{
							value = sp.Value.ToString();
							if (value.Length > 30)
								value = value.Substring(0, 30);
						}
						else
							value = sp.Value.ToString();
					}

					sb.AppendLine(string.Format("  {0} = {1}", sp.ParameterName, value));
				}
			}

			return sb.ToString();
		}

		#region Encryption
		public void SetRC2Key(byte[] key)
		{
			m_RC2_Key = key;
		}

		public void SetRC2IV(byte[] iv)
		{
			m_RC2_IV = iv;
		}

		public string EncryptConnectionString(string input)
		{
			if (string.IsNullOrEmpty(input))
				return null;

			RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider();

			// Get an encryptor.
			ICryptoTransform encryptor = rc2CSP.CreateEncryptor(m_RC2_Key, m_RC2_IV);

			// Encrypt the data as an array of encrypted bytes in memory.
			MemoryStream msEncrypt = new MemoryStream();
			CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

			// Convert the data to a byte array.
			byte[] toEncrypt = Encoding.ASCII.GetBytes(input);

			// Write all data to the crypto stream and flush it.
			csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
			csEncrypt.FlushFinalBlock();

			// Get the encrypted array of bytes.
			byte[] encrypted = msEncrypt.ToArray();

			return Convert.ToBase64String(encrypted);
			//return Encoding.UTF8.GetString(encrypted);
		}

		public string DecryptConnectionString(string input)
		{
			if (string.IsNullOrEmpty(input))
				return null;

			RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider();

			//Get a decryptor that uses the same key and IV as the encryptor.
			ICryptoTransform decryptor = rc2CSP.CreateDecryptor(m_RC2_Key, m_RC2_IV);

			//byte[] encrypted = Encoding.UTF8.GetBytes(input);

			byte[] encrypted = Convert.FromBase64String(input);

			// Now decrypt the previously encrypted message using the decryptor
			// obtained in the above step.
			MemoryStream msDecrypt = new MemoryStream(encrypted);
			CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

			// Read the decrypted bytes from the decrypting stream
			// and place them in a StringBuilder class.

			byte[] tmp = new byte[1024];
			List<byte> arB = new List<byte>();
			int readCount = 1024;
			while (readCount == 1024)
			{
				readCount = csDecrypt.Read(tmp, 0, 1024);
				if (readCount == 1024)
					arB.AddRange(tmp);
				else
					arB.AddRange(tmp.Take(readCount));
			}
			return Encoding.UTF8.GetString(arB.ToArray());
			//StringBuilder roundtrip = new StringBuilder();
			//int b = 0;
			//do
			//{
			//	b = csDecrypt.ReadByte();

			//	if (b != -1)
			//	{
			//		roundtrip.Append(Encoding.UTF8.GetChars();
			//	}

			//} while (b != -1);
			//return roundtrip.ToString();
		}
		#endregion Encryption

		public static T CreateObject<T>(SqlParameterCollection pars)
		{
			if (pars == null)
				return default(T);

			SqlParameter[] arr = new SqlParameter[pars.Count];
			pars.CopyTo(arr, 0);
			return CreateObject<T>(arr);
		}

		public static T CreateObject<T>(SqlParameter[] pars)
		{
			T obj = default(T);

			if (pars != null)
			{				
				if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
				{
					obj = (T)pars[0].Value;
				}
				else
				{
					obj = Activator.CreateInstance<T>();
					foreach (SqlParameter sp in pars)
					{
						if (sp != null && !string.IsNullOrEmpty(sp.ParameterName))
						{
							System.Reflection.PropertyInfo pi = obj.GetType().GetProperty(sp.ParameterName.TrimStart('@'), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
							if (pi != null)
							{
								if (sp.Value == null || sp.Value == DBNull.Value)
								{
									if (pi.PropertyType.IsValueType && !pi.PropertyType.FullName.StartsWith("System.Nullable`1"))
									{
										if (pi.PropertyType == typeof(int))
											pi.SetValue(obj, (int)-1, null);
										else if (pi.PropertyType == typeof(long))
											pi.SetValue(obj, (long)-1, null);
										else if (pi.PropertyType == typeof(short))
											pi.SetValue(obj, (short)-1, null);
										else if (pi.PropertyType == typeof(double))
											pi.SetValue(obj, (double)-1, null);
										else if (pi.PropertyType == typeof(decimal))
											pi.SetValue(obj, (decimal)-1, null);
										else if (pi.PropertyType == typeof(float))
											pi.SetValue(obj, (float)-1, null);
										else if (pi.PropertyType == typeof(byte))
											pi.SetValue(obj, (byte)0, null);
										else if (pi.PropertyType == typeof(bool))
											pi.SetValue(obj, false, null);
										else if (pi.PropertyType == typeof(Guid))
											pi.SetValue(obj, Guid.Empty, null);
										else if (pi.PropertyType == typeof(DateTime))
											pi.SetValue(obj, DateTime.MinValue, null);
										else if (pi.PropertyType == typeof(Decimal))
											pi.SetValue(obj, Decimal.Zero, null);
										else if (pi.PropertyType.IsEnum)
										{
											Type typeIn = pi.PropertyType.GetEnumUnderlyingType();
											pi.SetValue(obj, Convert.ChangeType(0, typeIn), null);
										}
										else
											pi.SetValue(obj, 0, null);
									}
									else
										pi.SetValue(obj, null, null);
								}
								else
								{
									if (pi.PropertyType != typeof(byte[]) && (pi.PropertyType.IsArray || typeof(Array).IsAssignableFrom(pi.PropertyType)))
									{
										string strVal = sp.Value.ToString();
										if (!string.IsNullOrEmpty(strVal))
										{
											string separator = ";";
											SqlParameterAttribute atrInput = (SqlParameterAttribute)pi.GetCustomAttributes(true).FirstOrDefault(a => { return a is SqlParameterAttribute; });
											if (atrInput != null)
												separator = atrInput.ArrayElementsSeparator;

											string[] tmp = strVal.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
											if (tmp != null && tmp.Length > 0)
											{
												Type arrayElementType = pi.PropertyType.GetElementType();
												Array arr = Array.CreateInstance(arrayElementType, tmp.Length);
												for (int i = 0; i < tmp.Length; i++)
												{
													if (arrayElementType.IsEnum)
													{
														int ival = int.Parse(tmp[i]);
														arr.SetValue(Enum.ToObject(arrayElementType, ival), i);
													}
													else if (arrayElementType.IsPrimitive)
													{
														if (arrayElementType == typeof(Int16)
																|| arrayElementType == typeof(Int32)
																|| arrayElementType == typeof(Int64)
																|| arrayElementType == typeof(UInt16)
																|| arrayElementType == typeof(UInt32)
																|| arrayElementType == typeof(UInt64)
																|| arrayElementType == typeof(byte))
														{
															long lval = long.Parse(tmp[i]);
															arr.SetValue(Convert.ChangeType(lval, arrayElementType), i);
														}
														else if (arrayElementType == typeof(float)
																|| arrayElementType == typeof(double)
																|| arrayElementType == typeof(decimal))
														{
															decimal lval = decimal.Parse(tmp[i]);
															arr.SetValue(Convert.ChangeType(lval, arrayElementType), i);
														}

													}
													else
														arr.SetValue(tmp[i], i);
												}

												pi.SetValue(obj, arr, null);
											}
											else
												pi.SetValue(obj, null, null);
										}
										else
											pi.SetValue(obj, null, null);
									}
									else
									{
										if (pi.PropertyType == typeof(System.Data.SqlTypes.SqlXml))
										{
											byte[] bs = Encoding.UTF8.GetBytes(sp.Value.ToString());
											MemoryStream mem = new MemoryStream(bs);
											System.Data.SqlTypes.SqlXml sx = new System.Data.SqlTypes.SqlXml(mem);

											pi.SetValue(obj, sx, null);
										}
										else
											pi.SetValue(obj, sp.Value, null);
									}
								}
							}
						}
					}
				}
			}

			return obj;
		}

		public static T CreateObject<T>(Dictionary<string, object> row)
		{
			T obj = default(T);

			if (row != null)
			{
				if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
				{
					obj = (T)row.Values.ToArray()[0];
				}
				else
				{
					obj = Activator.CreateInstance<T>();
					foreach (var de in row)
					{
						if (!string.IsNullOrEmpty(de.Key))
						{
							System.Reflection.PropertyInfo pi = obj.GetType().GetProperty(de.Key, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
							if (pi != null)
							{
								if (de.Value == null || de.Value == DBNull.Value)
								{
									if (pi.PropertyType.IsValueType && !pi.PropertyType.FullName.StartsWith("System.Nullable`1"))
									{
										if (pi.PropertyType == typeof(int))
											pi.SetValue(obj, (int)-1, null);
										else if (pi.PropertyType == typeof(long))
											pi.SetValue(obj, (long)-1, null);
										else if (pi.PropertyType == typeof(short))
											pi.SetValue(obj, (short)-1, null);
										else if (pi.PropertyType == typeof(double))
											pi.SetValue(obj, (double)-1, null);
										else if (pi.PropertyType == typeof(decimal))
											pi.SetValue(obj, (decimal)-1, null);
										else if (pi.PropertyType == typeof(float))
											pi.SetValue(obj, (float)-1, null);
										else if (pi.PropertyType == typeof(byte))
											pi.SetValue(obj, (byte)0, null);
										else if (pi.PropertyType == typeof(bool))
											pi.SetValue(obj, false, null);
										else if (pi.PropertyType == typeof(Guid))
											pi.SetValue(obj, Guid.Empty, null);
										else if (pi.PropertyType == typeof(DateTime))
											pi.SetValue(obj, DateTime.MinValue, null);
										else
											pi.SetValue(obj, 0, null);
									}
									else
										pi.SetValue(obj, null, null);
								}
								else
								{
									if (pi.PropertyType.IsArray || typeof(Array).IsAssignableFrom(pi.PropertyType))
									{
										string strVal = de.Value.ToString();
										if (!string.IsNullOrEmpty(strVal))
										{
											string separator = ";";
											SqlParameterAttribute atrInput = (SqlParameterAttribute)pi.GetCustomAttributes(true).FirstOrDefault(a => { return a is SqlParameterAttribute; });
											if (atrInput != null)
												separator = atrInput.ArrayElementsSeparator;

											if (pi.PropertyType == typeof(byte[]) && (!atrInput.ProperSQLDBType.HasValue || atrInput.ProperSQLDBType.Value == SqlDbType.VarBinary || atrInput.ProperSQLDBType.Value == SqlDbType.Image || atrInput.ProperSQLDBType.Value == SqlDbType.Binary))
											{
												pi.SetValue(obj, de.Value, null);
												continue;
											}

											string[] tmp = strVal.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
											if (tmp != null && tmp.Length > 0)
											{
												Type arrayElementType = pi.PropertyType.GetElementType();
												Array arr = Array.CreateInstance(arrayElementType, tmp.Length);
												for (int i = 0; i < tmp.Length; i++)
												{
													if (arrayElementType.IsEnum)
													{
														int ival = int.Parse(tmp[i]);
														arr.SetValue(Enum.ToObject(arrayElementType, ival), i);
													}
													else if (arrayElementType.IsPrimitive)
													{
														if (arrayElementType == typeof(Int16)
																|| arrayElementType == typeof(Int32)
																|| arrayElementType == typeof(Int64)
																|| arrayElementType == typeof(UInt16)
																|| arrayElementType == typeof(UInt32)
																|| arrayElementType == typeof(UInt64)
																|| arrayElementType == typeof(byte))
														{
															long lval = long.Parse(tmp[i]);
															arr.SetValue(Convert.ChangeType(lval, arrayElementType), i);
														}
														else if (arrayElementType == typeof(float)
																|| arrayElementType == typeof(double)
																|| arrayElementType == typeof(decimal))
														{
															decimal lval = decimal.Parse(tmp[i]);
															arr.SetValue(Convert.ChangeType(lval, arrayElementType), i);
														}

													}
													else
														arr.SetValue(tmp[i], i);
												}

												pi.SetValue(obj, arr, null);
											}
											else
												pi.SetValue(obj, null, null);
										}
										else
											pi.SetValue(obj, null, null);
									}
									else
									{
										if (pi.PropertyType == typeof(System.Data.SqlTypes.SqlXml))
										{
											byte[] bs = Encoding.UTF8.GetBytes(de.Value.ToString());
											MemoryStream mem = new MemoryStream(bs);
											System.Data.SqlTypes.SqlXml sx = new System.Data.SqlTypes.SqlXml(mem);

											pi.SetValue(obj, sx, null);
										}
										else
											pi.SetValue(obj, de.Value, null);
									}
								}
							}
						}
					}
				}
			}

			return obj;
		}



		public static SqlParameter CreateSqlParameter(string name, SqlDbType sqlType, ParameterDirection dir = ParameterDirection.Input)
		{
			return CreateSqlParameter(name, sqlType, -1, null, dir);
		}

		public static SqlParameter CreateSqlParameter(string name, SqlDbType sqlType, object val, ParameterDirection dir = ParameterDirection.Input)
		{
			return CreateSqlParameter(name, sqlType, -1, val, dir);
		}

		public static SqlParameter CreateSqlParameter(string name, SqlDbType sqlType, int size, object val, ParameterDirection dir = ParameterDirection.Input)
		{
			SqlParameter p = new SqlParameter(name, sqlType, size);
			p.Direction = dir;
			if (val == null)
				p.Value = DBNull.Value;
			else
			{
				if (val is Guid && ((Guid)val) == Guid.Empty)
					p.Value = DBNull.Value;
				else if (val is DateTime && ((DateTime)val) == DateTime.MinValue)
					p.Value = DBNull.Value;
				else if (val is Enum)
				{
					Type typeIn = Enum.GetUnderlyingType(val.GetType());
					object obj = Convert.ChangeType(val, typeIn);

					p.Value = obj;
				}
				else
				{
					if ((val is int && (int)val == -1)
						|| (val is long && (long)val == -1)
						|| (val is short && (short)val == -1)
						|| (val is float && (float)val == -1)
						|| (val is double && (double)val == -1))
						p.Value = DBNull.Value;
					else
						p.Value = val;
				}
			}
			return p;
		}



		public static Dictionary<string, SqlParameter> CreateSqlParameters<T>()
		{
			return CreateSqlParameters(typeof(T), null, null);
		}

		public static Dictionary<string, SqlParameter> CreateSqlParameters<T>(params string[] onlyTheseParams)
		{
			return CreateSqlParameters(typeof(T), null, onlyTheseParams);
		}

		public static Dictionary<string, SqlParameter> CreateSqlParameters(Type t, object bo, string[] onlyTheseParams)
		{
			Dictionary<string, SqlParameter> pars = new Dictionary<string, SqlParameter>();
			PropertyInfo[] pis = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			foreach (var prop in pis)
			{
				if (onlyTheseParams == null || onlyTheseParams.Length == 0 || onlyTheseParams.Contains(prop.Name) || onlyTheseParams.Contains("@" + prop.Name))
				{
					SqlParameterAttribute atrInput = (SqlParameterAttribute)prop.GetCustomAttributes(true).FirstOrDefault(a => { return a is SqlParameterAttribute; });
					if (atrInput != null)
					{
						short maxLength = atrInput.MaxLength;

						SqlDbType dbtype;
						if (atrInput.ProperSQLDBType.HasValue)
							dbtype = atrInput.ProperSQLDBType.Value;
						else if (SqlHelper.GetSqlType4ClrType(prop.PropertyType).HasValue)
							dbtype = SqlHelper.GetSqlType4ClrType(prop.PropertyType).Value;
						else
							throw new NotImplementedException();

						if (prop.PropertyType.IsArray && maxLength < 5)
							maxLength = 150;

						SqlParameter p = null;
						if (maxLength > -2 && maxLength != 0)
							p = new SqlParameter(prop.Name, dbtype, maxLength);
						else
							p = new SqlParameter(prop.Name, dbtype);


						p.Direction = atrInput.Direction;

						if (bo != null)
						{
							object val = prop.GetValue(bo, null);
							if (val != null)
							{
								if (val is Guid && ((Guid)val) == Guid.Empty)
								{
									p.Value = DBNull.Value;
								}
								else if (val is DateTime && ((DateTime)val) == DateTime.MinValue)
								{
									p.Value = DBNull.Value;
								}
								else if (val is byte[] && (!atrInput.ProperSQLDBType.HasValue || atrInput.ProperSQLDBType.Value == SqlDbType.VarBinary || atrInput.ProperSQLDBType.Value == SqlDbType.Image || atrInput.ProperSQLDBType.Value == SqlDbType.Binary))
								{
									p.Value = val;
								}
								else if (val is System.Data.SqlTypes.SqlXml)
								{
									p.Value = ((System.Data.SqlTypes.SqlXml)val).Value;
								}
								else if (val.GetType().IsArray)
								{
									string separator = atrInput.ArrayElementsSeparator;
									if (string.IsNullOrEmpty(separator))
										separator = ";";

									if (((Array)val).Length > 0)
									{
										if (((Array)val).GetValue(0).GetType().IsEnum)
										{
											Type typeIn = Enum.GetUnderlyingType(((Array)val).GetValue(0).GetType());
											string[] arr = new string[((Array)val).Length];
											for (int i = 0; i < ((Array)val).Length; i++)
												arr.SetValue(Convert.ChangeType(((Array)val).GetValue(i), typeIn).ToString(), i);
											p.Value = string.Join(separator, arr);
										}
										else
										{
											string[] tmps = new string[((Array)val).Length];
											for (int i = 0; i < ((Array)val).Length; i++)
												tmps[i] = ((Array)val).GetValue(i).ToString();
											p.Value = string.Join(separator, tmps);

										}
									}
									else
										p.Value = "";
								}
								else
								{
									if (atrInput.TreatMinusOneAsNull
											&& val != null
											&& ((val is int && (int)val == -1)
												|| (val is long && (long)val == -1)
												|| (val is short && (short)val == -1)
												|| (val is float && (float)val == -1)
												|| (val is decimal && (decimal)val == -1)
												|| (val is double && (double)val == -1)))
										p.Value = DBNull.Value;
									else
									{
										if (val.GetType().IsEnum)
										{
											Type typeIn = Enum.GetUnderlyingType(val.GetType());
											object newVal = Convert.ChangeType(val, typeIn);
											if (atrInput.TreatMinusOneAsNull
												&& newVal != null
												&& ((newVal is int && (int)newVal == -1)
													|| (newVal is long && (long)newVal == -1)
													|| (newVal is short && (short)newVal == -1)
													|| (newVal is float && (float)newVal == -1)
													|| (newVal is decimal && (decimal)newVal == -1)
													|| (newVal is double && (double)newVal == -1)))
											{
												p.Value = DBNull.Value;
											}
											else
												p.Value = newVal;
										}
										else
											p.Value = val;
									}
								}
							}
							else
								p.Value = DBNull.Value;
						}
						else
							p.Value = DBNull.Value;

						pars[prop.Name] = p;
					}
				}
			}
			return pars;
		}




		protected List<T> ExecuteReaderToList<T>(string storedProcedureName, SqlParameter[] pars)
		{
			ErrorType returnValue;
			return ExecuteReaderToList<T>(storedProcedureName, pars, false, out returnValue, null);
		}

		protected List<T> ExecuteReaderToList<T>(string storedProcedureName, SqlParameter[] pars, RangeSelector range = null)
		{
			ErrorType returnValue;
			return ExecuteReaderToList<T>(storedProcedureName, pars, false, out returnValue, range);
		}

		protected List<T> ExecuteReaderToList<T>(string storedProcedureName, SqlParameter[] pars, out ErrorType et, RangeSelector range = null)
		{
			return ExecuteReaderToList<T>(storedProcedureName, pars, true, out et, range);
		}

		List<T> ExecuteReaderToList<T>(string storedProcedureName, SqlParameter[] pars, bool takeReturnValue, out ErrorType et, RangeSelector range)
		{
			et = ErrorType.Runtime;
			if (string.IsNullOrEmpty(storedProcedureName))
			{
				OnErrorReport(null, "Wołanie 'ExecuteReaderToList' z pustą nazwą procedury.");
				return null;
			}

			try
			{
				List<T> lst = new List<T>();
				using (SqlHelper sh = new SqlHelper(m_ConnectionString))
				{
					sh.CommandTimeout = m_ExecutionTimeout;

					if (pars != null && pars.Length > 0)
						sh.Parameters.AddRange(pars);

					if (takeReturnValue)
						sh.AddReturnParameter();

					if (range != null && range.Offset > -1 && range.Count > 0)
						sh.AddRangeParameters(range.Offset, range.Count);

					sh.ExecuteReader(storedProcedureName);
					if (sh.Reader != null)
					{
						while (sh.Reader.Read())
						{
							Dictionary<string, object> row = sh.GetDictionaryFromCurrentReaderRow();
							if (row != null)
							{
								T obj = CreateObject<T>(row);
								lst.Add(obj);
							}
						}

						sh.Reader.Close();
					}

					if (range != null && range.Offset > -1 && range.Count > 0)
						range.Total = sh.GetTotalCount4Range();

					m_IsSqlServerOK = true;

					if (takeReturnValue)
						et = sh.GetReturnValue();

					return lst;
				}
			}
			catch (Exception ex)
			{
				OnErrorReport(ex, BuildErrorDesc4Procedure(storedProcedureName, pars));
			}
			return null;
		}

		protected Dictionary<string, string> ExecuteReaderToDictionary(string storedProcedureName, SqlParameter[] pars, bool takeReturnValue, out ErrorType et, RangeSelector range)
		{
			et = ErrorType.Runtime;
			if (string.IsNullOrEmpty(storedProcedureName))
			{
				OnErrorReport(null, "Wołanie 'ExecuteReaderToDictionary' z pustą nazwą procedury.");
				return null;
			}

			try
			{
				Dictionary<string, string> res = new Dictionary<string, string>();
				using (SqlHelper sh = new SqlHelper(m_ConnectionString))
				{
					sh.CommandTimeout = m_ExecutionTimeout;

					if (pars != null && pars.Length > 0)
						sh.Parameters.AddRange(pars);

					if (takeReturnValue)
						sh.AddReturnParameter();

					if (range != null && range.Offset > -1 && range.Count > 0)
						sh.AddRangeParameters(range.Offset, range.Count);

					sh.ExecuteReader(storedProcedureName);
					if (sh.Reader != null)
					{
						while (sh.Reader.Read())
						{
							if (!sh.Reader.IsDBNull(0))
							{
								string key = sh.Reader.GetString(0);
								string val = null;
								if (!sh.Reader.IsDBNull(1))
									val = sh.Reader.GetString(1);
								res[key] = val;
							}
						}

						sh.Reader.Close();
					}

					if (range != null && range.Offset > -1 && range.Count > 0)
						range.Total = sh.GetTotalCount4Range();

					m_IsSqlServerOK = true;

					if (takeReturnValue)
						et = sh.GetReturnValue();

					return res;
				}
			}
			catch (Exception ex)
			{
				OnErrorReport(ex, BuildErrorDesc4Procedure(storedProcedureName, pars));
			}
			return null;
		}




		protected int ExecuteNonQuery(string storedProcedureName, SqlParameter[] pars)
		{
			ErrorType returnValue = 0;
			return ExecuteNonQuery(storedProcedureName, pars, false, out returnValue);
		}

		protected int ExecuteNonQuery(string storedProcedureName, SqlParameter[] pars, out ErrorType et)
		{
			return ExecuteNonQuery(storedProcedureName, pars, true, out et);
		}

		int ExecuteNonQuery(string storedProcedureName, SqlParameter[] pars, bool takeReturnValue, out ErrorType et)
		{
			et = ErrorType.Success;

			if (string.IsNullOrEmpty(storedProcedureName))
			{
				OnErrorReport(null, "Wołanie 'ExecuteNonQuery' z pustą nazwą procedury.");
				return -1;
			}

			try
			{
				using (SqlHelper sh = new SqlHelper(m_ConnectionString))
				{
					sh.CommandTimeout = m_ExecutionTimeout;
					if (pars != null && pars.Length > 0)
						sh.Parameters.AddRange(pars);

					if (takeReturnValue)
						sh.AddReturnParameter();

					int ret = sh.ExecuteNonQuery(storedProcedureName);
					m_IsSqlServerOK = true;

					if (takeReturnValue)
						et = sh.GetReturnValue();

					return ret;
				}
			}
			catch (Exception ex)
			{
				OnErrorReport(ex, BuildErrorDesc4Procedure(storedProcedureName, pars));
			}
			return -1;
		}



		protected int ExecuteNonQueryInTransaction(string storedProcedureName, SqlParameter[] pars, SqlHelper sh)
		{
			ErrorType returnValue;
			return ExecuteNonQueryInTransaction(storedProcedureName, pars, false, out returnValue, sh);
		}

		protected int ExecuteNonQueryInTransaction(string storedProcedureName, SqlParameter[] pars, out ErrorType et, SqlHelper sh)
		{
			return ExecuteNonQueryInTransaction(storedProcedureName, pars, true, out et, sh);
		}

		int ExecuteNonQueryInTransaction(string storedProcedureName, SqlParameter[] pars, bool takeReturnValue, out ErrorType et, SqlHelper sh)
		{
			et = ErrorType.Success;

			if (string.IsNullOrEmpty(storedProcedureName))
			{
				OnErrorReport(null, "Wołanie 'ExecuteNonQueryInTransaction' z pustą nazwą procedury.");
				return -1;
			}

			try
			{
				sh.CommandTimeout = m_ExecutionTimeout;
				sh.Parameters.Clear();
				if (pars != null && pars.Length > 0)
					sh.Parameters.AddRange(pars);

				if (takeReturnValue)
					sh.AddReturnParameter();

				int ret = sh.ExecuteNonQuery(storedProcedureName);
				m_IsSqlServerOK = true;

				if (takeReturnValue)
					et = sh.GetReturnValue();

				return ret;
			}
			catch (Exception ex)
			{
				OnErrorReport(ex, BuildErrorDesc4Procedure(storedProcedureName, pars));
			}
			return -1;
		}
    }
}
