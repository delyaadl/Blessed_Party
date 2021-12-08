using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class tbl_User
    {
        [Key]
        public int user_id { get; set; }

        public string user_fullname { get; set; }

        public string username { get; set; }

        public string userpass { get; set; }

        public string user_phone { get; set; }

        public string user_email { get; set; }

        public string user_address { get; set; }

        public int city_id { get; set; }

        public int province_id { get; set; }

        public string flag_admin { get; set; }
    }
}
