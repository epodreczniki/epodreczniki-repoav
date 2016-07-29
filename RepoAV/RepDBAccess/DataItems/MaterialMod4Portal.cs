using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
    public class MaterialMod4Portal : BaseObject
    {
        [SqlParameter]
        public int Id { get; set; }

        [SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = 500)]
        public string Title { get; set; }

		[SqlParameter(System.Data.SqlDbType.NVarChar, MaxLength = 500)]
		public string Tag { get; set; }

		public MaterialMod4Portal()
			:base()
		{
			Id = -1;
		}
    }
}
