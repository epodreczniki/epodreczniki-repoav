using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PSNC.RepoAV.Services.RepApi.Models
{
    public class SetFormatReq
    {
        [Required]
        public string materialId { get; set; }

        [Required]
        public string formatType { get; set; }


        [Required]
        public string formatURL { get; set; }
        
        [Required]
        public string mime { get; set; }
        
        public string userId { get; set; }
        
        public string formatMD5 { get; set; }
        
    }
}