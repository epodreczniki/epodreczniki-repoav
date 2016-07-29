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
		public bool AddFormatGroup(FormatGroup t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu do metody AddFormatGroup.");
				return false;
			}

			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"MaterialId",
																			"SubtitleId",
																			"SourceId",
																			"AudioId");

			foreach (var de in pars)
				if (de.Key == "Id")
					de.Value.Direction = ParameterDirection.Output;
				else
					de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.AddFormatGroup", ps, out ret);

			if (ret == ErrorType.Success)
			{
				t.Id = (int)pars["Id"].Value;
				return true;
			}
			else if (ret == ErrorType.MaterialNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje materiał o Id={0} przekazany do metody AddFormatGroup.", t.MaterialId));
			else if (ret == ErrorType.SubtitleFormatNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje format przekazanydo metody jako SubtitleId={0}.", t.SubtitleId));
			else if (ret == ErrorType.FormatNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje format przekazanydo metody jako SourceId={0}.", t.SourceId));
			else
				OnErrorReport(ErrorType.General, string.Format("Niespodziewany błąd DB podczas dodawania grupy formatów. Sygnatura='{0}'.", t.GetSignature()));
			return false;
		}

		public bool RemoveFormatGroup(int id_FormatGroup)
		{
			if (id_FormatGroup < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator materiału do metody RemoveFormatGroup.");
				return false;
			}

			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("Id", SqlDbType.Int, id_FormatGroup)
			};

			ErrorType ret;
			ExecuteNonQuery("dbo.RemoveFormatGroup", pars, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.Format4GroupExists)
				OnErrorReport(ret, string.Format("Nie można usunąć grupy o Id={0}, gdyż istnieją dla niego formaty.", id_FormatGroup));
			else
				OnErrorReport(ret, string.Format("Nie istnieje grupa o Id={0}.", id_FormatGroup));

			return false;
		}

		public bool ChangeFormatGroup(FormatGroup t)
		{
			if (t == null)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Nie przekazano obiektu do metody ChangeFormatGroup.");
				return false;
			}
			if (t.Id < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator grupy do metody ChangeFormatGroup.");
				return false;
			}


			ErrorType ret;
			Dictionary<string, SqlParameter> pars = t.CreateSqlParameters(	"Id",
																			"MaterialId",
																			"SubtitleId",
																			"SourceId",
																			"AudioId");

			foreach (var de in pars)
				de.Value.Direction = ParameterDirection.Input;

			SqlParameter[] ps = pars.Values.ToArray();
			ExecuteNonQuery("dbo.ChangeFormatGroup", ps, out ret);

			if (ret == ErrorType.Success)
				return true;
			else if (ret == ErrorType.MaterialNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje materiał o Id={0} przekazany do metody AddFormatGroup.", t.MaterialId));
			else if (ret == ErrorType.SubtitleFormatNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje format przekazanydo metody jako SubtitleId={0}.", t.SubtitleId));
			else if (ret == ErrorType.FormatNotFound)
				OnErrorReport(ret, string.Format("Nie istnieje format przekazanydo metody jako SourceId={0}.", t.SourceId));
			else
				OnErrorReport(ErrorType.General, string.Format("Nie istnieje grupa o Id = {0}. Modyfikacja niemożliwa.", t.Id));
			return false;
		}

		public FormatGroup GetFormatGroup(int id_FormatGroup)
		{
			if (id_FormatGroup < 0)
			{
				OnErrorReport(ErrorType.InvalidParameter, "Przekazano niepoprawny identyfikator grupy do metody GetFormatGroup.");
				return null;
			}

			Dictionary<string, SqlParameter> pars = CreateSqlParameters<FormatGroup>(	"Id",
																						"MaterialId",
																						"SubtitleId",
																						"SourceId",
																						"AudioId",
																						"PublicId");

			foreach (var de in pars)
				if (de.Key == "Id")
					de.Value.Direction = ParameterDirection.Input;
				else
					de.Value.Direction = ParameterDirection.Output;
			pars["Id"].Value = id_FormatGroup;

			SqlParameter[] ps = pars.Values.ToArray();

			ErrorType ret;
			ExecuteNonQuery("dbo.GetFormatGroup", ps, out ret);

			if (ret == ErrorType.Success)
			{
				FormatGroup t = CreateObject<FormatGroup>(ps);
				return t;
			}
			else //if (ret == ErrorType.NotFound)
				OnErrorReport(ret, string.Format("Nie istnieje grupa formatów o Id={0}.", id_FormatGroup));

			return null;
		}

		public FormatGroup[] GetFormatGroups4Material(string publicId)
		{
			SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, publicId)
			};


			List<FormatGroup> lst = ExecuteReaderToList<FormatGroup>("dbo.GetFormatGroups4Material", pars);

			return lst.ToArray();
		}

        public FormatGroupExt[] GetExtFormatGroups4Material(string publicId)
        {
            SqlParameter[] pars = new SqlParameter[] 
			{ 
				CreateSqlParameter("PublicId", SqlDbType.VarChar, 150, publicId)
			};

            List<FormatGroupExt> lst = ExecuteReaderToList<FormatGroupExt>("dbo.GetExtFormatGroups4Material", pars);

            return lst.ToArray();
        }
    }
}
