using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blessed_Party.Pages
{
    public class ForgetPasswordModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public ForgetPasswordModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<ForgetPassword> ForgetPassword { get; set; }

        [BindProperty]
        public tbl_User Input { get; set; }

        public void OnGet(string returnUrl = null)
        {
        }

        public async Task<IActionResult> OnPostForgetAsync()
        {
            tbl_User res = await _context.tbl_User.Where(x => x.username == Input.username && x.user_email == Input.user_email).FirstOrDefaultAsync();

            if(res != null)
            {
                return RedirectToPage("ResetPassword", new { username = Input.username, email = Input.user_email});
            }

            TempData["Message"] = "Silakan menyesuaikan email dan username yang telah didaftarkan.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostForgotAsync()
        {
            try
            {
                tbl_User res = _context.tbl_User.Where(x => x.user_email == Input.user_email).FirstOrDefault();

                SmtpClient smtp_server = new SmtpClient();
                MailMessage _mailmsg = new MailMessage();
                string emailSender = "theyale.id@gmail.com";
                string emailSenderPassword = "liacantik11";
                string emailSenderHost = "smtp.gmail.com";
                int emailSenderPort = 587;
                bool emailIsSSL = true;

                string subject = "Reset your password";

                _mailmsg.From = new MailAddress(emailSender, "Blessed Party");

                if (res != null)
                {
                    int userId = res.user_id;
                    string password = res.userpass;
                    var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    var resultToken = new string(
                       Enumerable.Repeat(allChar, 8)
                       .Select(token => token[random.Next(token.Length)]).ToArray());

                    string token = resultToken.ToString();

                    ForgetPassword result = _context.ForgetPassword.Where(x => x.user_id == userId && x.token == token).FirstOrDefault();

                    //Create URL with above token
                    var lnkHref = "<a href='https://localhost:44330/ResetPassword?email=" + Input.user_email +"&token=" + token + ">Reset Password</a>";

                    if (result != null)
                    {
                        if (result.lastupdate_date < DateTime.UtcNow.AddHours(-24))
                        {
                            TempData["Message"] = "Token has expired. Please generate a new one.";
                            return RedirectToPage();
                        }
                    }
                    else
                    {
                        ForgetPassword newAdd = new ForgetPassword();
                        newAdd.user_id = userId;
                        newAdd.token = token;
                        newAdd.lastupdate_date = DateTime.Now;
                        _context.ForgetPassword.Add(newAdd);
                        await _context.SaveChangesAsync();
                    }

                    if (token == null)
                    {
                        // If user does not exist or is not confirmed.
                        TempData["Message"] = "Email not Registered.";
                        return RedirectToPage();
                    }
                    else
                    {
                        //Make TRUE because our body text is html  
                        _mailmsg.IsBodyHtml = true;

                        //Set From Email ID  
                        _mailmsg.From = new MailAddress(emailSender);

                        //Set To Email ID  
                        _mailmsg.To.Add("delyatjung@gmail.com");

                        //Set Subject  
                        _mailmsg.Subject = subject;

                        string bodyMess = "<b>Copy the link below to your webpage to reset your password. </b><br/> https://localhost:44330/ResetPassword/" + Input.user_email + "/" + token;
                        //Set Body Text of Email   
                        _mailmsg.Body = bodyMess;

                        //Set HOST server SMTP detail  
                        smtp_server.Host = emailSenderHost;

                        //Set PORT number of SMTP  
                        smtp_server.Port = emailSenderPort;

                        //Set SSL --> True / False  
                        smtp_server.EnableSsl = emailIsSSL;

                        //Set Sender UserEmailID, Password  
                        NetworkCredential _network = new NetworkCredential(emailSender, emailSenderPassword);
                        smtp_server.Credentials = _network;

                        //Send Method will send your MailMessage create above.  
                        smtp_server.Send(_mailmsg);
                        TempData["Message"] = "Reset password link has been sent to your email.";
                    }
                }
                else
                {
                    TempData["Message"] = "Email doesn't exist.";
                }
                return RedirectToPage();
            } catch (Exception ex)
            {
                return RedirectToPage();
            }
            
        }
    }
}
