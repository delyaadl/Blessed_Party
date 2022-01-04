using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Blessed_Party.Pages
{
    public class OrderAndPaymentModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public OrderAndPaymentModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_dtl_Order> tbl_dtl_Order { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Shipment> tbl_Shipment { get; set; }
        public IList<tbl_Order> tbl_Order { get; set; }
        public IList<tbl_AddressList> tbl_AddressList { get; set; }
        public IList<costView> costViewList { get; set; }
        public IList<provinceView> provinceViewList { get; set; }
        public IList<cityView> cityViewList { get; set; }

        [BindProperty]
        public tbl_Order edit_Order { get; set; }

        [BindProperty]
        public tbl_AddressList tbl_AddressListAdd { get; set; }

        public class costView
        {
            public string service { get; set; }
            public string description { get; set; }
            public decimal value { get; set; }
            public string etd { get; set; }
            public string note { get; set; }
        }
        public class provinceView
        {
            public int province_id { get; set; }
            public string province { get; set; }
        }

        public class cityView
        {
            public int city_id { get; set; }
            public string city_name { get; set; }
        }

        public async Task OnGetAsync(int order_id)
        {

            await LoadAll(order_id);
        }

        async Task LoadAll(int order_id)
        {
            TempData["etd"] = "1-3";
            TempData["order_id"] = order_id;
            int userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
            tbl_dtl_Order = await _context.tbl_dtl_Order.Where(x => x.order_id == order_id).ToListAsync();
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_Shipment shipp = _context.tbl_Shipment.Where(x => x.order_id == order_id).FirstOrDefault();
            tbl_Order res = _context.tbl_Order.Where(x => x.order_id == order_id).FirstOrDefault();
            tbl_User admin = _context.tbl_User.Where(x => x.flag_admin == "Y").FirstOrDefault();
            tbl_User cust = _context.tbl_User.Where(x => x.user_id == userid).FirstOrDefault();
            tbl_AddressList = await _context.tbl_AddressList.Where(x => x.user_id == userid).ToListAsync();
            TempData["shipping_address"] = res.shipping_address;

            int weight = 0;
            decimal subtotal = 0;

            foreach (var item in tbl_dtl_Order)
            {
                tbl_Product prod = _context.tbl_Product.Where(x => x.product_id == item.product_id).FirstOrDefault();

                weight = weight + (prod.product_weight * item.quantity);
                subtotal = subtotal + (prod.price * item.quantity);
            }

            // get province
            var client = new RestClient("https://api.rajaongkir.com/starter/province");
            var request = new RestRequest(Method.GET);
            request.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
            IRestResponse response = client.Execute(request);

            string result = response.Content.Remove(response.Content.Length - 2, 2).Split(new string[] { "results\":" }, StringSplitOptions.None).Last();
            provinceViewList = JsonConvert.DeserializeObject<List<provinceView>>(result);

            // get city
            var client1 = new RestClient("https://api.rajaongkir.com/starter/city");
            var request1 = new RestRequest(Method.GET);
            request1.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
            IRestResponse response1 = client1.Execute(request1);

            string result1 = response1.Content.Remove(response1.Content.Length - 2, 2).Split(new string[] { "results\":" }, StringSplitOptions.None).Last();
            cityViewList = JsonConvert.DeserializeObject<List<cityView>>(result1);

            // get cost
            var clientCost = new RestClient("https://api.rajaongkir.com/starter/cost");
            var requestCost = new RestRequest(Method.POST);
            requestCost.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
            requestCost.AddHeader("content-type", "application/x-www-form-urlencoded");
            requestCost.AddParameter("application/x-www-form-urlencoded", "origin=" + admin.city_id + "&destination=" + cust.city_id + "&weight=" + weight + "&courier=jne", ParameterType.RequestBody);
            IRestResponse responseCost = clientCost.Execute(requestCost);

            string resultCost = responseCost.Content;
            JObject jObjectRes = JObject.Parse(resultCost);
            costViewList = new List<costView>();
            int counter = 0;

            if (jObjectRes["rajaongkir"]["results"] != null)
            {
                foreach (var item in jObjectRes["rajaongkir"]["results"])
                {

                    foreach (var x in item["costs"])
                    {
                        costView cv = new costView();
                        cv.service = x["service"].ToString();
                        cv.description = x["description"].ToString();

                        foreach (var y in x["cost"])
                        {
                            if (counter == 0)
                            {
                                TempData["shipping_value"] = decimal.Parse(y["value"].ToString());
                            }
                            cv.note = y["note"].ToString();
                            cv.value = decimal.Parse(y["value"].ToString());
                            cv.etd = y["etd"].ToString();
                        }

                        costViewList.Add(cv);
                        counter++;
                    }

                }
            }
            else
            {
                TempData["shipping_value"] = 0;
            }



            if (res != null)
            {
                if (res.proof_of_payment != null)
                {
                    TempData["fileUpload"] = "1 file has been uploaded.";
                }
                else
                {
                    TempData["fileUpload"] = "";
                }
            }
            else
            {
                TempData["fileUpload"] = "";
            }

            TempData["weight"] = weight;
            TempData["order_status"] = res.order_status;
            TempData["catatan"] = res.order_note;
            if (shipp != null)
            {
                TempData["service"] = shipp.shipment_type;
                TempData["shipping_value"] = shipp.shipment_cost;
                TempData["etd"] = costViewList.Where(x => x.service.Contains(shipp.shipment_type)).Select(x => x.etd).FirstOrDefault() ?? "1-3";
            }
            else
            {
                TempData["service"] = "";
            }

        }

        public async Task<IActionResult> OnPostOrderAsync(int order_id, string shipping_srv, decimal shipping_val, IFormFile proof_of_payment, int weight, int address_id)
        {

            if (shipping_srv == null || shipping_val == 0)
            {
                TempData["Message"] = "Pengiriman ke kota Anda belum tersedia / Anda belum memilih jenis pengiriman!";
                return RedirectToPage();
            }
            TempData["weight"] = weight;

            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }
            tbl_Order res = _context.tbl_Order.Where(x => x.order_id == order_id).FirstOrDefault();

            if (res != null)
            {
                if (address_id != 0)
                {
                    // get province
                    var client = new RestClient("https://api.rajaongkir.com/starter/province");
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
                    IRestResponse response = client.Execute(request);

                    string result = response.Content.Remove(response.Content.Length - 2, 2).Split(new string[] { "results\":" }, StringSplitOptions.None).Last();
                    provinceViewList = JsonConvert.DeserializeObject<List<provinceView>>(result);

                    // get city
                    var client1 = new RestClient("https://api.rajaongkir.com/starter/city");
                    var request1 = new RestRequest(Method.GET);
                    request1.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
                    IRestResponse response1 = client1.Execute(request1);

                    string result1 = response1.Content.Remove(response1.Content.Length - 2, 2).Split(new string[] { "results\":" }, StringSplitOptions.None).Last();
                    cityViewList = JsonConvert.DeserializeObject<List<cityView>>(result1);

                    tbl_AddressList result_Address = _context.tbl_AddressList.Where(x => x.address_id == address_id).FirstOrDefault();
                    string city = cityViewList.Where(x => x.city_id == result_Address.city_id).Select(x => x.city_name).FirstOrDefault();
                    string province = provinceViewList.Where(x => x.province_id == result_Address.province_id).Select(x => x.province).FirstOrDefault();
                    res.shipping_address = result_Address.user_fullname + " - " + result_Address.user_address + ", " + city + ", " + province + " " + result_Address.user_phone;
                }

                res.order_note = edit_Order.order_note;

                if (proof_of_payment == null)
                {
                    if (res.proof_of_payment == null)
                    {
                        TempData["Message"] = "Mohon masukkan bukti pembayaran!";
                        return RedirectToPage();
                    }
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        await proof_of_payment.CopyToAsync(ms);
                        string Filename = proof_of_payment.FileName;
                        byte[] Bytes1 = ms.ToArray();

                        res.proof_of_payment = Bytes1;

                    }
                }

                res.order_status = "1";

                _context.Attach(res).State = EntityState.Modified;
                _context.Entry(res).Property(x => x.order_status).IsModified = true;
                _context.Entry(res).Property(x => x.proof_of_payment).IsModified = true;
                _context.Entry(res).Property(x => x.order_note).IsModified = true;

                await _context.SaveChangesAsync();
            }

            tbl_Shipment ship = _context.tbl_Shipment.Where(x => x.order_id == order_id).FirstOrDefault();

            if (ship != null)
            {
                ship.shipment_cost = shipping_val;
                ship.shipment_type = shipping_srv;
                ship.shipment_weight = weight;

                _context.Attach(ship).State = EntityState.Modified;
                _context.Entry(ship).Property(x => x.shipment_weight).IsModified = true;
                _context.Entry(ship).Property(x => x.shipment_type).IsModified = true;
                _context.Entry(ship).Property(x => x.shipment_cost).IsModified = true;

                await _context.SaveChangesAsync();
            }
            else
            {
                tbl_Shipment newship = new tbl_Shipment();
                newship.order_id = order_id;
                newship.shipment_cost = shipping_val;
                newship.shipment_type = shipping_srv;
                newship.shipment_weight = weight;

                _context.tbl_Shipment.Add(newship);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Pesanan dengan Order ID " + order_id + " telah dibayar";
            }

            return RedirectToPage("HistoryOrder");
        }

        public async Task<IActionResult> OnPostCancelAsync(int order_id, string shipping_srv, decimal shipping_val, int weight)
        {
            tbl_Shipment ship = _context.tbl_Shipment.Where(x => x.order_id == order_id).FirstOrDefault();

            if (ship != null)
            {
                ship.shipment_cost = shipping_val;
                ship.shipment_type = shipping_srv;
                ship.shipment_weight = weight;

                _context.Attach(ship).State = EntityState.Modified;
                _context.Entry(ship).Property(x => x.shipment_weight).IsModified = true;
                _context.Entry(ship).Property(x => x.shipment_type).IsModified = true;
                _context.Entry(ship).Property(x => x.shipment_cost).IsModified = true;

                await _context.SaveChangesAsync();
            }
            else
            {
                tbl_Shipment newship = new tbl_Shipment();
                newship.order_id = order_id;
                newship.shipment_cost = shipping_val;
                newship.shipment_type = shipping_srv;
                newship.shipment_weight = weight;

                _context.tbl_Shipment.Add(newship);
                await _context.SaveChangesAsync();
            }

            tbl_Order tbl_order_cancel = _context.tbl_Order.Where(x => x.order_id == order_id).FirstOrDefault();

            if (tbl_order_cancel != null)
            {
                tbl_order_cancel.order_status = "9";
                tbl_order_cancel.order_finish_date = DateTime.Now;
                _context.Attach(tbl_order_cancel).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                TempData["Message"] = "Pesanan dengan Order ID " + order_id + " dibatalkan";
            }

            return RedirectToPage("HistoryOrder");
        }

        public async Task<IActionResult> OnPostTerimaAsync(int order_id)
        {
            tbl_Order tbl_order_accept = _context.tbl_Order.Where(x => x.order_id == order_id).FirstOrDefault();

            if (tbl_order_accept != null)
            {
                tbl_order_accept.order_status = "4";
                tbl_order_accept.order_finish_date = DateTime.Now;
                _context.Attach(tbl_order_accept).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("HistoryOrder");
        }

        public async Task<IActionResult> OnPostReuploadAsync(int order_id, IFormFile proof_of_payment_2)
        {
            tbl_Order res = _context.tbl_Order.Where(x => x.order_id == order_id).FirstOrDefault();

            if (res != null)
            {
                if (proof_of_payment_2 == null)
                {
                    TempData["Message"] = "Mohon masukkan bukti pembayaran!";
                    return RedirectToPage();
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        await proof_of_payment_2.CopyToAsync(ms);
                        string Filename = proof_of_payment_2.FileName;
                        byte[] Bytes1 = ms.ToArray();

                        res.proof_of_payment = Bytes1;

                    }
                }

                _context.Attach(res).State = EntityState.Modified;
                _context.Entry(res).Property(x => x.proof_of_payment).IsModified = true;

                await _context.SaveChangesAsync();
                sendEmail(res.order_id.ToString());
                TempData["Message"] = "Re-upload success!";
                return RedirectToPage();
            }

            return RedirectToPage();
        }

        protected void sendEmail(string order_id)
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

                string bodyMess = "<p>Bukti pesanan dengan order id " + order_id + " telah diunggah kembali. Silakan cek menu master_orders.</p>";
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
