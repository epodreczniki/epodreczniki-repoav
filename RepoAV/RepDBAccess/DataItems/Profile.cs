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
	public class Profile: BaseObject
	{
		[SqlParameter]
		public int Id {get; set;}

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = 200)]
		public string Name {get; set;}

		[SqlParameter]
		public int? MinHeight { get; set; }

		[SqlParameter]
		public int? MinWidth { get; set; }

		[SqlParameter(System.Data.SqlDbType.Real)]
		public float? Apect { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 50)]
		public string Mime { get; set; }

		[SqlParameter]
		public int Id_ProfileGroup { get; set; }

		[SqlParameter]
		public bool Enabled { get; internal set; }


		public Profile()
			:base()
		{
			MinHeight = null;
			MinWidth = null;
			Apect = null;
			Id = -1;
			Id_ProfileGroup = -1;
			Enabled = true;
		}
	}
}
