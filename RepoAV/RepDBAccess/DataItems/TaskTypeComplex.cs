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
	public class TaskTypeComplex : BaseObject
	{
		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public TaskType Type {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 50)]
		public string TaskSubtype { get; set; }


		public TaskTypeComplex()
			:base()
		{
			TaskSubtype = null;
			Type = TaskType.Unknown;
		}
	}
}
