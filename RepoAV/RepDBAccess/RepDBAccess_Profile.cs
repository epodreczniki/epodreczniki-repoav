using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;
using System.Data;
using System.Data.SqlClient;
using PSNC.RepoAV.Common;
using System.Xml;

namespace PSNC.RepoAV.RepDBAccess
{
	public partial class RepDBAccess : BaseDBAccess
    {
		public bool AddProfile(Profile t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu Profile do metody AddProfile.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"Name",
																			"Id_ProfileGroup",
																			"MinHeight",
																			"MinWidth",
																			"Apect",
																			"Mime");

			foreach (var de in pars)
				if (de.Key == "Id")
					de.Value.Direction = ParameterDirection.Output;
				else
					de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.AddProfile", ps, out ret);

			if (ret == ErrorType.Success)
			{
				t.Id = (int)pars["Id"].Value;
				return true;
			}
			else
				OnErrorReport(ErrorType.General, string.Format("Niespodziewany błąd DB podczas dodawania profilu. Sygnatura='{0}'.", t.GetSignature()));
			return false;
		}

		public bool RemoveProfile(int id_Profile)
		{
			if (id_Profile < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody RemoveProfile.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] { CreateSqlParameter("Id", SqlDbType.Int, id_Profile) };

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveProfile", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.FormatWithProfileExists)
				OnErrorReport(ret, string.Format("Nie można usunąć profilu o Id={0}, gdyż istnieją dla niego formaty.", id_Profile));
			else
				OnErrorReport(ret, string.Format("Nie istnieje profil o Id={0}.", id_Profile));

			return false;
		}

		public bool ChangeProfile(Profile t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu Profile do metody ChangeProfile.");
				return false;
			}
			if (t.Id < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody ChangeProfile.");
				return false;
			}


			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters("Id",
																			"Name",
																			"Id_ProfileGroup",
																			"MinHeight",
																			"MinWidth",
																			"Apect",
																			"Mime");

