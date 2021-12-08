using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blessed_Party.Pages
{
    public class HistoryOrderModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public HistoryOrderModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Order> tbl_Order { get; set; }
        public IList<tbl_dtl_Order> tbl_dtl_Order { get; set; }
        public IList<tbl_Shipment> tbl_Shipment { get; set; }

        public async Task OnGetAsync(int order_id)
        {
            await LoadAll(order_id);
        }


        async Task LoadAll(int order_id)
        {
            int userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
            tbl_Order = await _context.tbl_Order.Where(x => x.user_id == userid).ToListAsync();
            tbl_dtl_Order = await _context.tbl_dtl_Order.ToListAsync();
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_Shipment = await _context.tbl_Shipment.ToListAsync();

        }
    }
}
