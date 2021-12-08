using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class tbl_Shipment
    {
        [Key]
        public int shipment_id { get; set; }

        public int order_id { get; set; }

        public int shipment_weight { get; set; }

        public string shipment_type { get; set; }

        public decimal shipment_cost { get; set; }

        public string shipment_awb { get; set; }

        public DateTime? shipment_date { get; set; }
    }
}
