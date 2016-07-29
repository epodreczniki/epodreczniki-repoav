using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
    public class FormatGroupExt : FormatGroup
    {
        [SqlParameter(System.Data.SqlDbType.VarChar, MaxLength = 150)]
        public string Name { get; set; }
        
        public FormatGroupExt()
            : base()
		{
		}
    }
}
