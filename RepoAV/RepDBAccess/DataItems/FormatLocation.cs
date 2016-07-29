using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class FormatLocation : BaseObject
	{
		[SqlParameter]
		public int NodeId {get; set;}

		[SqlParameter]
		public string NodeName {get; set;}

        [SqlParameter]
        public bool IsOnLine { get; set; }

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public NodeRole Role {get; set;}


		public FormatLocation()
			:base()
		{
			Role = NodeRole.Other;
			NodeId = -1;
            IsOnLine = false;
		}
	}
}
