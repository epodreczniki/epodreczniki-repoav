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
		public bool AddFormat(FormatMod t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu do metody AddFormat.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"FormatGroupId",
																			"ProfileId",
																			"XmlMetadata",
																			"Type",
																			"UniqueId",
																			"Size",
																			"InternalStatus",
																			"Mime");

			foreach (var de in pars)
				if (de.Key == "Id")
					de.Value.Direction = ParameterDirection.Output;
				else
					de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.AddFormat", ps, out ret);

			if (ret == ErrorType.Success)
			{
				t.Id = (int)pars["Id"].Value;
				return true;
			}
			else if (ret == ErrorType.FormatGroupNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje grupa o Id={0} przekazana do metody AddFormat.", t.FormatGroupId));
			else if (ret == ErrorType.ProfileNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje profil o Id={0} przekazany do metody AddFormat.", t.ProfileId ?? -1));
			else if (ret == ErrorType.FormatTypeNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje typ format o Id={0} przekazanydo metody AddFormat.", t.Type));
			else if (ret == ErrorType.AlreadyExists)
				OnErrorReport(ret, string.Format("Istnieje już format o UniqueId={0} - AddFormat.", t.UniqueId));
			else
				OnErrorReport(ErrorType.General, string.Format("Niespodziewany błąd DB podczas dodawania formatu. Sygnatura='{0}'.", t.GetSignature()));
			return false;
		}

		public bool RemoveFormat(int id_Format)
		{
			if (id_Format < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator materiału do metody RemoveFormat.");
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

		public bool ChangeFormat(FormatMod t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu do metody ChangeFormat.");
				return false;
			}
			if (t.Id < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator grupy do metody ChangeFormat.");
				return false;
			}


			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"FormatGroupId",
																			"ProfileId",
																			"XmlMetadata",
																			"Type",
																			"UniqueId",
																			"Size",
																			"Mime");

			foreach (var de in pars)
				de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.ChangeFormat", ps, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.FormatGroupNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje grupa o Id={0} przekazana do metody ChangeFormat.", t.FormatGroupId));
			else if (ret == ErrorType.ProfileNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje profil o Id={0} przekazany do metody ChangeFormat.", t.ProfileId ?? -1));
			else if (ret == ErrorType.FormatTypeNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje typ format o Id={0} przekazanydo metody ChangeFormat.", t.Type));
			else if (ret == ErrorType.AlreadyExists)
				OnErrorReport(ret, string.Format("Istnieje już format o UniqueId={0} - ChangeFormat.", t.UniqueId));
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje grupa o Id = {0}. Modyfikacja niemożliwa.", t.Id));
			return false;
		}

		public Format GetFormat(int id_Format)
		{
			if (id_Format < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody GetFormat.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<Format>("Id",
																				"FormatGroupId",
																				"ProfileId",
																				"XmlMetadata",
																				"Type",
																				"UniqueId",
																				"Size",
																				"CreatedDate",
																				"Status",
																				"InternalStatus",
																				"AllowDistribution",
																				"Mime");

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
				Format t = CreateObject<Format>(ps);
				return t;
			}
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje format o Id={0}.", id_Format));

			return null;
		}

		public Format GetFormat(string uniqueId)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody GetFormat.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<Format>("Id",
																				"FormatGroupId",
																				"ProfileId",
																				"XmlMetadata",
																				"Type",
																				"UniqueId",
																				"Size",
																				"CreatedDate",
																				"Status",
																				"InternalStatus",
																				"AllowDistribution",
																				"Mime");

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

			if (ret == ErrorType.Success)
			{
				Format t = CreateObject<Format>(ps);
				return t;
			}
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o UId={0}.", uniqueId));

			return null;
		}

		public Format[] GetFormatsFromSameMaterial(string uniqueId)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody GetFormatsFromSameMaterial.");
				return null;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			List<Format> lst = ExecuteReaderToList<Format>("dbo.GetFormatsFromSameMaterial", pars, out ret);

			if (ret == ErrorType.Success)
				return lst.ToArray();
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje format o UniqueId = {0}.", uniqueId));

			return null;
		}

		public Format[] GetFormats4Material(string publicId)
		{
			if (string.IsNullOrEmpty(publicId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator publiczny do metody GetFormats4Material.");
				return null;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, publicId)
			};

			ErrorType ret;
			List<Format> lst = ExecuteReaderToList<Format>("dbo.GetFormats4Material", pars, out ret);

			if (ret == ErrorType.Success)
				return lst.ToArray();

			return null;
		}

		public Format[] GetFormats4Group(int formatGroupId)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("FormatGroupId", SqlDbType.Int, formatGroupId)
			};


			ErrorType ret;
			List<Format> lst = ExecuteReaderToList<Format>("dbo.GetFormats4Group", pars, out ret);

			if (ret == ErrorType.Success)
				return lst.ToArray();
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje grupa o Id = {0}.", formatGroupId));

			return lst.ToArray();
		}

		public Format[] GetFormatsFromSameGroupAndProfile(string uniqueId)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody GetFormatsFromSameGroupAndProfile.");
				return null;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			List<Format> lst = ExecuteReaderToList<Format>("dbo.GetFormatsFromSameGroupAndProfile", pars, out ret);

			if (ret == ErrorType.Success)
				return lst.ToArray();
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje format o UniqueId = {0}.", uniqueId));

			return null;
		}

		public Dictionary<string, List<FormatWithProfileGroup>> GetFormats4MaterialByGroup(string publicId)//słownik: (id grupy formatów + ":" + id grupy profili) -> tablica formatów
		{
			if (string.IsNullOrEmpty(publicId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator publiczny do metody GetFormatsFromSameMaterial.");
				return null;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, publicId)
			};

			ErrorType ret;
			List<FormatWithProfileGroup> lst = ExecuteReaderToList<FormatWithProfileGroup>("dbo.GetFormats4MaterialByGroup", pars, out ret);

			if (ret == ErrorType.Success)
			{
				Dictionary<string, List<FormatWithProfileGroup>> res = new Dictionary<string, List<FormatWithProfileGroup>>();
				if (lst != null)
				{
					List<FormatWithProfileGroup> tmp = null;
					foreach(var f in lst)
					{
						string sign = string.Format("{0}:{1}", f.FormatGroupId, f.Id_ProfileGroup.HasValue ? f.Id_ProfileGroup.Value.ToString() : "NULL");
						if (res.ContainsKey(sign))
							tmp = res[sign];
						else
						{
							tmp = new List<FormatWithProfileGroup>();
							res.Add(sign, tmp);
						}
						tmp.Add(f);
					}					
				}
				return res;
			}
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje materiał o publicznym Id='{0}'.", publicId));

			return null;
		}

		public bool SetFormatMetadata(string uniqueId, string xmlMetadata)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatMetadata.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("XmlMetadata", SqlDbType.NVarChar, -1, xmlMetadata),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetFormatMetadata", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o UId={0}.", uniqueId));

			return false;
		}

		public bool SetFormatMetadataExt(string uniqueId, string xmlMetadata, int? duration, long? size, string mime)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatMetadataExt.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("XmlMetadata", SqlDbType.NVarChar, -1, DBNull.Value),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId),
				CreateSqlParameter("Duration", SqlDbType.Int, DBNull.Value),
				CreateSqlParameter("Size", SqlDbType.BigInt, DBNull.Value),
				CreateSqlParameter("Mime", SqlDbType.VarChar, 50, mime)
			};

			if (duration.HasValue)
				pars[2].Value = duration.Value;

			if (size.HasValue)
				pars[3].Value = size.Value;

			if (!string.IsNullOrEmpty(xmlMetadata))
				pars[0].Value = xmlMetadata;

			ErrorType ret;
			ExecuteNonQuery("dbo.SetFormatMetadataExt", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o UId={0}.", uniqueId));

			return false;
		}

		public bool SetFormatStatus(string uniqueId, FormatStatus oldStatus, FormatStatus newStatus)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatStatus.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("OldStatus", SqlDbType.SmallInt, (short)oldStatus),
				CreateSqlParameter("NewStatus", SqlDbType.SmallInt, (short)newStatus),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetFormatStatus", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.StateChangedMeanwhile)
				OnErrorReport(ret, string.Format("Stan formatu o UId={0} został zmieniony w międzyczasie.", uniqueId));
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o UId={0}.", uniqueId));

			return false;
		}

		public bool SetFormatInternalStatus(string uniqueId, FormatInternalStatus oldStatus, FormatInternalStatus newStatus)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody SetFormatInternalStatus.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("OldStatus", SqlDbType.SmallInt, (short)oldStatus),
				CreateSqlParameter("NewStatus", SqlDbType.SmallInt, (short)newStatus),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetFormatInternalStatus", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.StateChangedMeanwhile)
				OnErrorReport(ret, string.Format("Stan wewnętrzny formatu o UId={0} został zmieniony w międzyczasie.", uniqueId));
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o UId={0}.", uniqueId));

			return false;
		}

		public bool MaterialFormatWasAddedToNode(string uniqueId, int nodeId)
		{
			if (string.IsNullOrEmpty(uniqueId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody MaterialFormatWasAddedToNode.");
				return false;
			}
			if (nodeId < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator węzła do metody MaterialFormatWasAddedToNode.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("NodeId", SqlDbType.Int, nodeId),
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.AddFormatLocation", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.NodeNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0} - nie można dodać lokalizacji.", nodeId));
			else
				OnErrorReport(ret, string.Format("Nie istnieje format o UId={0}.", uniqueId));

			return false;
		}

		public int[] GetFormatLocations(string uniqueId)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			List<int> lst = ExecuteReaderToList<int>("dbo.GetFormatLocations", pars, out ret);

			if (ret == ErrorType.Success)
				return lst.ToArray();
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje format o UniqueId = {0}.", uniqueId));

			return null;
		}

		public FormatLocation[] GetFormatLocationsExt(string uniqueId)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId)
			};

			ErrorType ret;
			List<FormatLocation> lst = ExecuteReaderToList<FormatLocation>("dbo.GetFormatLocationsExt", pars, out ret);

			if (ret == ErrorType.Success)
				return lst.ToArray();
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje format o UniqueId = {0}.", uniqueId));

			return null;
		}

		public FormatSource GetSourceUrl4Format(string uniqueId)
		{
			Dictionary<string, SqlParameter> pars = CreateSqlParameters<FormatSource>();
			pars["UniqueId"].Value = uniqueId;

			SqlParameter[] ps = pars.Values.ToArray();

			ErrorType ret;
			ExecuteNonQuery("dbo.GetSourceUrl4Format", ps, out ret);

			if (ret == ErrorType.Success)
			{
				FormatSource t = CreateObject<FormatSource>(ps);
				return t;
			}
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje format o UniqueId = {0}.", uniqueId));

			return null;
		}

		public bool AddFormatLocation(string uniqueId, int nodeId, long currentFreeSpace)
		{
			if (string.IsNullOrEmpty(uniqueId) || nodeId < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawne parametry wejściowe do metody AddFormatLocation.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[]
			{
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId, ParameterDirection.Input),
				CreateSqlParameter("NodeId", SqlDbType.Int, nodeId, ParameterDirection.Input),
				CreateSqlParameter("FreeSpace", SqlDbType.BigInt, DBNull.Value, ParameterDirection.Input)
			};

			if (currentFreeSpace >= 0)
				pars[2].Value = currentFreeSpace;

			ErrorType ret;
			ExecuteNonQuery("dbo.AddFormatLocation", pars, out ret);

			if (ret == ErrorType.Success)
			{
				return true;
			}
			else if (ret == ErrorType.FormatNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje format o podanym UniqueId={0} przekazany do metody AddFormatLocation.", uniqueId));
			else if (ret == ErrorType.NodeNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0} przekazany do metody AddFormatLocation.", nodeId));
			return false;
		}

		public bool AddFormatLocationIfMetadataExists(string uniqueId, int nodeId, long currentFreeSpace, out bool removeFormatFromRepo)
		{
			removeFormatFromRepo = false;
			if (string.IsNullOrEmpty(uniqueId) || nodeId < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawne parametry wejściowe do metody AddFormatLocation.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[]
			{
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId, ParameterDirection.Input),
				CreateSqlParameter("NodeId", SqlDbType.Int, nodeId, ParameterDirection.Input),
				CreateSqlParameter("FreeSpace", SqlDbType.BigInt, DBNull.Value, ParameterDirection.Input)
			};

			if (currentFreeSpace >= 0)
				pars[2].Value = currentFreeSpace;

			ErrorType ret;
			ExecuteNonQuery("dbo.AddFormatLocation", pars, out ret);

			if (ret == ErrorType.Success)
			{
				return true;
			}
			else if (ret == ErrorType.FormatNotFound)
			{
				removeFormatFromRepo = true;
				return true;
			}
			else if (ret == ErrorType.NodeNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0} przekazany do metody AddFormatLocationIfMetadataExists.", nodeId));
			return false;
		}

		public string GetFileExtension4Mime(string mimeType)
		{
			SqlParameter[] pars = new SqlParameter[]
			{
				CreateSqlParameter("Mime", SqlDbType.VarChar, 50, mimeType),
				CreateSqlParameter("FileExtension", SqlDbType.VarChar, 10, null, ParameterDirection.Output)
			};


			ExecuteNonQuery("dbo.GetFileExtension4Mime", pars);

			if (pars[1].Value != DBNull.Value)
				return (string)pars[1].Value;

			return null;
		}


		public bool RemoveFormatLocation(string uniqueId, int nodeId, long currentFreeSpace)
		{
			if (string.IsNullOrEmpty(uniqueId) || nodeId < 0 )
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawne parametry wejściowe do metody RemoveFormatLocation.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[]
			{
				CreateSqlParameter("UniqueId", SqlDbType.VarChar, 150, uniqueId, ParameterDirection.Input),
				CreateSqlParameter("NodeId", SqlDbType.Int, nodeId, ParameterDirection.Input),
				CreateSqlParameter("FreeSpace", SqlDbType.BigInt, DBNull.Value, ParameterDirection.Input)
			};

			if (currentFreeSpace >= 0)
				pars[2].Value = currentFreeSpace;

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveFormatLocation", pars, out ret);

			if (ret == ErrorType.Success)
			{
				return true;
			}
			else if (ret == ErrorType.FormatNotFound)
                OnErrorReport(ret, string.Format("Nie istnieje format o podanym UniqueId={0} przekazany do metody RemoveFormatLocation.", uniqueId));
			else if (ret == ErrorType.NodeNotFound)
                OnErrorReport(ret, string.Format("Nie istnieje węzeł o Id={0} przekazany do metody RemoveFormatLocation.", nodeId));
			return false;
		}


		public FormatData4Sync[] GetFormats4Sync(FormatSelector4Sync sel)
		{
			Dictionary<string, SqlParameter> pars = sel.CreateSqlParameters();

			List<FormatData4Sync> lst = ExecuteReaderToList<FormatData4Sync>("dbo.GetFormats4Sync", pars.Values.ToArray(), sel);

			if (lst != null)
				return lst.ToArray();

			return null;
		}

		public Format[] GetFormats4WithInternalStatus(FormatSelector4IntStat sel)
		{
			Dictionary<string, SqlParameter> pars = sel.CreateSqlParameters();

			List<Format> lst = ExecuteReaderToList<Format>("dbo.GetFormats4WithInternalStatus", pars.Values.ToArray(), sel);

			if (lst != null)
				return lst.ToArray();

			return null;
		}


		public Mime2Extension[] GetAllMime2FileExtension()
		{
			List<Mime2Extension> lst = ExecuteReaderToList<Mime2Extension>("dbo.GetAllMime2FileExt", null);
			if (lst != null)
				return lst.ToArray();
			return null;
		}

		public Format[] GetFormatsWithoutNumOfLocations(NumOfCopiesSelector sel)
		{
			Dictionary<string, SqlParameter> pars = sel.CreateSqlParameters();

			List<Format> lst = ExecuteReaderToList<Format>("dbo.GetFormatsWithoutNumOfLocations", pars.Values.ToArray(), sel);

			if (lst != null)
				return lst.ToArray();

			return null;
		}


		public FormatExt[] GetExtFormats4Material(string publicId)
		{
			if (string.IsNullOrEmpty(publicId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator publiczny do metody GetExtFormats4Material.");
				return null;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, publicId)
			};

			ErrorType ret;
			List<FormatExt> lst = ExecuteReaderToList<FormatExt>("dbo.GetExtFormats4Material", pars, out ret);

			if (ret == ErrorType.Success)
				return lst.ToArray();

			return null;
		}

		public FormatExt[] GetExtFormats4Group(int formatGroupId)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("FormatGroupId", SqlDbType.Int, formatGroupId)
			};


			ErrorType ret;
			List<FormatExt> lst = ExecuteReaderToList<FormatExt>("dbo.GetExtFormats4Group", pars, out ret);

			if (ret == ErrorType.Success)
				return lst.ToArray();
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje grupa o Id = {0}.", formatGroupId));

			return lst.ToArray();
		}
    }
}
