using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlTypes;
using PSNC.RepoAV.DBAccess;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.MaterialFormatDBAccess
{
	[Serializable]
	public class Response2Send : BaseObject
	{
		[SqlParameter]
		public long TaskId {get; internal set;}

		[SqlParameter]
		public ErrorType ErrorCode {get; internal set;}

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = -1)]
		public string Result {get; set;}

		[SqlParameter]
		public DateTime TaskFinishDate {get; set;}


		public Response2Send()
			:base()
		{
			ErrorCode = ErrorType.Runtime;
			TaskId = -1;
		}
	}
}
