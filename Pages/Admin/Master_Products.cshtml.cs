using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blessed_Party.Pages.Admin
{
    public class Master_ProductsModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public Master_ProductsModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Category> tbl_Category { get; set; }
        public IList<tbl_Rating_Product> tbl_Rating_Product { get; set; }
        public IList<tbl_cart> tbl_cart { get; set; }
        public IList<tbl_dtl_Order> tbl_dtl_Order { get; set; }

        [BindProperty]
        public tbl_Product tbl_Product_Add { get; set; }

        [BindProperty]
        public tbl_Product tbl_Product_Edit { get; set; }

        [BindProperty]
        public tbl_Product tbl_Product_Delete { get; set; }

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
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_Category = await _context.tbl_Category.ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(IFormFile FotoProduk)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            tbl_Product = await _context.tbl_Product.Where(e => e.product_id == tbl_Product_Add.product_id).ToListAsync();

            if (tbl_Product.Count > 0)
            {
                TempData["Message"] = "product_id tidak dapat diduplikasi!";
            }
            else
            {
                tbl_Product_Add.created_date = DateTime.Now;
                using (var ms = new MemoryStream())
                {
                    await FotoProduk.CopyToAsync(ms);
                    string Filename = FotoProduk.FileName;
                    byte[] Bytes1 = ms.ToArray();

                    tbl_Product_Add.product_image = Bytes1;
                }
                _context.tbl_Product.Add(tbl_Product_Add);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Data berhasil ditambahkan!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(IFormFile FotoProdukEdit)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            tbl_Product res = _context.tbl_Product.Where(x => x.product_id == tbl_Product_Edit.product_id).FirstOrDefault();
            if (res != null)
            {
                if (res.product_image == null)
                {
                    TempData["Message"] = "Belum ada gambar yang di-upload";
                }
                else
                {
                    _context.Entry(res).State = EntityState.Detached;

                    if(FotoProdukEdit == null)
                    {
                        _context.Attach(tbl_Product_Edit).State = EntityState.Modified;
                        _context.Entry(tbl_Product_Edit).Property(x => x.product_image).IsModified = false;
                    } else
                    {
                        using (var ms = new MemoryStream())
                        {
                            await FotoProdukEdit.CopyToAsync(ms);
                            string Filename = FotoProdukEdit.FileName;
                            byte[] Bytes1 = ms.ToArray();

                            tbl_Product_Edit.product_image = Bytes1;
                        }

                        _context.Attach(tbl_Product_Edit).State = EntityState.Modified;
                        _context.Entry(tbl_Product_Edit).Property(x => x.product_image).IsModified = true;
                    }
                }
            }


            try
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = "Data berhasil diubah!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!tbl_ProductExists(tbl_Product_Edit.product_id))
                {
                    return NotFound();
                }
                else
                {
                    TempData["Message"] = "Data tidak berhasil diubah, silakan periksa kembali!";
                }
            }

            return RedirectToPage();
        }

        private bool tbl_ProductExists(int id)
        {
            return _context.tbl_Product.Any(e => e.product_id == id);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int product_id)
        {
            tbl_dtl_Order = await _context.tbl_dtl_Order.Where(x => x.product_id == product_id).ToListAsync();
            tbl_Rating_Product = await _context.tbl_Rating_Product.Where(x => x.product_id == product_id).ToListAsync();
            tbl_cart = await _context.tbl_cart.Where(x => x.product_id == product_id).ToListAsync();
            tbl_Product_Delete = await _context.tbl_Product.FindAsync(product_id);

            foreach (var item in tbl_cart)
            {
                tbl_cart deleteDtl = _context.tbl_cart.Where(x => x.product_id == item.product_id).FirstOrDefault();
                _context.tbl_cart.Remove(deleteDtl);
                await _context.SaveChangesAsync();
            }

            foreach (var item in tbl_Rating_Product)
            {
                tbl_Rating_Product deleteDtl = _context.tbl_Rating_Product.Where(x => x.product_id == item.product_id).FirstOrDefault();
                _context.tbl_Rating_Product.Remove(deleteDtl);
                await _context.SaveChangesAsync();
            }

            foreach (var item in tbl_dtl_Order)
            {
                tbl_dtl_Order deleteDtl = _context.tbl_dtl_Order.Where(x => x.dtl_order_id == item.dtl_order_id).FirstOrDefault();
                _context.tbl_dtl_Order.Remove(deleteDtl);
                await _context.SaveChangesAsync();
            }

            if (tbl_Product_Delete != null)
            {
                _context.tbl_Product.Remove(tbl_Product_Delete);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Data berhasil dihapus!";
            }

            return RedirectToPage();
        }
    }
}
