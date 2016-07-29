using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PSNC.RepoAV.Services.RepApi.Models
{
    public class SetMaterialDistributionReq
    {
        [Required]
        public string materialId { get; set; }
        [Required]
        public string enable { get; set; }
    }
}