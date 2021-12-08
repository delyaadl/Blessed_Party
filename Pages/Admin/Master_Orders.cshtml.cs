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
    public class Master_OrdersModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public Master_OrdersModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Order> tbl_Order { get; set; }
        public IList<tbl_Shipment> tbl_Shipment { get; set; }

        [BindProperty]
        public tbl_Order tbl_Order_Edit { get; set; }

        [BindProperty]
        public tbl_Shipment shipment_Add { get; set; }

        [BindProperty]
        public tbl_Shipment shipment_Delete { get; set; }

        [BindProperty]
        public tbl_Order tbl_Order_Delete { get; set; }

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
            tbl_Order = await _context.tbl_Order.ToListAsync();
            tbl_Shipment = await _context.tbl_Shipment.ToListAsync();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            tbl_Order check1 = _context.tbl_Order.Where(x => x.order_id == tbl_Order_Edit.order_id).FirstOrDefault();

            if (int.Parse(tbl_Order_Edit.order_status) == 9)
            {
                if (check1.order_status != "0")
                {
                    TempData["Message"] = "Pesanan sudah dalam proses, tidak bisa dibatalkan.";
                    return RedirectToPage();
                }
                else
                {
                    tbl_Order res = _context.tbl_Order.Where(x => x.order_id == tbl_Order_Edit.order_id).FirstOrDefault();
                    if (res != null)
                    {
                        res.order_finish_date = DateTime.Now;
                        res.order_status = tbl_Order_Edit.order_status;
                        _context.Attach(res).State = EntityState.Modified;

                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Pesanan dibatalkan.";
                        return RedirectToPage();
                    }
                }
            }

            if (check1.order_status == "0")
            {
                TempData["Message"] = "Pesanan belum dibayar, tunggu pesanan dibayar baru lanjut proses selanjutnya.";
                return RedirectToPage();
            }

            if (check1.order_status == "4" || check1.order_status == "9")
            {
                TempData["Message"] = "Pesanan selesai / dibatalkan, tidak dapat mengubah data";
                return RedirectToPage();
            }


            if (int.Parse(tbl_Order_Edit.order_status) >= 2)
            {
                tbl_Shipment checkSP = _context.tbl_Shipment.Where(x => x.order_id == tbl_Order_Edit.order_id).FirstOrDefault();
                if (checkSP == null)
                {
                    if (shipment_Add.shipment_awb == null)
                    {
                        TempData["Message"] = "Silakan isi nomor resi untuk lanjut ke proses berikutnya.";
                        return RedirectToPage();
                    }
                    else
                    {
                        shipment_Add.order_id = tbl_Order_Edit.order_id;
                        shipment_Add.shipment_date = DateTime.Now;
                        _context.tbl_Shipment.Add(shipment_Add);
                    }
                }
                else
                {
                    if (shipment_Add.shipment_awb != null)
                    {
                        checkSP.shipment_awb = shipment_Add.shipment_awb;
                        shipment_Add.shipment_date = DateTime.Now;
                        _context.Attach(checkSP).State = EntityState.Modified;
                    }
                    else
                    {
                        TempData["Message"] = "Silakan isi nomor resi untuk lanjut ke proses berikutnya.";
                        return RedirectToPage();
                    }
                }

                tbl_Order res = _context.tbl_Order.Where(x => x.order_id == tbl_Order_Edit.order_id).FirstOrDefault();
                if (res != null)
                {
                    if (tbl_Order_Edit.order_status == "4" || tbl_Order_Edit.order_status == "9")
                    {
                        res.order_finish_date = DateTime.Now;
                    }

                    res.order_status = tbl_Order_Edit.order_status;
                    _context.Attach(res).State = EntityState.Modified;

                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Data berhasil diubah!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!tbl_OrderExists(tbl_Order_Edit.order_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["Message"] = "Data tidak berhasil diubah, silakan periksa kembali!";
                    }
                }
            }

            return RedirectToPage();
        }

        private bool tbl_OrderExists(int id)
        {
            return _context.tbl_Product.Any(e => e.product_id == id);
        }

        public async Task<IActionResult> OnPostDownloadAsync()
        {
            var path = Path.GetFullPath("./wwwroot/TemplateOrder.xlsx");
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
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var rowcount = worksheet.Dimension.Rows;
                        //for (int row = 2; row <= rowcount; row++)
                        //{
                        //    if ( worksheet.Cells[row, 2].Value == null)
                        //    {
                        //        TempData["Message"] = "Import data dibatalkan, harap isi semua detail pesanan yang kosong!";
                        //        return RedirectToPage();
                        //    }
                        //}

                        for (int row = 2; row <= rowcount; row++)
                        {
                            tbl_Order newOrder = new tbl_Order();
                            newOrder.order_date = DateTime.Now;
                            newOrder.order_finish_date = DateTime.Now;
                            newOrder.shipping_address = "imported";
                            newOrder.order_note = "imported";
                            newOrder.order_status = "4";
                            newOrder.user_id = 1007;

                            _context.tbl_Order.Add(newOrder);
                            await _context.SaveChangesAsync();
                            int order_id = newOrder.order_id;
                            int temp_id = 0;
                            decimal totalprice = 0;

                            if (worksheet.Cells[row, 2].Value != null)
                            {
                                string[] dataProd = worksheet.Cells[row, 2].Value.ToString().Split(", ");
                                foreach (var item in dataProd)
                                {
                                    tbl_dtl_Order tbl_dtl_Order_Excel = new tbl_dtl_Order();
                                    tbl_Product res = _context.tbl_Product.Where(x => item.ToLower().Contains(x.product_name.ToLower())).FirstOrDefault();
                                    if (res != null)
                                    {
                                        if (res.product_id == temp_id)
                                        {
                                            tbl_dtl_Order last = _context.tbl_dtl_Order.Where(x => x.product_id == temp_id && x.order_id == order_id).FirstOrDefault();

                                            last.quantity = last.quantity + 1;
                                            _context.Attach(last).State = EntityState.Modified;
                                            await _context.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            tbl_dtl_Order_Excel.product_id = res.product_id;
                                            tbl_dtl_Order_Excel.order_id = order_id;
                                            tbl_dtl_Order_Excel.price = res.price;
                                            tbl_dtl_Order_Excel.quantity = 1;

                                            _context.tbl_dtl_Order.Add(tbl_dtl_Order_Excel);
                                            await _context.SaveChangesAsync();
                                        }
                                        temp_id = res.product_id;
                                        totalprice = totalprice + res.price;
                                    }
                                }

                                tbl_Order addAmount = _context.tbl_Order.Where(x => x.order_id == order_id).FirstOrDefault();
                                addAmount.amount = totalprice;
                                _context.Attach(addAmount).State = EntityState.Modified;
                                await _context.SaveChangesAsync();
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
