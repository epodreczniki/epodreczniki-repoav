using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PSNC.RepoAV.Services.RepApi.Models
{
    public class SetMaterialReq
    {
        [Required]
        public string materialId { get; set; }
        [Required]
        public string materialURL { get; set; }
        [Required]
        public string mime { get; set; }
        public string userId { get; set; }
        public string materialMD5 { get; set; }
        public string metadata { get; set; }
    }
}