using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class ForgetPassword
    {
        [Key]
        public int fp_id { get; set; }
        public int user_id { get; set; }
        public string token { get; set; }
        public DateTime lastupdate_date { get; set; }
    }
}
