using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blessed_Party.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public ResetPasswordModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<ForgetPassword> ForgetPassword { get; set; }

        [BindProperty]
        public tbl_User Input { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostResetAsync(string new_password, string cpassword, string email, string token)
        {
            tbl_User res = _context.tbl_User.Where(x => x.user_email == email).FirstOrDefault();

            if (new_password == cpassword)
            {
                if (res != null)
                {
                    ForgetPassword res2 = _context.ForgetPassword.Where(x => x.user_id == res.user_id && x.token == token).FirstOrDefault();

                    if(res2 != null)
                    {
                        ASCIIEncoding encoding = new ASCIIEncoding();
                        byte[] keyBytes = encoding.GetBytes("BlessedParty");
                        byte[] messageBytes = encoding.GetBytes(new_password);
                        HMACSHA256 cryptographer = new HMACSHA256(keyBytes);
                        byte[] bytes = cryptographer.ComputeHash(messageBytes);

                        string encryptedString = BitConverter.ToString(bytes).Replace("-", "").ToLower();

                        res.userpass = encryptedString;
                        _context.Attach(res).Property(x => x.userpass).IsModified = true;

                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Password has changed.";
                        return RedirectToPage();
                    } else
                    {
                        TempData["Message"] = "Token Invalid. Password didn't change.";
                        return RedirectToPage();
                    }
                }
            }
            else
            {
                TempData["Message"] = "Password didn't match.";
                return Page();
            }

            return Page();
        }
    }
}
