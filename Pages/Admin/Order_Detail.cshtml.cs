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
    public class Order_DetailModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public Order_DetailModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Order> tbl_Order { get; set; }
        public IList<tbl_dtl_Order> tbl_dtl_Order { get; set; }

        [BindProperty]
        public tbl_Product tbl_Product_Add { get; set; }

        [BindProperty]
        public tbl_Product tbl_Product_Edit { get; set; }

        [BindProperty]
        public tbl_Product tbl_Product_Delete { get; set; }

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
            TempData["order_id"] = order_id;
            tbl_dtl_Order = await _context.tbl_dtl_Order.Where(x => x.order_id == order_id).ToListAsync();
            tbl_Product = await _context.tbl_Product.ToListAsync();
        }
    }
}
