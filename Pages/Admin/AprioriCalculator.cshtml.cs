using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blessed_Party.Pages.Admin
{
    public class AprioriCalculatorModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public AprioriCalculatorModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<apriori_input_tbl> apriori_input_tbl { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.User.FindFirst("fAdmin")?.Value == null || HttpContext.User.FindFirst("fAdmin")?.Value == "N")
            {
                return denyAccess();
            }
            else
            {
                return Page();
            }
        }

        public IActionResult denyAccess()
        {
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostSetAprioriAsync(decimal minsup, decimal minconf, DateTime? start_date, DateTime? end_date)
        {
            try
            {
                if(start_date != null && end_date != null)
                {
                    DateTime firstDate = DateTime.Parse(start_date.ToString());
                    DateTime secondDate = DateTime.Parse(end_date.ToString());
                    double diff2 = (firstDate - secondDate).TotalDays;

                    if (diff2 > 0)
                    {
                        TempData["Message"] = "Tanggal terakhir transaksi yang dipilih tidak boleh lebih kecil dari tanggal mulai!";
                        return RedirectToPage();
                    }
                }

                _context.apriori_input_tbl.RemoveRange(_context.apriori_input_tbl);
                await _context.SaveChangesAsync();

                apriori_input_tbl addNew = new apriori_input_tbl();
                addNew.minimum_support = minsup;
                addNew.minimum_confidence = minconf;
                addNew.start_date = start_date;
                addNew.end_date = end_date;

                _context.apriori_input_tbl.Add(addNew);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Input telah dimasukkan, silakan periksa home untuk hasil.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                return RedirectToPage();
            }
            
        }
    }
}
