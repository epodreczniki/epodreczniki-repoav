using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class Material : MaterialAdd
	{
		[SqlParameter]
		public DateTime CreatedDate {get; set;}

		[SqlParameter]
		public DateTime ModifyDate { get; set; }

		[SqlParameter]
		public int FormatGroupsCount { get; set; }

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public MaterialStatus Status { get; set; }

		public Material()
			:base()
		{
		}
	}
}
