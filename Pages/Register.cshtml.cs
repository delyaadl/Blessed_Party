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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Blessed_Party.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public RegisterModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<provinceView> provinceViewList { get; set; }
        public IList<cityView> cityViewList { get; set; }

        [BindProperty]
        public tbl_User Input { get; set; }

        public string ReturnUrl { get; set; }
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

        public async Task OnGetAsync(string returnUrl = null)
        {
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);


            ReturnUrl = returnUrl;

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
        }

        public async Task<IActionResult> OnPostRegisterAsync(string returnUrl = null)
        {

            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
                }

                Input.flag_admin = "N";

                tbl_User result = _context.tbl_User.Where(x => x.username == Input.username).FirstOrDefault();

                if (result != null)
                {
                    TempData["Message"] = "User already registered.";
                    return RedirectToPage();
                }
                else
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] keyBytes = encoding.GetBytes("BlessedParty");
                    byte[] messageBytes = encoding.GetBytes(Input.userpass);
                    HMACSHA256 cryptographer = new HMACSHA256(keyBytes);
                    byte[] bytes = cryptographer.ComputeHash(messageBytes);

                    string encryptedString = BitConverter.ToString(bytes).Replace("-", "").ToLower();

                    Input.userpass = encryptedString;

                    _context.tbl_User.Add(Input);
                    await _context.SaveChangesAsync();

                    tbl_User res2 = _context.tbl_User.Where(x => x.username == Input.username).FirstOrDefault();

                    var claims = new List<Claim>
                    {
                    new Claim("sUserID", Convert.ToString(res2.user_id)),
                    new Claim("sUserName", res2.username),
                    new Claim("sEmail", Convert.ToString(res2.user_email)),
                    new Claim("fAdmin", Convert.ToString(res2.flag_admin)),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToPage("/Index");

                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return RedirectToPage();
            }

        }
    }
}
