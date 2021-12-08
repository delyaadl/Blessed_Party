using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class tbl_dtl_Order
    {
        [Key]
        public int dtl_order_id { get; set; }

        public int order_id { get; set; }

        public int product_id { get; set; }

        public int quantity { get; set; }

        public decimal price { get; set; }
    }
}
