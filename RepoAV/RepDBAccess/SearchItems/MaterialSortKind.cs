using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.RepDBAccess
{
	public enum MaterialSortKind : short
	{
		TitleASC = 0,
		TitleDESC = 1,
		PublicIdASC = 2,
		PublicIdDESC = 3,
		DurationASC = 4,
		DurationDESC = 5,
		CreatedDateASC = 6,
		CreatedDateDESC = 7,
		ModifyDateASC = 8,
		ModifyDateDESC = 9,
		FormatGroupsASC = 10,
		FormatGroupsDESC = 11,
		MaterialTypeASC = 12,
		MaterialTypeDESC = 13
	}
}
