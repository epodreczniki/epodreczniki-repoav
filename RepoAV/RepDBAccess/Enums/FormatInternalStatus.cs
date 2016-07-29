using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.RepDBAccess
{
	public enum FormatInternalStatus : short
	{
		Adding = 0,
		AddError = 1,
		Recoding = 2,
		RecError = 3,
		RemovePending = 4,
		Added = 5,
        InvalidFile = 6,
        NotFound = 7,
        Recoded = 8
	}
}
