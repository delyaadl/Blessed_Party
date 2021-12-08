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
        public IList<costView> costViewList { get; set; }

        [BindProperty]
        public tbl_Order edit_Order { get; set; }

        public class costView
        {
            public string service { get; set; }
            public string description { get; set; }
            public decimal value { get; set; }
            public string etd { get; set; }
            public string note { get; set; }
        }

        public async Task OnGetAsync(int order_id)
        {

            await LoadAll(order_id);
        }

        async Task LoadAll(int order_id)
        {
            int userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
            tbl_dtl_Order = await _context.tbl_dtl_Order.Where(x => x.order_id == order_id).ToListAsync();
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_Shipment shipp = _context.tbl_Shipment.Where(x => x.order_id == order_id).FirstOrDefault();
            tbl_Order res = _context.tbl_Order.Where(x => x.order_id == order_id).FirstOrDefault();
            tbl_User admin = _context.tbl_User.Where(x => x.flag_admin == "Y").FirstOrDefault();
            tbl_User cust = _context.tbl_User.Where(x => x.user_id == userid).FirstOrDefault();
            TempData["shipping_address"] = res.shipping_address;

            int weight = 0;
            decimal subtotal = 0;

            foreach(var item in tbl_dtl_Order)
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

            // get city
            var client1 = new RestClient("https://api.rajaongkir.com/starter/city");
            var request1 = new RestRequest(Method.GET);
            request1.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
            IRestResponse response1 = client1.Execute(request1);

            string result1 = response1.Content.Remove(response1.Content.Length - 2, 2).Split(new string[] { "results\":" }, StringSplitOptions.None).Last();

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

            if(jObjectRes["rajaongkir"]["results"] != null)
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
            } else
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
            if(shipp != null)
            {
                TempData["service"] = shipp.shipment_type;
                TempData["shipping_value"] = shipp.shipment_cost;
            } else
            {
                TempData["service"] ="";
            }

        }

        public async Task<IActionResult> OnPostOrderAsync(int order_id, string shipping_srv, decimal shipping_val, IFormFile proof_of_payment, int weight)
        {
            TempData["weight"] = weight;

            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }
            tbl_Order res = _context.tbl_Order.Where(x => x.order_id == order_id).FirstOrDefault();

            if (res != null)
            {
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

        public async Task<IActionResult> OnPostCancelAsync(int order_id)
        {
            tbl_Order tbl_order_cancel =  _context.tbl_Order.Where(x=>x.order_id == order_id).FirstOrDefault();

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
    }
}
