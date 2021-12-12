using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;

namespace Blessed_Party.Pages
{
    public class CartModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public CartModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Order> tbl_Order { get; set; }
        public IList<tbl_dtl_Order> tbl_dtl_Order { get; set; }
        public IList<tbl_cart> tbl_cart { get; set; }
        public IList<provinceView> provinceViewList { get; set; }
        public IList<cityView> cityViewList { get; set; }

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
            int userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
            tbl_cart = await _context.tbl_cart.Where(x => x.user_id == userid).ToListAsync();
            tbl_Product = await _context.tbl_Product.ToListAsync();
        }

        public async Task<IActionResult> OnPostCheckoutAsync(string hdnCartId)
        {
            try
            {
                int userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
                tbl_User user = _context.tbl_User.Where(x => x.user_id == userid).FirstOrDefault();
                string province = "";
                string city = "";

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

                foreach (var item in provinceViewList)
                {
                    if(item.province_id == user.province_id)
                    {
                        province = item.province;
                        break;
                    }
                }

                foreach (var item in cityViewList)
                {
                    if(item.city_id == user.city_id)
                    {
                        city = item.city_name;
                        break;
                    }
                }

                decimal amount = 0;

                if (hdnCartId == null || hdnCartId == "")
                {
                    TempData["Message"] = "Silakan pilih produk yang akan dicheckout";
                } else
                {
                    tbl_Order order_Add = new tbl_Order();

                    order_Add.order_status = "0";
                    order_Add.order_date = DateTime.Now;
                    order_Add.user_id = userid;
                    order_Add.shipping_address = user.user_address + ", " + city + ", " + province;

                    _context.tbl_Order.Add(order_Add);
                    await _context.SaveChangesAsync();
                    int orderid = order_Add.order_id;

                    string[] cart_arr = hdnCartId.Split(",");
                    foreach(var item in cart_arr)
                    {
                        tbl_dtl_Order dtl_Add = new tbl_dtl_Order();
                        tbl_cart res = _context.tbl_cart.Where(x => x.cart_id == int.Parse(item)).FirstOrDefault();
                        tbl_Product prod = _context.tbl_Product.Where(x => x.product_id == res.product_id).FirstOrDefault();
                        tbl_Order ord = _context.tbl_Order.Where(x => x.user_id == userid).OrderBy(x => x.order_id).LastOrDefault();
                        amount = amount + (prod.price * res.quantity);

                        dtl_Add.product_id = res.product_id;
                        dtl_Add.quantity = res.quantity;
                        dtl_Add.price = prod.price;
                        dtl_Add.order_id = orderid;
                        ord.amount = amount;

                        tbl_cart tbl_cart_Delete = new tbl_cart();
                        tbl_cart_Delete = await _context.tbl_cart.FindAsync(int.Parse(item));

                        if (tbl_cart_Delete != null)
                        {
                            _context.tbl_cart.Remove(tbl_cart_Delete);
                        }

                        _context.Attach(ord).State = EntityState.Modified;
                        _context.tbl_dtl_Order.Add(dtl_Add);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToPage("OrderAndPayment", new { order_id = orderid});
                }

                return RedirectToPage();
            } catch(Exception ex)
            {
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int cart_id_delete)
        {

            tbl_cart tbl_cart_delete = await _context.tbl_cart.FindAsync(cart_id_delete);

            if (tbl_cart_delete != null)
            {
                _context.tbl_cart.Remove(tbl_cart_delete);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Data berhasil dihapus!";
            }

            return RedirectToPage();
        }
    }
}
