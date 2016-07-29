using PSNC.RepoAV.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace PSNC.RepoAV.RepDBAccess
{
	public class Statistics : BaseObject
	{
        public string Name { get; set; }
        public int Count { get; set; }
        public string Time 
        {  
           get { return string.Format("{0:00}:{1:00}", Count/60, Count % 60);}
        }

        public Statistics()
			:base()
		{
            Name = string.Empty;
            Count = -1;
		}
	}
}
