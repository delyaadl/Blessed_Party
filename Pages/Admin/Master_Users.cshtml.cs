using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;

namespace Blessed_Party.Pages.Admin
{
    public class Master_UsersModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public Master_UsersModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_cart> tbl_cart { get; set; }
        public IList<tbl_Rating_Product> tbl_Rating_Product { get; set; }
        public IList<tbl_Prediction> tbl_Prediction { get; set; }
        public IList<tbl_Order> tbl_Order { get; set; }
        public IList<tbl_Prediction40> tbl_Prediction40 { get; set; }
        public IList<tbl_Prediction80> tbl_Prediction80 { get; set; }
        public IList<tbl_Rating_40_User> tbl_Rating_40_User { get; set; }
        public IList<tbl_Rating_80_User> tbl_Rating_80_User { get; set; }
        public IList<provinceView> provinceViewList { get; set; }
        public IList<cityView> cityViewList { get; set; }

        //[BindProperty]
        //public tbl_User tbl_User_Add { get; set; }

        [BindProperty]
        public tbl_User tbl_User_Edit { get; set; }

        [BindProperty]
        public tbl_User tbl_User_Delete { get; set; }

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

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.User.FindFirst("fAdmin")?.Value == null || HttpContext.User.FindFirst("fAdmin")?.Value == "N")
            {
                return denyAccess();
            }
            else
            {
                await LoadAll();
                return Page();
            }
        }

        public IActionResult denyAccess()
        {
            return RedirectToPage("/Index");
        }

        async Task LoadAll()
        {
            tbl_User = await _context.tbl_User.ToListAsync();

            // get province
            var client = new RestClient("https://api.rajaongkir.com/starter/province");
            var request = new RestRequest(Method.GET);
            request.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
            IRestResponse response = client.Execute(request);

            string result = response.Content.Remove(response.Content.Length - 2, 2).Split(new string[] { "results\":" }, StringSplitOptions.None).Last() ;
            provinceViewList = JsonConvert.DeserializeObject<List<provinceView>>(result);

            // get city
            var client1 = new RestClient("https://api.rajaongkir.com/starter/city");
            var request1 = new RestRequest(Method.GET);
            request1.AddHeader("key", "e7cf7163ebbfca49ec5b2b2f70f89277");
            IRestResponse response1 = client1.Execute(request1);

            string result1 = response1.Content.Remove(response1.Content.Length - 2, 2).Split(new string[] { "results\":" }, StringSplitOptions.None).Last();
            cityViewList = JsonConvert.DeserializeObject<List<cityView>>(result1);
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            tbl_User checkUN = _context.tbl_User.Where(x => x.username == tbl_User_Edit.username && x.user_id != tbl_User_Edit.user_id).FirstOrDefault();
            tbl_User res = _context.tbl_User.Where(x => x.user_id == tbl_User_Edit.user_id).FirstOrDefault();
            if(checkUN != null)
            {
                TempData["Message"] = "username telah terdaftar!";
            } else
            {

                if (res != null)
                {
                    _context.Entry(res).State = EntityState.Detached;
                    _context.Attach(tbl_User_Edit).State = EntityState.Modified;
                    _context.Entry(tbl_User_Edit).Property(x => x.userpass).IsModified = false;
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Data berhasil diubah!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!tbl_UserExists(tbl_User_Edit.user_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["Message"] = "Data tidak berhasil diubah, silakan periksa kembali!";
                    }
                }
            }

            return RedirectToPage();
        }

        private bool tbl_UserExists(int id)
        {
            return _context.tbl_User.Any(e => e.user_id == id);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int user_id)
        {

            tbl_User_Delete = await _context.tbl_User.FindAsync(user_id);
            tbl_cart = await _context.tbl_cart.Where(x => x.user_id == user_id).ToListAsync();
            tbl_Rating_Product = await _context.tbl_Rating_Product.Where(x => x.user_id == user_id).ToListAsync();
            tbl_Rating_40_User = await _context.tbl_Rating_40_User.Where(x => x.user_id == user_id).ToListAsync();
            tbl_Rating_80_User = await _context.tbl_Rating_80_User.Where(x => x.user_id == user_id).ToListAsync();
            tbl_Prediction = await _context.tbl_Prediction.Where(x => x.user_id == user_id).ToListAsync();
            tbl_Prediction40 = await _context.tbl_Prediction40.Where(x => x.user_id == user_id).ToListAsync();
            tbl_Prediction80 = await _context.tbl_Prediction80.Where(x => x.user_id == user_id).ToListAsync();
            tbl_Order = await _context.tbl_Order.Where(x => x.user_id == user_id).ToListAsync();

            if(tbl_cart.Count > 0 || tbl_Rating_Product.Count > 0 || tbl_Prediction.Count > 0 || tbl_Prediction40.Count > 0 || tbl_Prediction80.Count > 0 ||
                tbl_Rating_40_User.Count > 0 || tbl_Rating_80_User.Count > 0 || tbl_Order.Count > 0)
            {
                TempData["Message"] = "Tidak dapat menghapus user karena user sudah pernah melakukan transaksi / rating!";
                return RedirectToPage();
            }

            if (tbl_User_Delete != null)
            {
                _context.tbl_User.Remove(tbl_User_Delete);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Data berhasil dihapus!";
            }

            tbl_User = await _context.tbl_User.ToListAsync();

            return RedirectToPage();
        }
    }
}
