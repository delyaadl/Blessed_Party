using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class tbl_cart
    {
        [Key]
        public int cart_id { get; set; }

        public int user_id { get; set; }

        public int product_id { get; set; }

        public int quantity { get; set; }
    }
}
