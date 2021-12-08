using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class tbl_Prediction
    {
        [Key]
        public int cf_id { get; set; }

        public int product_id { get; set; }

        public int user_id { get; set; }

        public decimal? prediction_score { get; set; }

        public DateTime created_date { get; set; }

    }
}
