using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderAndPaymentController : Controller
    {
        private readonly Data.BPartyContext _context;

        public OrderAndPaymentController(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_cart> tbl_cart { get; set; }
        public IList<tbl_AddressList> tbl_AddressList { get; set; }
        public IList<tbl_dtl_Order> tbl_dtl_Order { get; set; }
        public IList<costView> costViewList { get; set; }
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
        public class costView
        {
            public string service { get; set; }
            public string description { get; set; }
            public decimal value { get; set; }
            public string etd { get; set; }
            public string note { get; set; }
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("reTotal")]
        public async Task<JObject> reTotal([FromBody] JObject variabel)
        {
            try
            {
                if (HttpContext.User.FindFirst("sUserID")?.Value == null || HttpContext.User.FindFirst("sUserID")?.Value == "")
                {
                    return JObject.Parse("{ result:[{\"msg\": \"Not Login\"}]}");
                }
                else
                {

                    int product_id = int.Parse(variabel["product_id"].ToString());
                    int userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
                    tbl_cart res = _context.tbl_cart.Where(x => x.product_id == product_id && x.user_id == userid).FirstOrDefault();
                    if (res != null)
                    {
                        res.quantity = res.quantity + 1;
                        _context.Attach(res).State = EntityState.Modified;
                        _context.Entry(res).Property(x => x.quantity).IsModified = true;
                    }
                    else
                    {
                        tbl_cart tbl_cart_Add = new tbl_cart();
                        tbl_cart_Add.product_id = product_id;
                        tbl_cart_Add.user_id = userid;
                        tbl_cart_Add.quantity = 1;
                        _context.tbl_cart.Add(tbl_cart_Add);
                    }

                    await _context.SaveChangesAsync();
                    return JObject.Parse("{ result:[{\"msg\": \"Berhasil\"}]}");
                }
            }
            catch (Exception ex)
            {
                return JObject.Parse("{}");
            }
        }

        [HttpPost]
        [Route("saveAddress")]
        public async Task<JObject> saveAddress([FromBody] JObject value)
        {
            try
            {
                if (value["name"] != null && value["name"].ToString() != "" && value["phone"] != null && value["phone"].ToString() != "" && value["address"] != null && value["address"].ToString() != "" && value["city"] != null && value["city"].ToString() != "" &&
                    value["province"] != null && value["province"].ToString() != "")
                {
                    string return_string = "[";
                    tbl_User admin = _context.tbl_User.Where(x => x.flag_admin == "Y").FirstOrDefault();
                    tbl_User cust = _context.tbl_User.Where(x => x.user_id == int.Parse(value["user_id"].ToString())).FirstOrDefault();
                    tbl_dtl_Order = await _context.tbl_dtl_Order.Where(x => x.order_id == int.Parse(value["order_id"].ToString())).ToListAsync();

                    int weight = 0;
                    decimal subtotal = 0;
                    foreach (var item in tbl_dtl_Order)
                    {
                        tbl_Product prod = _context.tbl_Product.Where(x => x.product_id == item.product_id).FirstOrDefault();

                        weight = weight + (prod.product_weight * item.quantity);
                        subtotal = subtotal + (prod.price * item.quantity);
                    }

                    tbl_AddressList addNew = new tbl_AddressList();
                    addNew.user_id = int.Parse(value["user_id"].ToString());
                    addNew.user_fullname = value["name"].ToString();
                    addNew.user_address = value["address"].ToString();
                    addNew.user_phone = value["phone"].ToString();
                    addNew.city_id = int.Parse(value["city"].ToString());
                    addNew.province_id = int.Parse(value["province"].ToString());

                    _context.tbl_AddressList.Add(addNew);
                    await _context.SaveChangesAsync();
                    var address_id = addNew.address_id;

                    // get cost
                    var clientCost = new RestClient("https://api.rajaongkir.com/starter/cost");
                    var requestCost = new RestRequest(Method.POST);
                    requestCost.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
                    requestCost.AddHeader("content-type", "application/x-www-form-urlencoded");
                    requestCost.AddParameter("application/x-www-form-urlencoded", "origin=" + admin.city_id + "&destination=" + addNew.city_id + "&weight=" + weight + "&courier=jne", ParameterType.RequestBody);
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

                    if (costViewList != null)
                    {
                        foreach (var item in costViewList)
                        {
                            return_string = return_string + "{\"shipping_val\": \"" + item.value + "\", " + 
                                                        "\"msg\": \"\", " +
                                                        "\"address_id\": \"" + address_id + "\", " +
                                                        "\"shipping_srv\": \"" + item.service + "\", " +
                                                        "\"etd\": \"" + item.etd + "\"},";
                        }
                        return_string = return_string.Remove(return_string.Length - 1, 1);

                        return_string = return_string + "]";
                        return_string = "{ result: " + return_string + "}";
                    }

                    tbl_AddressList = await _context.tbl_AddressList.Where(x => x.user_id == int.Parse(value["user_id"].ToString())).ToListAsync();
                    return JObject.Parse(return_string);
                }
                else
                {
                    return JObject.Parse("{ result:[{\"msg\": \"Mohon isi data yang wajib diisi!\"}]}");
                }
                
            }
            catch (Exception ex)
            {
                return JObject.Parse("{}");
            }
        }

        [HttpPost]
        [Route("changeAddress")]
        public async Task<JObject> changeAddress([FromBody] JObject value)
        {
            try
            {
                if (value["address_id"] != null && value["address_id"].ToString() != "")
                {
                    int city_id = 0;
                    if(value["address_id"].ToString() == "0")
                    {
                        var res = _context.tbl_User.Where(x => x.user_id == int.Parse(value["user_id"].ToString())).FirstOrDefault();
                        city_id = res.city_id;
                    } else
                    {
                        var res = _context.tbl_AddressList.Where(x => x.address_id == int.Parse(value["address_id"].ToString())).FirstOrDefault();
                        city_id = res.city_id;
                    }

                    string return_string = "[";
                    tbl_User admin = _context.tbl_User.Where(x => x.flag_admin == "Y").FirstOrDefault();
                    tbl_dtl_Order = await _context.tbl_dtl_Order.Where(x => x.order_id == int.Parse(value["order_id"].ToString())).ToListAsync();

                    int weight = 0;
                    decimal subtotal = 0;
                    foreach (var item in tbl_dtl_Order)
                    {
                        tbl_Product prod = _context.tbl_Product.Where(x => x.product_id == item.product_id).FirstOrDefault();

                        weight = weight + (prod.product_weight * item.quantity);
                        subtotal = subtotal + (prod.price * item.quantity);
                    }

                    // get cost
                    var clientCost = new RestClient("https://api.rajaongkir.com/starter/cost");
                    var requestCost = new RestRequest(Method.POST);
                    requestCost.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
                    requestCost.AddHeader("content-type", "application/x-www-form-urlencoded");
                    requestCost.AddParameter("application/x-www-form-urlencoded", "origin=" + admin.city_id + "&destination=" + city_id + "&weight=" + weight + "&courier=jne", ParameterType.RequestBody);
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

                    if (costViewList.Count > 0)
                    {
                        foreach (var item in costViewList)
                        {
                            return_string = return_string + "{\"shipping_val\": \"" + item.value + "\", " +
                                                        "\"msg\": \"\", " +
                                                        "\"shipping_srv\": \"" + item.service + "\", " +
                                                        "\"etd\": \"" + item.etd + "\"},";
                        }
                        return_string = return_string.Remove(return_string.Length - 1, 1);

                        return_string = return_string + "]";
                        return_string = "{ result: " + return_string + "}";
                    }

                    return JObject.Parse(return_string);
                }
                else
                {
                    return JObject.Parse("{ result:[{\"msg\": \"Mohon isi data yang wajib diisi!\"}]}");
                }
            }
            catch (Exception ex)
            {
                return JObject.Parse("{}");
            }
        }

        [HttpPost]
        [Route("loadDdlAddress")]
        public async Task<JObject> loadDdlAddress([FromBody] JObject value)
        {
            try
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
                string return_string = "[";
                tbl_AddressList = await _context.tbl_AddressList.Where(x => x.user_id == int.Parse(value["user_id"].ToString())).ToListAsync();

                if(tbl_AddressList.Count > 0)
                {
                    foreach (var item in tbl_AddressList)
                    {
                        var city_name = cityViewList.Where(x => x.city_id == item.city_id).Select(x => x.city_name).FirstOrDefault();
                        var province_name = provinceViewList.Where(x => x.province_id == item.province_id).Select(x => x.province).FirstOrDefault();

                        return_string = return_string + "{\"address_id\": \"" + item.address_id + "\", " +
                                                        "\"msg\": \"\", " +
                                                        "\"user_id\": \"" + item.user_id + "\", " +
                                                        "\"name\": \"" + item.user_fullname + "\", " +
                                                        "\"phone\": \"" + item.user_phone + "\", " +
                                                        "\"address\": \"" + item.user_address + "\", " +
                                                        "\"city\": \"" + city_name + "\", " +
                                                        "\"province\": \"" + province_name + "\"},";
                    }
                    return_string = return_string.Remove(return_string.Length - 1, 1);

                    return_string = return_string + "]";
                    return_string = "{ result: " + return_string + "}";
                }

                return JObject.Parse(return_string);
            }
            catch (Exception ex)
            {
                return JObject.Parse("{}");
            }
        }

    }
}
