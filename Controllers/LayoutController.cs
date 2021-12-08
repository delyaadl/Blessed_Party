using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Blessed_Party.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LayoutController : Controller
    {
        private readonly Data.BPartyContext _context;

        public LayoutController(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] JObject user)
        {
            tbl_User = await _context.tbl_User.ToListAsync();
            string username = user["username"].ToString();
            string userpass = user["userpass"].ToString();
            tbl_User resultFind = _context.tbl_User.Where(x => x.username == username && x.userpass == userpass).FirstOrDefault();


            if (resultFind != null)
            {
                HttpContext.Session.SetString("sUserID", resultFind.user_id.ToString());
                HttpContext.Session.SetString("sUsername", resultFind.username);
                HttpContext.Session.SetString("sFullname", resultFind.user_fullname);
                HttpContext.Session.SetString("sEmail", resultFind.user_email);
                HttpContext.Session.SetString("sFlagAdmin", resultFind.flag_admin);
                HttpContext.Session.SetString("sPhone", resultFind.user_phone);

                return RedirectToPage("./Index");
            }
            else
            {
                TempData["errorMsg"] = "The username or password you entered is incorrect!";
                return RedirectToPage("./Index");
            }
        }
    }
}