			foreach (var de in pars)
				de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.ChangeProfile", ps, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ErrorType.General, string.Format("Nie isntieje profil o Id = {0}. Modyfikacja niemożliwa.", t.Id));
			return false;
		}

		public Profile GetProfile(int id_Profile)
		{
			if (id_Profile < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator profilu do metody GetProfile.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<Profile>("Id",
																				"Name",																				
																				"MinHeight",
																				"MinWidth",
																				"Apect", 
																				"Mime",
																				"Id_ProfileGroup",
																				"Enabled");



            foreach (var de in pars)
                if (de.Key != "Id")
                    de.Value.Direction = ParameterDirection.Output;
                else
                    de.Value.SqlValue = id_Profile;

			SqlParameter[] ps = pars.Values.ToArray();

			ErrorType ret;
			ExecuteNonQuery("dbo.GetProfile", ps, out ret);

			if (ret == ErrorType.Success)
			{
				Profile t = CreateObject<Profile>(ps);
				return t;
			}
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje profil o Id={0}.", id_Profile));

			return null;
		}

		public ProfileGroup[] GetAllProfileGroups()
		{
			List<ProfileGroup> lst = ExecuteReaderToList<ProfileGroup>("dbo.GetAllProfileGroups", null);

			return lst.ToArray();
		}

		public Profile[] GetProfilesFromGroup(int id_ProfileGroup)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_ProfileGroup", SqlDbType.Int, id_ProfileGroup)
			};

			List<Profile> lst = ExecuteReaderToList<Profile>("dbo.GetProfilesFromGroup", pars);

			return lst.ToArray();
		}

		public bool AddProfileGroup(ProfileGroup pg)
		{
			if (pg == null || string.IsNullOrEmpty(pg.Name))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu Profile do metody AddProfileGroup.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = pg.CreateSqlParameters("Id",
																			"Name",
																			"OperationXML",
																			"MaterialType",
																			"Enabled",
																			"TaskSubtype");

			foreach (var de in pars)
				if (de.Key == "Id")
					de.Value.Direction = ParameterDirection.Output;
				else
					de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.AddProfileGroup", ps, out ret);

			if (ret == ErrorType.Success)
			{
				pg.Id = (int)pars["Id"].Value;
				return true;
			}
			else
				OnErrorReport(ErrorType.General, string.Format("Niespodziewany błąd DB podczas dodawania grupy profili. Sygnatura='{0}'.", pg.GetSignature()));
			return false;
		}

		public bool ChangeProfileGroup(ProfileGroup pg)
		{
			if (pg == null || string.IsNullOrEmpty(pg.Name))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu Profile do metody ChangeProfileGroup.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = pg.CreateSqlParameters("Id",
																			"Name",
																			"OperationXML",
																			"MaterialType",
																			"Enabled",
																			"TaskSubtype");

			foreach (var de in pars)
				de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.ChangeProfileGroup", ps, out ret);

			if (ret == ErrorType.Success)
			{
				pg.Id = (int)pars["Id"].Value;
				return true;
			}
			else
				OnErrorReport(ErrorType.General, string.Format("Niespodziewany błąd DB podczas modyfikacji grupy profili. Sygnatura='{0}'.", pg.GetSignature()));
			return false;
		}

		public bool RemoveProfileGroup(int id_ProfileGroup)
		{
			if (id_ProfileGroup < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody RemoveProfileGroup.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] { CreateSqlParameter("Id", SqlDbType.Int, id_ProfileGroup) };

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveProfileGroup", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.FormatWithProfileExists)
				OnErrorReport(ret, string.Format("Nie można usunąć grupy profili o Id={0}, gdyż istnieją formaty używające profili z tej grupy.", id_ProfileGroup));
			else
				OnErrorReport(ret, string.Format("Nie istnieje grupa profili o Id={0}.", id_ProfileGroup));

			return false;
		}		

		public ProfileGroupExt[] GetProfilesWithGroups4MaterialType(MaterialType? matType)
		{
			try
			{
				SqlParameter[] pars = new SqlParameter[] 
				{ 
					CreateSqlParameter("MaterialType", SqlDbType.SmallInt, DBNull.Value)
				};

				if (matType.HasValue)
					pars[0].Value = matType.Value;

				Dictionary<int, ProfileGroupExt> dic = new Dictionary<int, ProfileGroupExt>();
				List<ProfileGroupExt> lst = new List<ProfileGroupExt>();
				using (SqlHelper sh = new SqlHelper(m_ConnectionString))
				{
					sh.CommandTimeout = m_ExecutionTimeout;

					sh.Parameters.Add(pars[0]);

					sh.ExecuteReader("dbo.GetProfilesWithGroups4MaterialType");
					if (sh.Reader != null)
					{
						while (sh.Reader.Read())
						{
							Dictionary<string, object> row = sh.GetDictionaryFromCurrentReaderRow();
							if (row != null)
							{
								int id_group = (int)row["Id_ProfileGroup"];
								ProfileGroupExt pg = null;
								if (dic.ContainsKey(id_group))
								{
									pg = dic[id_group];
								}
								else
								{
									pg = new ProfileGroupExt();
									pg.Name = (string)row["GroupName"];
									pg.OperationXML = (string)row["OperationXML"];
									pg.Id = id_group;
									pg.Enabled = (bool)row["Enabled"];
									pg.MaterialType = (MaterialType)((short)row["MaterialType"]);
                                    pg.TaskSubtype = (string)row["TaskSubtype"];
                                    pg.DownloadSourceFiles = (string)row["DownloadSourceFiles"];
									dic[id_group] = pg;                                  
								}
								row.Remove("GroupName");
								row.Remove("OperationXML");
								row.Remove("MaterialType");

								Profile p = CreateObject<Profile>(row);
								pg.Profiles.Add(p);
							}
						}
						sh.Reader.Close();
					}
				}
				m_IsSqlServerOK = true;
				return dic.Values.ToArray();
			}
			catch (Exception ex)
			{
				OnErrorReport(ex, "Bład podczas pobierania wszystkich profili wraz z grupami profili.");
			}
			return null;
		}

		public bool SetProfileGroupEnabled(int id_ProfileGroup, bool enabled)
		{
			if (id_ProfileGroup < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody SetProfileGroupEnabled.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id", SqlDbType.Int, id_ProfileGroup),
				CreateSqlParameter("Enabled", SqlDbType.Bit, enabled) 
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetProfileGroupEnabled", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje grupa profili o Id={0}.", id_ProfileGroup));

			return false;
		}

		public string GetOperationXml4Profile(int id_Profile)
		{
			if (id_Profile < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator zadania do metody GetOperationXml4Profile.");
				return null;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Profile", SqlDbType.Int, id_Profile),
				CreateSqlParameter("OperationXML", SqlDbType.NVarChar, -1, null, ParameterDirection.Output) 
			};

			ExecuteNonQuery("dbo.GetOperationXml4Profile", pars);

			if (pars[1].Value != DBNull.Value)
				return (string)pars[1].Value;
			return null;
		}

		public string GetFileExtension4Profile(int id_Profile)
		{
			SqlParameter[] pars = new SqlParameter[]
			{
				CreateSqlParameter("Id_Profile", SqlDbType.Int, id_Profile),
				CreateSqlParameter("FileExtension", SqlDbType.VarChar, 10, null, ParameterDirection.Output)
			};

			ErrorType et;
			ExecuteNonQuery("dbo.GetFileExtension4Profile", pars, out et);

			if (et == ErrorType.Success)
			{
				if (pars[1].Value != DBNull.Value)
					return (string)pars[1].Value;
			}
			else
				OnErrorReport(et, string.Format("Nie istnieje profil o Id={0}.", id_Profile));


			return null;
		}



		public string GetEncodingProfilesXml()
		{
			List<ProfileData4XML> lst = ExecuteReaderToList<ProfileData4XML>("dbo.GetProfiles4XML", null);

			if (lst != null)
			{
				StringBuilder sb = new StringBuilder();
				int currentProfile = -1;
				foreach (ProfileData4XML pd in lst)
				{
					if (currentProfile != pd.Id)
					{
						if (currentProfile > -1)
							sb.Append("</Profile>");
						sb.Append(string.Format("<Profile xmlns:m=\"http://schemas.psnc.pl/media-metadata/1.0\" Name=\"{0}\" AudioOnly=\"{1}\" FileFormat=\"{2}\">", pd.Name ?? "", pd.AudioOnly ? "true" : "false", pd.FileFormat ?? ""));
					}
					currentProfile = pd.Id;

					if (pd.VideoBitrate > -1 || pd.FrameWidth > -1 || pd.FrameHeight > -1 || pd.Framerate > -1 || pd.VideoCoding != null)
					{
						sb.Append("<Video>");
						sb.Append("<m:ColorDomain>unknown</m:ColorDomain>");
						sb.Append("<m:Frame>");
						sb.Append(string.Format("<m:Width>{0}</m:Width>", pd.FrameWidth > -1 ? pd.FrameWidth.ToString() : "unknown"));
						sb.Append(string.Format("<m:Height>{0}</m:Height>", pd.FrameHeight > -1 ? pd.FrameHeight.ToString() : "unknown"));
						sb.Append("</m:Frame>");
						sb.Append(string.Format("<m:Bitrate>{0}</m:Bitrate>", pd.VideoBitrate > -1 ? pd.VideoBitrate.ToString() : "unknown"));
						sb.Append(string.Format("<m:FrameRate>{0}</m:FrameRate>", pd.Framerate > -1 ? pd.Framerate.ToString() : "unknown"));
						sb.Append(string.Format("<m:Coding>{0}</m:Coding>", pd.VideoCoding ?? "unknown"));
						sb.Append("</Video>");
					}
					if (pd.AudioBitrate > -1 || !string.IsNullOrEmpty(pd.AudioCoding) || pd.Sample > -1 || pd.SampleRate > -1)
					{
						sb.Append("<Audio>");
						sb.Append(string.Format("<m:Bitrate>{0}</m:Bitrate>", pd.AudioBitrate > -1 ? pd.AudioBitrate.ToString() : "unknown"));						
						sb.Append(string.Format("<m:Coding>{0}</m:Coding>", pd.AudioCoding ?? "unknown"));
						sb.Append(string.Format("<m:Sample>{0}</m:Sample>", pd.Sample > -1 ? pd.Sample.ToString() : "unknown"));
						sb.Append(string.Format("<m:SampleRate>{0}</m:SampleRate>", pd.SampleRate > -1 ? pd.SampleRate.ToString() : "unknown"));
						sb.Append("</Audio>");
					}
				}

				if (sb.Length > 0)
					sb.Append("</Profile>");

				return sb.ToString();
			}
			return string.Empty;
		}

    }
}
