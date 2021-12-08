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
    public class Master_ShipmentsModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public Master_ShipmentsModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_Shipment> tbl_Shipment { get; set; }


        public async Task OnGetAsync()
        {
            if (HttpContext.User.FindFirst("fAdmin")?.Value == null || HttpContext.User.FindFirst("fAdmin")?.Value == "N")
            {
                denyAccess();
            }
            await LoadAll();
        }

        public IActionResult denyAccess()
        {
            return RedirectToPage("/Index");
        }

        async Task LoadAll()
        {
            tbl_Shipment = await _context.tbl_Shipment.ToListAsync();
        }
    }
}
