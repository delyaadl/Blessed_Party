using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Blessed_Party.Models;

namespace Blessed_Party.Data
{
    public class BPartyContext : DbContext
    {
        public BPartyContext(DbContextOptions<BPartyContext> options)
            : base(options)
        {
        }

        public DbSet<tbl_User> tbl_User { get; set; }
        public DbSet<tbl_Product> tbl_Product { get; set; }
        public DbSet<tbl_Category> tbl_Category { get; set; }
        public DbSet<tbl_cart> tbl_cart { get; set; }
        public DbSet<tbl_dtl_Order> tbl_dtl_Order { get; set; }
        public DbSet<tbl_Order> tbl_Order { get; set; }
        public DbSet<tbl_Rating_Product> tbl_Rating_Product { get; set; }
        public DbSet<tbl_Rating_40_User> tbl_Rating_40_User { get; set; }
        public DbSet<tbl_Rating_80_User> tbl_Rating_80_User { get; set; }
        public DbSet<tbl_Shipment> tbl_Shipment { get; set; }
        public DbSet<tbl_Prediction> tbl_Prediction { get; set; }
        public DbSet<tbl_Prediction40> tbl_Prediction40 { get; set; }
        public DbSet<tbl_Prediction80> tbl_Prediction80 { get; set; }
    }
}
