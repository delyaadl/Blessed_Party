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
using OfficeOpenXml;

namespace Blessed_Party.Pages.Admin
{
    public class Master_RatingProductsModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public Master_RatingProductsModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Rating_Product> tbl_Rating_Product { get; set; }

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
            tbl_User = await _context.tbl_User.ToListAsync();
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_Rating_Product = await _context.tbl_Rating_Product.ToListAsync();
        }

        public async Task<IActionResult> OnPostDownloadAsync()
        {
            var path = Path.GetFullPath("./wwwroot/TemplateRating.xlsx");
            MemoryStream memory = new MemoryStream();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(path));

            ////Read the File data into Byte Array.
            //byte[] bytes = System.IO.File.ReadAllBytes(path);

            ////Send the File to Download.
            //return File(bytes, "application/octet-stream", "Template Risk Identification.xlsx");
        }

        public async Task<IActionResult> OnPostUploadFile(IFormFile fileSelect)
        {
            try
            {

                //var list = new List<Mst_Risk_Identification>();

                using (var stream = new MemoryStream())
                {
                    await fileSelect.CopyToAsync(stream);
                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var rowcount = worksheet.Dimension.Rows;
                        for (int row = 2; row <= rowcount; row++)
                        {
                            if (worksheet.Cells[row, 1].Value == null || worksheet.Cells[row, 2].Value == null || worksheet.Cells[row, 3].Value == null)
                            {
                                TempData["Message"] = "Import data dibatalkan, harap isi semua data kosong!";
                                return RedirectToPage();
                            }
                        }

                        for (int row = 2; row <= rowcount; row++)
                        {
                            tbl_Rating_Product tbl_Rating_Product_Excel = new tbl_Rating_Product();
                            tbl_User user_id = _context.tbl_User.Where(x => x.username == worksheet.Cells[row, 1].Value.ToString().Trim()).FirstOrDefault();
                            int userid = 0;

                            if(user_id == null)
                            {
                                tbl_User addnew = new tbl_User();
                                addnew.username = worksheet.Cells[row, 1].Value.ToString().Trim();
                                addnew.userpass = "c724c5c2c1859202176871bd1079865276c3570a5891a00cf7635a4688ede597";
                                addnew.user_address = "dummy";
                                addnew.user_email = "dummy@gmail.com";
                                addnew.user_fullname = worksheet.Cells[row, 1].Value.ToString().Trim();
                                addnew.user_phone = "123456";
                                addnew.province_id = 1;
                                addnew.city_id = 1;
                                addnew.flag_admin = "N";

                                _context.tbl_User.Add(addnew);
                                await _context.SaveChangesAsync();
                                userid = addnew.user_id;
                            } else
                            {
                                userid = user_id.user_id;
                            }

                            tbl_Rating_Product_Excel.user_id = userid;
                            tbl_Rating_Product_Excel.product_id = int.Parse(worksheet.Cells[row, 2].Value.ToString().Trim());
                            tbl_Rating_Product_Excel.rating_number = int.Parse(worksheet.Cells[row, 3].Value.ToString().Trim());


                            tbl_Rating_Product = await _context.tbl_Rating_Product.Where(c => c.user_id == tbl_Rating_Product_Excel.user_id && c.product_id == tbl_Rating_Product_Excel.product_id).ToListAsync();

                            if (tbl_Rating_Product.Count == 0)
                            {
                                _context.tbl_Rating_Product.Add(tbl_Rating_Product_Excel);
                                await _context.SaveChangesAsync();
                                TempData["Message"] = "Data berhasil diimport";
                            }
                            else
                            {
                                if (row == rowcount - 1)
                                {
                                    TempData["Message"] = "Tidak ada data baru!";
                                }
                            }


                        }
                    }
                }
                return RedirectToPage();
            }

            catch (Exception ex)
            {
                string a = ex.Message;
                return RedirectToPage();
                // throw new FileFormatException($"Error on file processing - {ex.Message}");

            }

        }
    }
}
