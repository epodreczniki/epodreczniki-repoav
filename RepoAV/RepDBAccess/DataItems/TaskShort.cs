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
	public class TaskShort : BaseObject
	{
		[SqlParameter]
		public long Id {get; internal set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string UniqueId {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string PublicId {get; set;}

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public TaskType Type {get; set;}

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public TaskStatus Status {get; set;}

		[SqlParameter]
		public bool ResultProcessed { get; set; }


		public Dictionary<string, string> Content
		{
			get { return m_Content; }
			set { m_Content = value; }
		}

		Dictionary<string, string> m_Content;

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 50)]
		public string TaskSubtype { get; set; }


		public TaskShort()
			:base()
		{
			m_Content = new Dictionary<string, string>();
			Status = TaskStatus.New;
			Type = TaskType.Unknown;
			Id = -1;
		}
	}
}
