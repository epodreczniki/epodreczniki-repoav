using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlTypes;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	[Serializable]
	public class ProfileGroup: BaseObject
	{
		[SqlParameter]
		public int Id {get; set;}

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = 200)]
		public string Name {get; set;}

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = -1)]
		public string OperationXML { get; set; }

		[SqlParameter]
		public bool Enabled { get; set; }

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public MaterialType MaterialType { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 50)]
		public string TaskSubtype { get; set; }

        [SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
        public string DownloadSourceFiles { get; set; }

		public ProfileGroup()
			:base()
		{
			Id = -1;
			Enabled = true;
			MaterialType = RepoAV.RepDBAccess.MaterialType.Unknown;
		}
	}

	[Serializable]
	public class ProfileGroupExt : ProfileGroup
	{
		public List<Profile> Profiles;

		public ProfileGroupExt()
			: base()
		{
			Profiles = new List<Profile>();
		}
	}
}
