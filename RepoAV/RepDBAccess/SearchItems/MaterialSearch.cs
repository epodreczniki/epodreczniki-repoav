using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class MaterialSearch : RangeSelector
	{
		[SqlParameter]
		public int DurationFrom {get; set;}

		[SqlParameter]
		public int DurationTo { get; set; }

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = 500)]
		public string Title {get; set;}

		[SqlParameter]
		public bool? AllowDistribution {get; set;}

		[SqlParameter(System.Data.SqlDbType.SmallInt)]
		public MaterialType? MaterialType {get; set;}

		[SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
		public string PublicId { get; set; }

		[SqlParameter]
		public DateTime CreatedDateFrom { get; set; }

		[SqlParameter]
		public DateTime CreatedDateTo { get; set; }

		[SqlParameter]
		public DateTime ModifyDateFrom { get; set; }

		[SqlParameter]
		public DateTime ModifyDateTo { get; set; }

		[SqlParameter]
		public MaterialSortKind SortOrder  { get; set; }

		[SqlParameter(System.Data.SqlDbType.SmallInt, TreatMinusOneAsNull=false)]
		public MaterialStatus? MaterialStatus { get; set; }

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = 150)]
		public string Tag { get; set; }		

		public MaterialSearch()
			: base()
		{
			DurationFrom = -1;
			DurationTo = -1;
			AllowDistribution = null;
			MaterialType = null;
			CreatedDateFrom = DateTime.MinValue;
			CreatedDateTo = DateTime.MinValue;
			ModifyDateFrom = DateTime.MinValue;
			ModifyDateTo = DateTime.MinValue;
			SortOrder = MaterialSortKind.TitleASC;
			MaterialStatus = null;
		}

		public MaterialSearch(int offset, int count)
			: base(offset, count)
		{
			DurationFrom = -1;
			DurationTo = -1;
			AllowDistribution = null;
			MaterialType = null;
			CreatedDateFrom = DateTime.MinValue;
			CreatedDateTo = DateTime.MinValue;
			ModifyDateFrom = DateTime.MinValue;
			ModifyDateTo = DateTime.MinValue;
			SortOrder = MaterialSortKind.TitleASC;
		}
	}
}
