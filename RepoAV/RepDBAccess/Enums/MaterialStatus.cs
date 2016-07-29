using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.RepDBAccess
{
	public enum MaterialStatus : short
	{
		Invalid = 1,
		NotFound = 2,
		RecError = 3,
		AddError = 4,
		RemovePending = 5,
		Recoding = 6,
		Adding = 7,
		Added = 8,
		Recoded = 10,
        NotAvailable = 11
	}
}
