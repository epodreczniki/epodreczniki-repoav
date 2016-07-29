using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using System.Data;
using System.IO;

namespace PSNC.RepoAV.DBAccess
{
	public class BaseObject
	{
		public BaseObject()
		{
		}

		public static Dictionary<string, object> CreateSqlParameters<T>(T obj)
		{
			Dictionary<string, object> pars = new Dictionary<string, object>();
			if (obj != null)
			{
				System.Reflection.PropertyInfo[] pis = obj.GetType().GetProperties();
				foreach (var prop in pis)
				{
					if (prop.CanRead)
					{
						SqlParameterAttribute attrib = (SqlParameterAttribute)prop.GetCustomAttributes(true).FirstOrDefault(a => { return a is SqlParameterAttribute; });

						if (attrib != null)
						{
							object val = prop.GetValue(obj, null);
							if (val is Guid && ((Guid)val) == Guid.Empty)
							{
								pars.Add(prop.Name, null);
							}
							else if (val is DateTime && ((DateTime)val) == DateTime.MinValue)
							{
								pars.Add(prop.Name, null);
							}
							else
							{
								object objVal = prop.GetValue(obj, null);
								if (attrib.TreatMinusOneAsNull && objVal != null)
								{
									if ((objVal is int && (int)objVal == -1)
										|| (objVal is long && (long)objVal == -1)
										|| (objVal is short && (short)objVal == -1)
										|| (objVal is float && (float)objVal == -1)
										|| (objVal is double && (double)objVal == -1))
										pars.Add(prop.Name, null);
									else if (objVal is string && ((string)objVal).Length == 0)
										pars.Add(prop.Name, null);
									else
										pars.Add(prop.Name, objVal);
								}
								else
									pars.Add(prop.Name, objVal);
							}
						}
					}
				}
			}
			return pars;
		}

		public Dictionary<string, SqlParameter> CreateSqlParameters(params string[] onlyTheseParams)
		{
			return BaseDBAccess.CreateSqlParameters(GetType(), this, onlyTheseParams);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			Type t = GetType();
			sb.AppendFormat("Type = {0}\r\n", t.Name);
			foreach (var p in t.GetProperties())
			{
				sb.AppendFormat("  {0} = {1}\r\n", p.Name, p.GetValue(this, null) ?? "NULL");
			}

			return sb.ToString();
		}

		public static void Print(object o, StringBuilder sb, string prefix)
		{
			if (o == null)
			{
				sb.AppendFormat("NULL\r\n");
				return;
			}

			if (o is string)
			{
				sb.AppendFormat("{0}\r\n", o);
				return;
			}

			Type t = o.GetType();

			if (prefix.Length > 0 && (t.IsArray || t.IsClass) && t != typeof(string) && t != typeof(DateTime))
				sb.AppendLine();

			if ((t.IsArray || t.IsClass) && t != typeof(string) && t != typeof(DateTime))
				sb.AppendFormat("{1}{0}\r\n", t.Name, prefix);



			if (t.IsArray)
			{
				sb.AppendFormat("  {1}Length = {0}\r\n", t.GetProperty("Length").GetValue(o, null), prefix);
				for (int i = 0; i < Math.Min(((Array)o).Length, 20); i++)
				{
					object elem = ((Array)o).GetValue(i);
					sb.AppendFormat("    {0}{1} : ", prefix, i);
					Print(elem, sb, prefix + "    ");
				}
			}
			else if (t.IsClass)
			{
				foreach (var p in t.GetProperties())
				{
					if (p.CanRead)
					{
						object val = p.GetValue(o, null);
						sb.AppendFormat("  {1}{0} = ", p.Name, prefix);
						Print(val, sb, prefix + "  ");
					}
					else
						sb.AppendFormat("  {1}{0} = ???", p.Name, prefix);
				}
			}
			else
				sb.AppendFormat("{0}\r\n", o);
		}

		public void Print(StringBuilder sb, string prefix)
		{
			BaseObject.Print(this, sb, prefix);
		}

		protected virtual void PrepareSignature(StringBuilder sb)
		{
			foreach (var p in GetType().GetProperties())
				sb.AppendFormat("{0}={1};", p.Name, p.GetValue(this, null) ?? "NULL");
		}

		public string GetSignature()
		{
			StringBuilder sb = new StringBuilder();
			PrepareSignature(sb);

			return sb.ToString();
		}

	}
}
