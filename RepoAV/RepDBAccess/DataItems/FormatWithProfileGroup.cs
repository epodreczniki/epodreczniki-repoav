using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class FormatWithProfileGroup : Format
	{
		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public int? Id_ProfileGroup {get; set;}

		public FormatWithProfileGroup()
			:base()
		{
			Id_ProfileGroup = null;
		}
	}
}
