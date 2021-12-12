using Blessed_Party.Models;
using egitlab_PotionNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexController : Controller
    {
        private readonly Data.BPartyContext _context;

        public IndexController(Data.BPartyContext context)
        {
            _context = context;
        }

        QueryData queryData = new QueryData();
        public IList<tbl_cart> tbl_cart { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("addToCart")]
        public async Task<JObject> addToCart([FromBody] JObject variabel)
        {
            try
            {
                if(HttpContext.User.FindFirst("sUserID")?.Value == null || HttpContext.User.FindFirst("sUserID")?.Value == "")
                {
                    return JObject.Parse("{ result:[{\"msg\": \"Not Login\"}]}");
                } else
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
        [Route("addToCartDetail")]
        public async Task<JObject> addToCartDetail([FromBody] JObject variabel)
        {
            try
            {
                if (HttpContext.User.FindFirst("sUserID")?.Value == null || HttpContext.User.FindFirst("sUserID")?.Value == "")
                {
                    return JObject.Parse("{ result:[{\"msg\": \"Not Login\", \"returnUrl\": \"ProductDetail%2F" + variabel["product_id"].ToString() + "\"}]}");
                }
                else
                {

                    int product_id = int.Parse(variabel["product_id"].ToString());
                    int userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
                    tbl_cart res = _context.tbl_cart.Where(x => x.product_id == product_id && x.user_id == userid).FirstOrDefault();
                    if (res != null)
                    {
                        res.quantity = res.quantity + int.Parse(variabel["quantity"].ToString());
                        _context.Attach(res).State = EntityState.Modified;
                        _context.Entry(res).Property(x => x.quantity).IsModified = true;
                    }
                    else
                    {
                        tbl_cart tbl_cart_Add = new tbl_cart();
                        tbl_cart_Add.product_id = product_id;
                        tbl_cart_Add.user_id = userid;
                        tbl_cart_Add.quantity = int.Parse(variabel["quantity"].ToString());
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
        [Route("SearchProduct")]
        public JObject SearchProduct([FromBody] JObject query)
        {
            string return_string = "[";
            try
            {
                DataTable dt = new DataTable();
                string strSql = "SELECT * FROM tbl_Product " +
                                "where product_name LIKE '%" + query["query"].ToString() + "%' and flag_available = 'Y' " +
                                "order by created_date desc";

                dt = queryData.GetDataSql(strSql);

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        return_string = return_string + "{\"product_id\": \"" + dt.Rows[i]["product_id"].ToString() +
                                                        "\", \"product_name\": \"" + dt.Rows[i]["product_name"].ToString() +
                                                        "\", \"description\": \"" + dt.Rows[i]["description"].ToString() +
                                                        "\", \"product_weight\": \"" + dt.Rows[i]["product_weight"].ToString() +
                                                        "\", \"price\": \"" + dt.Rows[i]["price"].ToString() +
                                                        "\", \"product_image\": \"" + string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String((byte[])dt.Rows[i]["product_image"]))  +
                                                        "\", \"created_date\": \"" + dt.Rows[i]["created_date"].ToString() +
                                                        "\", \"flag_available\": \"" + dt.Rows[i]["flag_available"].ToString() +
                                                        "\"},";
                    }
                    return_string = return_string.Remove(return_string.Length - 1, 1);
                }
                return_string = return_string + "]";
                return_string = "{ result: " + return_string + "}";
                return JObject.Parse(return_string);
            }
            catch (Exception ex) { return JObject.Parse("{}"); }
        }

        [HttpPost]
        [Route("ChangeCategory")]
        public JObject ChangeCategory([FromBody] JObject query)
        {
            string return_string = "[";
            try
            {
                DataTable dt = new DataTable(); 
                string strSql = "";
                if (query["query"].ToString() != "9999")
                {
                    strSql = "SELECT * FROM tbl_Product " +
                                    "where category_id =" + query["query"].ToString() + " and flag_available = 'Y' " +
                                    "order by created_date desc";
                } else
                {
                    strSql = "SELECT * FROM tbl_Product " +
                                    "where flag_available = 'Y' " +
                                    "order by created_date desc";
                }

                dt = queryData.GetDataSql(strSql);

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        return_string = return_string + "{\"product_id\": \"" + dt.Rows[i]["product_id"].ToString() +
                                                        "\", \"product_name\": \"" + dt.Rows[i]["product_name"].ToString() +
                                                        "\", \"description\": \"" + dt.Rows[i]["description"].ToString() +
                                                        "\", \"product_weight\": \"" + dt.Rows[i]["product_weight"].ToString() +
                                                        "\", \"price\": \"" + dt.Rows[i]["price"].ToString() +
                                                        "\", \"product_image\": \"" + string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String((byte[])dt.Rows[i]["product_image"])) +
                                                        "\", \"created_date\": \"" + dt.Rows[i]["created_date"].ToString() +
                                                        "\", \"flag_available\": \"" + dt.Rows[i]["flag_available"].ToString() +
                                                        "\"},";
                    }
                    return_string = return_string.Remove(return_string.Length - 1, 1);
                }
                return_string = return_string + "]";
                return_string = "{ result: " + return_string + "}";
                return JObject.Parse(return_string);
            }
            catch (Exception ex) { return JObject.Parse("{}"); }
        }

        [HttpPost]
        [Route("changeQuantity")]
        public async Task<JObject> changeQuantity([FromBody] JObject variabel)
        {
            try
            {
                if (HttpContext.User.FindFirst("sUserID")?.Value == null || HttpContext.User.FindFirst("sUserID")?.Value == "")
                {
                    return JObject.Parse("{ result:[{\"msg\": \"Not Login\"}]}");
                }
                else
                {
                    tbl_cart res = _context.tbl_cart.Where(x => x.cart_id == int.Parse(variabel["cart_id"].ToString())).FirstOrDefault();

                    res.quantity = int.Parse(variabel["quantity"].ToString());

                    _context.Attach(res).State = EntityState.Modified;
                    _context.Entry(res).Property(x => x.quantity).IsModified = true;
                    await _context.SaveChangesAsync();
                    return JObject.Parse("{ result:[{\"msg\": \"Berhasil\"}]}");
                }
            }
            catch (Exception ex)
            {
                return JObject.Parse("{}");
            }
        }
    }
}
