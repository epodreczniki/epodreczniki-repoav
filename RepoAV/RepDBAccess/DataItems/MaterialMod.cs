using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class MaterialMod : BaseObject
	{
		[SqlParameter]
		public int Id {get; set;}

		[SqlParameter]
		public int? Duration {get; set;}

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = 500)]
		public string Title {get; set;}

		[SqlParameter]
		public bool AllowDistribution {get; set;}

        [SqlParameter]
        public bool Deleted { get; set; }

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public MaterialType MaterialType {get; set;}

		[SqlParameter(System.Data.SqlDbType.Xml)]
		public System.Data.SqlTypes.SqlXml Metadata { get; set; }

		public MaterialMod()
			:base()
		{
			Id = -1;
			Duration = null;
			MaterialType = RepoAV.RepDBAccess.MaterialType.Unknown;
			Metadata = null;
		}
	}
}
