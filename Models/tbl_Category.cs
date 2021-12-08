using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Models
{
    public class tbl_Category
    {
        [Key]
        public int category_id { get; set; }
        public string category_name { get; set; }
        public DateTime? created_date { get; set; }
    }
}
