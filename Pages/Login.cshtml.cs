using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Blessed_Party.Pages
{
    public class LoginModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public LoginModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }

        [BindProperty]
        public UsernameModel Input { get; set; }

        public class UsernameModel
        {
            public string username { get; set; }
            public string password { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);


            TempData["returnUrl"] = returnUrl;
        }

        public async Task<IActionResult> OnPostLogin(string returnUrl = null)
        {
            if(returnUrl != null)
            {
                if (returnUrl.Contains("Logout") || returnUrl.Contains("Register"))
                {
                    returnUrl = null;
                }
            }

            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            }

            tbl_User = await _context.tbl_User.ToListAsync();
            tbl_User resultFind = _context.tbl_User.Where(x => x.username == Input.username).FirstOrDefault();


            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] keyBytes = encoding.GetBytes("BlessedParty");
            byte[] messageBytes = encoding.GetBytes(Input.password);
            HMACSHA256 cryptographer = new HMACSHA256(keyBytes);
            byte[] bytes = cryptographer.ComputeHash(messageBytes);

            string encryptedString = BitConverter.ToString(bytes).Replace("-", "").ToLower();

            if (resultFind != null)
            {
                if (encryptedString != resultFind.userpass)
                {
                    TempData["Message"] = "The password you entered is incorrect";
                    return RedirectToPage();
                }
                else
                {
                    var claims = new List<Claim>
                    {
                    new Claim("sUserID", Convert.ToString(resultFind.user_id)),
                    new Claim("sUserName", resultFind.username),
                    new Claim("sEmail", Convert.ToString(resultFind.user_email)),
                    new Claim("fAdmin", Convert.ToString(resultFind.flag_admin)),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                }

                if (returnUrl != null)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    return LocalRedirect("/Index");
                }
            }
            else
            {
                TempData["Message"] = "The username you entered doesn't exist.";
                return RedirectToPage();
            }
        }
    }
}
