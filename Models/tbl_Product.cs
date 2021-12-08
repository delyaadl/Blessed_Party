using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class tbl_Product
    {
        [Key]
        public int product_id { get; set; }

        public int? category_id { get; set; }

        public string product_name { get; set; }

        public string description { get; set; }

        public int product_weight { get; set; }

        public decimal price { get; set; }

        public byte[] product_image { get; set; }

        public DateTime created_date { get; set; }

        public string flag_available { get; set; }

    }
}
