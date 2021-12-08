using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blessed_Party.CollaborativeFiltering;
using Blessed_Party.Models;
using Blessed_Party.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blessed_Party.Pages
{
    public class ProductDetailModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public ProductDetailModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Product> tbl_Product_2 { get; set; }
        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Rating_Product> tbl_Rating_Product { get; set; }
        public async Task OnGetAsync(string product_id)
        {
            await LoadAll(product_id);
        }

        async Task LoadAll(string product_id)
        {
            TempData["product_description"] = "";
            int total_rating = 0;
            if (!product_id.Contains(","))
            {
                tbl_Product prodRes = _context.tbl_Product.Where(x => x.product_id == int.Parse(product_id)).FirstOrDefault();
                tbl_Rating_Product = await _context.tbl_Rating_Product.Where(x => x.product_id == int.Parse(product_id)).ToListAsync();
                tbl_User = await _context.tbl_User.ToListAsync();
                tbl_Product = await _context.tbl_Product.ToListAsync();
                TempData["total_rating_count"] = tbl_Rating_Product.Count != 0 ? tbl_Rating_Product.Count : 0;
                if (tbl_Rating_Product.Count > 0)
                {
                    foreach (var item in tbl_Rating_Product)
                    {
                        total_rating = total_rating + int.Parse(item.rating_number.ToString());
                    }

                    total_rating = total_rating / tbl_Rating_Product.Count;
                }
                TempData["product_id"] = product_id;
                TempData["total_rating"] = total_rating;
                TempData["product_name"] = prodRes.product_name;
                TempData["product_description"] = prodRes.description;
                TempData["flag_available"] = prodRes.flag_available;
                TempData["price"] = prodRes.price;
                TempData["weight"] = prodRes.product_weight;
            } else
            {
                string[] prodID = product_id.Split(",");

                tbl_Product_2 = await _context.tbl_Product.Where(x => prodID.Contains(x.product_id.ToString())).ToListAsync();
                var prod = await _context.tbl_Product.Where(x => prodID.Contains(x.product_id.ToString())).ToListAsync();
                tbl_User = await _context.tbl_User.ToListAsync();
                tbl_Product = await _context.tbl_Product.ToListAsync();
                tbl_Rating_Product = await _context.tbl_Rating_Product.ToListAsync();
                string product_name = "";
                string product_description = "";
                string flag_available = "";
                decimal price = 0;
                decimal weight = 0;

                foreach(var item in prod)
                {
                    product_name = product_name + item.product_name + ", ";
                    if(item.description != null)
                    {
                        product_description = product_description + item.product_name + " DESKRIPSI : " + item.description + "end end ";
                    } else
                    {
                        product_description = product_description + item.product_name + " DESKRIPSI : - end end ";
                    }
                    flag_available = flag_available + item.flag_available + " + ";
                    weight = weight + item.product_weight;
                    price = price + item.price;
                }

                if(product_name.Length > 0)
                {
                    product_name = product_name.Remove(product_name.Length - 2, 2);
                }

                if (flag_available.Length > 0)
                {
                    flag_available = flag_available.Remove(flag_available.Length - 2, 2);
                }

                TempData["product_id"] = product_id;
                TempData["product_name"] = product_name;
                TempData["product_description"] = product_description;
                TempData["flag_available"] = flag_available;
                TempData["price"] = price;
                TempData["weight"] = weight;
            }
        }

        public async Task<IActionResult> OnPostRateAsync(int rating_value, int product_id)
        {
            CollaFil CF = new CollaFil(_context);
            if (HttpContext.User.FindFirst("sUserID")?.Value == null || HttpContext.User.FindFirst("sUserID")?.Value == "")
            {
                return RedirectToPage("Login", new { returnUrl = "/ProductDetail/" + product_id});
            }
            else
            {
                if (rating_value == 0)
                {
                    TempData["rating_msg"] = "rating minimal 1 bintang";
                    return RedirectToPage();
                }

                int userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
                tbl_Rating_Product res = _context.tbl_Rating_Product.Where(x => x.product_id == product_id && x.user_id == userid).FirstOrDefault();

                if (res == null)
                {
                    tbl_Rating_Product addNew = new tbl_Rating_Product();
                    addNew.user_id = userid;
                    addNew.product_id = product_id;
                    addNew.rating_number = rating_value;

                    _context.tbl_Rating_Product.Add(addNew);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    res.rating_number = rating_value;
                    _context.Attach(res).State = EntityState.Modified;
                    _context.Entry(res).Property(x => x.rating_number).IsModified = true;
                    await _context.SaveChangesAsync();

                }

                await CF.Prediction(userid);
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostAddToCartAsync(string prods_id, string product_id, int quantityCheck)
        {
            string[] prod_id = prods_id.Split(",");

            if (HttpContext.User.FindFirst("sUserID")?.Value == null || HttpContext.User.FindFirst("sUserID")?.Value == "")
            {
                return RedirectToPage("/Login", new { returnUrl = "/ProductDetail/" + product_id });
            }
            else
            {
                foreach (var item in prod_id)
                {
                    int prodct_id = int.Parse(item);
                    int userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
                    tbl_cart res = _context.tbl_cart.Where(x => x.product_id == prodct_id && x.user_id == userid).FirstOrDefault();
                    if (res != null)
                    {
                        res.quantity = res.quantity + quantityCheck;
                        _context.Attach(res).State = EntityState.Modified;
                        _context.Entry(res).Property(x => x.quantity).IsModified = true;
                    }
                    else
                    {
                        tbl_cart tbl_cart_Add = new tbl_cart();
                        tbl_cart_Add.product_id = prodct_id;
                        tbl_cart_Add.user_id = userid;
                        tbl_cart_Add.quantity = quantityCheck;
                        _context.tbl_cart.Add(tbl_cart_Add);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToPage("/Cart");
        }
    }
}
