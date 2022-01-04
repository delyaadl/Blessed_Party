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
using System.Net.Mail;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using System.Configuration;
using System.Net;

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

            if(int.Parse(tbl_Order_Edit.order_status) == 5)
            {
                if(tbl_Order_Edit.admin_note == null)
                {
                    TempData["Message"] = "Silakan tulis detail masalah terlebih dahulu.";
                    return RedirectToPage();
                } else
                {
                    tbl_Order res = _context.tbl_Order.Where(x => x.order_id == tbl_Order_Edit.order_id).FirstOrDefault();

                    if (res.order_status != "2" && res.order_status != "3" && res.order_status != "4" && res.order_status != "9")
                    {
                        res.order_status = tbl_Order_Edit.order_status;
                        res.admin_note = tbl_Order_Edit.admin_note;
                        _context.Attach(res).State = EntityState.Modified;
                        _context.Entry(res).Property(x => x.admin_note).IsModified = true;
                        _context.Entry(res).Property(x => x.order_status).IsModified = true;

                        await _context.SaveChangesAsync();
                        sendEmail(res.admin_note);
                        TempData["Message"] = "Informasi telah dikirim ke customer.";
                        return RedirectToPage();
                    } else
                    {
                        TempData["Message"] = "Pesanan sudah dalam proses pengiriman / selesai / dibatalkan.";
                        return RedirectToPage();
                    }
                }
            }

            if (int.Parse(tbl_Order_Edit.order_status) == 9)
            {
                if (check1.order_status != "0")
                {
                    TempData["Message"] = "Pesanan sudah dalam proses, tidak bisa dibatalkan.";
                    return RedirectToPage();
                }
                else
                {
                    tbl_Shipment ship = _context.tbl_Shipment.Where(x => x.order_id == tbl_Order_Edit.order_id).FirstOrDefault();

                    if (ship != null)
                    {
                        ship.shipment_cost = 0;
                        ship.shipment_type = "-";
                        ship.shipment_weight = 0;

                        _context.Attach(ship).State = EntityState.Modified;
                        _context.Entry(ship).Property(x => x.shipment_weight).IsModified = true;
                        _context.Entry(ship).Property(x => x.shipment_type).IsModified = true;
                        _context.Entry(ship).Property(x => x.shipment_cost).IsModified = true;

                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        tbl_Shipment newship = new tbl_Shipment();
                        newship.order_id = tbl_Order_Edit.order_id;
                        newship.shipment_cost = 0;
                        newship.shipment_type = "-";
                        newship.shipment_weight = 0;

                        _context.tbl_Shipment.Add(newship);
                        await _context.SaveChangesAsync();
                    }

                    tbl_Order res = _context.tbl_Order.Where(x => x.order_id == tbl_Order_Edit.order_id).FirstOrDefault();
                    if (res != null)
                    {
                        res.admin_note = "";
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

                    res.admin_note = "";
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

        protected void sendEmail(string textEmail)
        {
            try
            {
                //Fetching Settings from WEB.CONFIG file.  
                string emailSender = "theyale.id@gmail.com";
                string emailSenderPassword = "liacantik11";
                string emailSenderHost = "smtp.gmail.com";
                int emailSenderPort = 587;
                bool emailIsSSL = true;


                string subject = "Pesanan Anda Bermasalah.";

                //Base class for sending email  
                MailMessage _mailmsg = new MailMessage();

                //Make TRUE because our body text is html  
                _mailmsg.IsBodyHtml = true;

                //Set From Email ID  
                _mailmsg.From = new MailAddress(emailSender);

                //Set To Email ID  
                _mailmsg.To.Add("delyatjung@gmail.com");

                //Set Subject  
                _mailmsg.Subject = subject;

                string bodyMess = "<p>Pesanan anda bermasalah. berikut adalah catatan dari penjual :<br/>" + textEmail + "</p>";
                //Set Body Text of Email   
                _mailmsg.Body = bodyMess;


                //Now set your SMTP   
                SmtpClient _smtp = new SmtpClient();

                //Set HOST server SMTP detail  
                _smtp.Host = emailSenderHost;

                //Set PORT number of SMTP  
                _smtp.Port = emailSenderPort;

                //Set SSL --> True / False  
                _smtp.EnableSsl = emailIsSSL;

                //Set Sender UserEmailID, Password  
                NetworkCredential _network = new NetworkCredential(emailSender, emailSenderPassword);
                _smtp.Credentials = _network;

                //Send Method will send your MailMessage create above.  
                _smtp.Send(_mailmsg);

            }
            catch (Exception ex)
            {
                string x = "Message not send";
            }
            
        }

    }
}
