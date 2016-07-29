using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.Common;

namespace PSNC.RepoAV.DBAccess
{
	public class DBAccessException : ApplicationException
	{
		public ErrorType Error;

		public DBAccessException(string message)
			: this(0, message)
		{ }

		public DBAccessException(ErrorType errorCode, string message)
			: this(null, errorCode, message)
		{ }

		public DBAccessException(Exception ex, string message)
			: this(ex, 0, message)
		{ }

		public DBAccessException(Exception ex)
			: this(null, 0, ex != null ? ex.Message : null)
		{ }

		public DBAccessException(Exception ex, ErrorType errorCode, string message)
			: base(message, ex)
		{
			Error = errorCode;
		}
	}
}
