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
    public class Master_CategoriesModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public Master_CategoriesModel (Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_Category> tbl_Category { get; set; }

        [BindProperty]
        public tbl_Category tbl_Category_Add { get; set; }

        [BindProperty]
        public tbl_Category tbl_Category_Edit { get; set; }

        [BindProperty]
        public tbl_Category tbl_Category_Delete { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.User.FindFirst("fAdmin")?.Value == null || HttpContext.User.FindFirst("fAdmin")?.Value == "N")
            {
                return denyAccess();
            }
            else
            {
                await LoadAll();
                return Page();
            }
        }

        public IActionResult denyAccess()
        {
            return RedirectToPage("/Index");
        }

        async Task LoadAll()
        {
            tbl_Category = await _context.tbl_Category.ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            tbl_Category_Add.created_date = DateTime.Now;
            
            _context.tbl_Category.Add(tbl_Category_Add);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Data berhasil ditambahkan!";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            tbl_Category res = _context.tbl_Category.Where(x => x.category_id == tbl_Category_Edit.category_id).FirstOrDefault();

            _context.Entry(res).State = EntityState.Detached;

            _context.Attach(tbl_Category_Edit).State = EntityState.Modified;
            _context.Entry(tbl_Category_Edit).Property(x => x.category_id).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = "Data berhasil diubah!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!tbl_CategoryExists(tbl_Category_Edit.category_id))
                {
                    return NotFound();
                }
                else
                {
                    TempData["Message"] = "Data tidak berhasil diubah, silakan periksa kembali!";
                }
            }

            tbl_Category = await _context.tbl_Category.ToListAsync();

            return RedirectToPage();
        }

        private bool tbl_CategoryExists(int id)
        {
            return _context.tbl_Category.Any(e => e.category_id == id);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int category_id)
        {
           
            tbl_Category_Delete = await _context.tbl_Category.FindAsync(category_id);

            if (tbl_Category_Delete != null)
            {
                _context.tbl_Category.Remove(tbl_Category_Delete);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Data berhasil dihapus!";
            }

            return RedirectToPage();
        }
    }
}
