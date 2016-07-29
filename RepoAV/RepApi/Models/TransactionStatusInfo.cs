using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PSNC.RepoAV.Services.RepApi.Models
{
    public class TransactionStatusInfo
    {
        public string materialId { get; set; }
        public string status { get; set; }
        public string description { get; set; }
    }
}