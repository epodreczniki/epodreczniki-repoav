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
	public class Task : TaskShort
	{
		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = -1)]
		public string Result {get; set;}

		[SqlParameter]
		public DateTime CreatedDate {get; set;}

		[SqlParameter]
		public DateTime TakenDate {get; set;}

		[SqlParameter]
		public DateTime FinishDate { get; set; }

		[SqlParameter]
		public int? ExecutingNodeId { get; set; }

		[SqlParameter]
		public int? SupervisorId { get; set; }

		[SqlParameter]
		public DateTime BeginDate { get; set; }

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150, ArrayElementsSeparator=";")]
		public int[] PreferredNodeIds { get; set; }

		[SqlParameter]
		public bool CanSkipPreferredNodes { get; set; }

		
		public Task()
			:base()
		{			
			TakenDate = DateTime.MinValue;
			CreatedDate = DateTime.MinValue;
			FinishDate = DateTime.MinValue;
			ExecutingNodeId = null;
			SupervisorId = null;
			PreferredNodeIds = null;
		}
	}
}
