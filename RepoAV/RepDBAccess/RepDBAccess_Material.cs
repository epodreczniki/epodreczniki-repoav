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
		public bool AddMaterial(MaterialAdd t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu do metody AddMaterial.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"Title",
																			"Duration",
																			"AllowDistribution",
																			"MaterialType",
																			"PublicId",
																			"Tags",
																			"Metadata");

			foreach (var de in pars)
				if (de.Key == "Id")
					de.Value.Direction = ParameterDirection.Output;
				else
					de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.AddMaterial", ps, out ret);

			if (ret == ErrorType.Success)
			{
				t.Id = (int)pars["Id"].Value;
				return true;
			}
			else if (ret == ErrorType.MaterialTypeNotFound)
				OnErrorReport(ret, string.Format("Nieznany typ materiału ({0}) przekazany do metody AddMaterial.", t.MaterialType));
			else if (ret == ErrorType.AlreadyExists)
				OnErrorReport(ret, string.Format("Istnieje już inny materiał o takim samym tytule ({0}) lub Public Id ({1}).", t.Title, t.PublicId));
			else
				OnErrorReport(ErrorType.General, string.Format("Niespodziewany błąd DB podczas dodawania materiału. Sygnatura='{0}'.", t.GetSignature()));
			return false;
		}

		public bool RemoveMaterial(int id_Material)
		{
			if (id_Material < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator materiału do metody RemoveMaterial.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id", SqlDbType.Int, id_Material) ,
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, DBNull.Value) 
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveMaterial", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.FormatGroup4MaterialExists)
				OnErrorReport(ret, string.Format("Nie można usunąć materiału o Id={0}, gdyż istnieją dla niego grupy formatów.", id_Material));
			else
				OnErrorReport(ret, string.Format("Nie istnieje materiał o Id={0}.", id_Material));

			return false;
		}

		public bool RemoveMaterial(string publicId)
		{
			if (string.IsNullOrEmpty(publicId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator publiczny materiału do metody RemoveMaterial.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id_Material", SqlDbType.Int, DBNull.Value),
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, publicId) 
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveMaterial", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.FormatGroup4MaterialExists)
				OnErrorReport(ret, string.Format("Nie można usunąć materiału o PId='{0}', gdyż istnieją dla niego grupy formatów.", publicId));
			else
				OnErrorReport(ret, string.Format("Nie istnieje materiał o PId='{0}'.", publicId));

			return false;
		}

		public bool ChangeMaterial(MaterialMod t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu MaterialMod do metody ChangeMaterial.");
				return false;
			}
			if (t.Id < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator materiału do metody ChangeMaterial.");
				return false;
			}


			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"Title",
																			"Duration",
																			"AllowDistribution",
																			"MaterialType",
																			"Metadata");

			foreach (var de in pars)
				de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.ChangeMaterial", ps, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.MaterialTypeNotFound)
                OnErrorReport(ret, string.Format("Nieznany typ materiału ({0}) przekazany do metody ChangeMaterial.", t.MaterialType));
			else if (ret == ErrorType.AlreadyExists)
				OnErrorReport(ret, string.Format("Istnieje już inny materiał o takim samym tytule ({0}).", t.Title));
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje materiał o Id = {0}. Modyfikacja niemożliwa.", t.Id));
			return false;
		}

        public bool ChangeMaterial4Portal(MaterialMod4Portal t)
        {
            if (t == null)
            {
                OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu MaterialMod do metody ChangeMaterial4Portal.");
                return false;
            }
            if (t.Id < 0)
            {
                OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator materiału do metody ChangeMaterial4Portal.");
                return false;
            }


            ErrorType ret;
            Dictionary<string, SqlParameter> pars = t.CreateSqlParameters("Id",
                                                                          "Title");

            foreach (var de in pars)
                de.Value.Direction = ParameterDirection.Input;

            SqlParameter[] ps = pars.Values.ToArray();
            ExecuteNonQuery("dbo.ChangeMaterial4Portal", ps, out ret);

            if (ret == ErrorType.Success)
                return true;
            else if (ret == ErrorType.AlreadyExists)
                OnErrorReport(ret, string.Format("Istnieje już inny materiał o takim samym tytule ({0}).", t.Title));
            else
                OnErrorReport(ErrorType.General, string.Format("Nie istnieje materiał o Id = {0}. Modyfikacja niemożliwa.", t.Id));
            return false;
        }


		public Material GetMaterial(int id_Material)
		{
			if (id_Material < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator materiału do metody GetMaterial.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<Material>(	"Id",
																					"Duration",
																					"Title",
																					"AllowDistribution",
                                                                                    "Deleted",
																					"PublicId",
																					"MaterialType",
																					"CreatedDate",
																					"ModifyDate",
																					"Status",
																					"Tags",
																					"Metadata");

			foreach (var de in pars)
				if (de.Key == "PublicId" || de.Key == "Id")
					de.Value.Direction = ParameterDirection.InputOutput;
				else
					de.Value.Direction = ParameterDirection.Output;
			pars["PublicId"].Value = DBNull.Value;
			pars["Id"].Value = id_Material;

			SqlParameter[] ps = pars.Values.ToArray();

			ErrorType ret;
			ExecuteNonQuery("dbo.GetMaterial", ps, out ret);

			if (ret == ErrorType.Success)
			{
				Material t = CreateObject<Material>(ps);
				return t;
			}
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje material o Id={0}.", id_Material));

			return null;
		}

        public Material GetMaterial4Portal(int id_Material)
        {
            if (id_Material < 0)
            {
                OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator materiału do metody GetExtMaterial.");
                return null;
            }

            Dictionary<string, SqlParameter> pars = CreateSqlParameters<Material>("Id",
                                                                                    "Duration",
                                                                                    "Title",
                                                                                    "AllowDistribution",
                                                                                    "PublicId",
                                                                                    "MaterialType",
                                                                                    "CreatedDate",
                                                                                    "ModifyDate",
                                                                                    "Status",
                                                                                    "FormatGroupsCount",
																					"Tags",
																					"Metadata");

            foreach (var de in pars)
                if (de.Key == "PublicId" || de.Key == "Id")
                    de.Value.Direction = ParameterDirection.InputOutput;
                else
                    de.Value.Direction = ParameterDirection.Output;
            pars["PublicId"].Value = DBNull.Value;
            pars["Id"].Value = id_Material;

            SqlParameter[] ps = pars.Values.ToArray();

            ErrorType ret;
            ExecuteNonQuery("dbo.GetExtMaterial", ps, out ret);

            if (ret == ErrorType.Success)
            {
                Material t = CreateObject<Material>(ps);
                return t;
            }
            else //if (ret == ErrorType.NotFound)
                OnErrorReport(ret, string.Format("Nie istnieje material o Id={0}.", id_Material));

            return null;
        }


		public Material GetMaterial(string publicId)
		{
			if (string.IsNullOrEmpty(publicId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator publiczny do metody GetMaterial.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<Material>("Id",
																					"Duration",
																					"Title",
																					"AllowDistribution",
                                                                                    "Deleted",
																					"PublicId",
																					"MaterialType",
																					"CreatedDate",
																					"ModifyDate",
																					"Status",
																					"Tags",
																					"Metadata");

			foreach (var de in pars)
				if (de.Key == "PublicId" || de.Key == "Id")
					de.Value.Direction = ParameterDirection.InputOutput;
				else
					de.Value.Direction = ParameterDirection.Output;
			pars["PublicId"].Value = publicId;
			pars["Id"].Value = DBNull.Value;

			SqlParameter[] ps = pars.Values.ToArray();

			ErrorType ret;
			ExecuteNonQuery("dbo.GetMaterial", ps, out ret);

			if (ret == ErrorType.Success)
			{
				Material t = CreateObject<Material>(ps);
				return t;
			}
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje material o PId={0}.", publicId));

			return null;
		}

        public Material GetMaterial4Format(int formatId)
        {
            if (formatId < 0)
            {
                OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator formatu do metody GetMaterial4Format.");
                return null;
            }
            Dictionary<string, SqlParameter> pars = CreateSqlParameters<Material>("Id",
                                                                                    "Duration",
                                                                                    "Title",
                                                                                    "AllowDistribution",
                                                                                    "Deleted",
                                                                                    "PublicId",
                                                                                    "MaterialType",
                                                                                    "CreatedDate",
                                                                                    "ModifyDate",
                                                                                    "Status",
																					"Tags",
																					"Metadata");

            foreach (var de in pars)
                if (de.Key == "Id")
                    de.Value.Direction = ParameterDirection.InputOutput;
                else
                    de.Value.Direction = ParameterDirection.Output;
            pars["Id"].Value = formatId;

            SqlParameter[] ps = pars.Values.ToArray();

            ErrorType ret;
            ExecuteNonQuery("dbo.GetMaterial4Format", ps, out ret);

            if (ret == ErrorType.Success)
            {
                Material t = CreateObject<Material>(ps);
                return t;
            }         
            return null;
        }

		public Material[] FindMaterials(MaterialSearch search)
		{
			if(!String.IsNullOrEmpty(search.Title))
            {
                search.Title = search.Title.Trim();
                if(search.Title.Length > 0)
                {
                    if(!search.Title.StartsWith("%"))
                    {
                        search.Title = String.Format("%{0}", search.Title);
                    }
                    if(!search.Title.EndsWith("%"))
                    {
                        search.Title = String.Format("{0}%", search.Title);
                    }
                }
            }
            
            Dictionary<string, SqlParameter> pars = search.CreateSqlParameters("DurationFrom",
																				"DurationTo",
																				"Title",
																				"AllowDistribution",
																				"MaterialType",
																				"PublicId",
																				"CreatedDateFrom",
																				"CreatedDateTo",
																				"ModifyDateFrom",
																				"ModifyDateTo",
																				"SortOrder",
																				"MaterialStatus",
																				"Tag");

			List<Material> lst = ExecuteReaderToList<Material>("dbo.FindMaterials", pars.Values.ToArray(), search);

			return lst.ToArray();
		}

		public bool SetMaterialAllowDistribution(int id_Material, bool allowDistribution)
		{
			if (id_Material < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator materiału do metody SetMaterialAllowDistribution.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id", SqlDbType.Int, id_Material) ,
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, DBNull.Value) ,
				CreateSqlParameter("AllowDistribution", SqlDbType.Bit, allowDistribution) 
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetMaterialAllowDistribution", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje materiał o Id={0}.", id_Material));

			return false;
		}

        public bool SetMaterialDeleted(int id_Material, bool deleted)
        {
            if (id_Material < 0)
            {
                OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator materiału do metody SetMaterialAllowDistribution.");
                return false;
            }

            SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id", SqlDbType.Int, id_Material) ,
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, DBNull.Value) ,
				CreateSqlParameter("Deleted", SqlDbType.Bit, deleted) 
			};

            ErrorType ret;
            ExecuteNonQuery("dbo.SetMaterialDeleted", pars, out ret);

            if (ret == ErrorType.Success)
                return true;
            else
                OnErrorReport(ret, string.Format("Nie istnieje materiał o Id={0}.", id_Material));

            return false;
        }


		public bool SetMaterialAllowDistribution(string publicId, bool allowDistribution)
		{
			if (string.IsNullOrEmpty(publicId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator publiczny materiału do metody SetMaterialAllowDistribution.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id", SqlDbType.Int, DBNull.Value) ,
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, publicId) ,
				CreateSqlParameter("AllowDistribution", SqlDbType.Bit, allowDistribution) 
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.SetMaterialAllowDistribution", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else
				OnErrorReport(ret, string.Format("Nie istnieje materiał o PId={0}.", publicId));

			return false;
		}


		public MaterialStatus GetMaterialStatus(string publicId)
		{
			if (string.IsNullOrEmpty(publicId))
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator publiczny materiału do metody GetMaterialStatus.");
				return MaterialStatus.NotFound;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, publicId) ,
				CreateSqlParameter("Status", SqlDbType.SmallInt, DBNull.Value, ParameterDirection.Output) 
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.GetMaterialStatus", pars, out ret);

			if (ret == ErrorType.Success)
				return (MaterialStatus)((short)pars[1].Value);
			else
				return MaterialStatus.NotFound;
		}


		public Statistics GetStatistics()
		{
			Dictionary<string, SqlParameter> pars = CreateSqlParameters<Statistics>();
			SqlParameter[] ps = pars.Values.ToArray();

			ExecuteNonQuery("dbo.GetStatistics", ps);
			Statistics t = CreateObject<Statistics>(ps);

			return t;
		}

        public Statistics[] GetStatistics2()
        {
            List<Statistics> lst = ExecuteReaderToList<Statistics>("dbo.GetStatistics2", null);

            return lst.ToArray();
        }


		public Material[] GetMaterials2Remove(RangeSelector rs)
		{
			List<Material> lst = ExecuteReaderToList<Material>("dbo.GetMaterials2Remove", null, rs);

			return lst.ToArray();
		}


		public string[] GetPublicIds4ChangedMaterialsSince(DateTime from, DateTime to)
		{
			SqlParameter[] pars = new SqlParameter[]
			{
				CreateSqlParameter("From", SqlDbType.DateTime, DBNull.Value),
				CreateSqlParameter("To", SqlDbType.DateTime, DBNull.Value)
			};

			if (from > DateTime.MinValue)
				pars[0].Value = from;
			if (to > DateTime.MinValue)
				pars[1].Value = to;

			List<string> lst = ExecuteReaderToList<string>("dbo.GetPublicIds4ChangedMaterialsSince", pars);

			return lst.ToArray();
		}
    }
}
