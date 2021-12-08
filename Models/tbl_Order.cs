using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class tbl_Order
    {
        [Key]
        public int order_id { get; set; }

        public int user_id { get; set; }

        public decimal amount { get; set; }

        public string order_status { get; set; }

        public string order_note { get; set; }

        public string shipping_address { get; set; }

        public byte[] proof_of_payment { get; set; }

        public DateTime order_date { get; set; }
        public DateTime? order_finish_date { get; set; }
    }
}
