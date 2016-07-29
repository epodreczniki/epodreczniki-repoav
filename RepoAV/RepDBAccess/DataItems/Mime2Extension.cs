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
	public class Mime2Extension : BaseObject
	{
		public string Mime {get; set;}

		public string FileExtension {get; set;}

		public Mime2Extension()
			:base()
		{
		}
	}
}
