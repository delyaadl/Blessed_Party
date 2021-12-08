using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blessed_Party.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public ChangePasswordModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }

        [BindProperty]
        public tbl_User Input { get; set; }

        public void OnGet(int user_id)
        {
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(string old_password, string new_password, string cpassword)
        {
            int userID = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
            tbl_User res = _context.tbl_User.Where(x => x.user_id == userID).FirstOrDefault();

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] keyBytes = encoding.GetBytes("BlessedParty");
            byte[] messageBytes = encoding.GetBytes(old_password);
            HMACSHA256 cryptographer = new HMACSHA256(keyBytes);
            byte[] bytes = cryptographer.ComputeHash(messageBytes);

            string encryptedString = BitConverter.ToString(bytes).Replace("-", "").ToLower();

            if (res.userpass == encryptedString)
            {
                if (new_password == cpassword)
                {
                    if (res != null)
                    {
                        ASCIIEncoding encoding1 = new ASCIIEncoding();
                        byte[] keyBytes1 = encoding.GetBytes("BlessedParty");
                        byte[] messageBytes1 = encoding.GetBytes(new_password);
                        HMACSHA256 cryptographer1 = new HMACSHA256(keyBytes1);
                        byte[] bytes1 = cryptographer1.ComputeHash(messageBytes1);

                        string encryptedString1 = BitConverter.ToString(bytes1).Replace("-", "").ToLower();

                        res.userpass = encryptedString1;
                        _context.Attach(res).Property("userpass").IsModified = true;

                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Password has changed.";
                        return RedirectToPage();
                    }
                }
                else
                {
                    TempData["Message"] = "New Password and Confirmation didn't match.";
                    return RedirectToPage();
                }
            } else
            {
                TempData["Message"] = "Old password didn't match. Please re-check.";
            }
            

            return RedirectToPage();
        }
    }
}
