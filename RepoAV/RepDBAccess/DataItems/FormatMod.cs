using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class FormatMod : BaseObject
	{
		[SqlParameter]
		public int Id {get; set;}

		[SqlParameter]
		public int FormatGroupId {get; set;}

		[SqlParameter]
		public int? ProfileId {get; set;}

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = -1)]
		public string XmlMetadata {get; set;}

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public FormatType Type {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string UniqueId {get; set;}

		[SqlParameter]
		public long Size {get; set;}

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public FormatInternalStatus InternalStatus { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 50)]
		public string Mime { get; set; }

		public FormatMod()
			:base()
		{
			Id = -1;
			FormatGroupId = -1;
			ProfileId = null;
			Type = FormatType.Source;
			InternalStatus = FormatInternalStatus.Adding;
		}
	}
}
