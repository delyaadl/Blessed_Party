using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class apriori_input_tbl
    {
        [Key]
        public int apriori_id { get; set; }

        public decimal? minimum_support { get; set; }

        public decimal? minimum_confidence { get; set; }

        public DateTime? start_date { get; set; }

        public DateTime? end_date { get; set; }
    }
}
