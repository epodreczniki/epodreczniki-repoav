using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class MaterialAdd : MaterialMod
	{
		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string PublicId {get; set;}

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = -1, ArrayElementsSeparator=";")]
		public string[] Tags { get; set; }

		public MaterialAdd()
			:base()
		{
		}
	}
}
