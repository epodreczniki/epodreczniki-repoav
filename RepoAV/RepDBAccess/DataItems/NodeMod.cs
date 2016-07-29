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
	public class NodeMod: BaseObject
	{
		[SqlParameter]
		public int Id {get; set;}

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public NodeRole Role {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 500)]
		public string ExternalAddress {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 500)]
		public string InternalAddress {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 500)]
		public string Url {get; set;}

        [SqlParameter(System.Data.SqlDbType.Int)]
        public int ProcaPortNumber { get; set; }

		[SqlParameter()]
		public bool Enabled { get; set; }

		public NodeMod()
			:base()
		{
			Role = NodeRole.Manager;
			Id = -1;
			Enabled = true;
		}
	}
}
