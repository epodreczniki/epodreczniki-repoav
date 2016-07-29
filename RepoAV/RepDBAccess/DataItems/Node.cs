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
	public class Node: BaseObject
	{
		[SqlParameter]
		public int Id {get; internal set;}

		[SqlParameter]
		public NodeRole Role {get; internal set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 50)]
		public string Name { get; internal set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 500)]
		public string ExternalAddress {get; internal set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 500)]
		public string InternalAddress {get; internal set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 500)]
		public string Url {get; internal set;}

		[SqlParameter]
		public bool Enabled { get; internal set; }

		[SqlParameter]
		public bool IsOnline {get; internal set;}

		[SqlParameter]
		public long? FreeSpace { get; internal set; } //w MB

		[SqlParameter]
		public long? TotalSpace { get; internal set; } //w MB

        [SqlParameter(System.Data.SqlDbType.Int)]
        public int ProcaPortNumber { get; set; }

		public Node()
			:base()
		{
			IsOnline = false;
			Role = NodeRole.Manager;
			Id = -1;
			FreeSpace = null;
			Enabled = true;
		}
	}
}
