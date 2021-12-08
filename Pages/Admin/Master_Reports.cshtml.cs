using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blessed_Party.Pages.Admin
{
    public class Master_ReportsModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public Master_ReportsModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Order> tbl_Order { get; set; }
        public IList<tbl_Shipment> tbl_Shipment { get; set; }
        public IList<reportsView> reportsViewList { get; set; }

        public class reportsView
        {
            public int order_id { get; set; }

            public int user_id { get; set; }

            public string username { get; set; }
            public string user_fullname { get; set; }
            public string phone { get; set; }
            public string products { get; set; }

            public int products_weight { get; set; }

            public string shipment_type { get; set; }

            public string shipment_awb { get; set; }

            public decimal total_price { get; set; }

            public string order_status { get; set; }

            public string order_note { get; set; }

            public string shipping_address { get; set; }

            public DateTime order_date { get; set; }

            public DateTime? order_finish_date { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int order_id)
        {
            if (HttpContext.User.FindFirst("fAdmin")?.Value == null || HttpContext.User.FindFirst("fAdmin")?.Value == "N")
            {
                return denyAccess();
            }
            else
            {
                await LoadAll(order_id);
                return Page();
            }
        }

        public IActionResult denyAccess()
        {
            return RedirectToPage("/Index");
        }

        async Task LoadAll(int order_id)
        {
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_User = await _context.tbl_User.ToListAsync();
            tbl_Order = await _context.tbl_Order.Where(x => x.order_status == "4" || x.order_status == "9").ToListAsync();
            tbl_Shipment = await _context.tbl_Shipment.ToListAsync();

            var result = (from ord in tbl_Order
                          join ship in tbl_Shipment on ord.order_id equals ship.order_id
                          select new reportsView
                          {
                              order_id = ord.order_id,
                              order_status = ord.order_status,
                              shipment_awb = ship.shipment_awb,
                              shipment_type = ship.shipment_type,
                              products = string.Join(',', tbl_Product.Where(x => _context.tbl_dtl_Order.Where(y => y.order_id == ord.order_id).Select(y => y.product_id).Contains(x.product_id)).Select(x => x.product_name).ToArray()),
                              products_weight = _context.tbl_Product.Where(x =>  _context.tbl_dtl_Order.Where(x => x.order_id == ord.order_id).Select(x => x.product_id).Contains(x.product_id)).Sum(x => x.product_weight),
                              total_price = _context.tbl_Product.Where(x => _context.tbl_dtl_Order.Where(x => x.order_id == ord.order_id).Select(x => x.product_id).Contains(x.product_id)).Sum(x => x.price),
                              order_note = ord.order_note,
                              user_id = ord.user_id,
                              username = (from usr in tbl_User where usr.user_id == ord.user_id select usr.username).FirstOrDefault(),
                              user_fullname = (from usr in tbl_User where usr.user_id == ord.user_id select usr.user_fullname).FirstOrDefault(),
                              phone = (from usr in tbl_User where usr.user_id == ord.user_id select usr.user_phone).FirstOrDefault(),
                              shipping_address = ord.shipping_address,
                              order_date = ord.order_date,
                              order_finish_date = ord.order_finish_date
                          }).ToList();

            reportsViewList = result; 
        }
    }
}
