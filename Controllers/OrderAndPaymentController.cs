using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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
    }
}
