using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	[Serializable]
	public class TaskAdd : BaseObject
	{
		[SqlParameter]
		public long Id {get; internal set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string UniqueId { get; set; }		

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string PublicId {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150, ArrayElementsSeparator = ";")]
		public int[] PreferredNodeIds { get; set; }

		[SqlParameter]
		public bool CanSkipPreferredNodes { get; set; }

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public TaskType Type {get; set;}

		[SqlParameter]
		public int SupervisorId {get; set;}

		[SqlParameter]
		public DateTime BeginDate {get; set;}

		public Dictionary<string, string> Content 
		{
			get { return m_Content; } 
		}
		
		Dictionary<string, string> m_Content;

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 50)]
		public string TaskSubtype { get; set; }

		public TaskAdd()
			:base()
		{
			BeginDate = DateTime.MinValue;
			m_Content = new Dictionary<string, string>();
			Id = -1;
			PreferredNodeIds = null;
			UniqueId = null;
			Type = TaskType.AddMaterial;
			SupervisorId = -1;
			CanSkipPreferredNodes = false;
		}
	}
}
