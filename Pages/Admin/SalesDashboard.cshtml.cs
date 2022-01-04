using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blessed_Party.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blessed_Party.Pages.Admin
{
    public class SalesDashboardModel : PageModel
    {
        private readonly Data.BPartyContext _context;

        public SalesDashboardModel(Data.BPartyContext context)
        {
            _context = context;
        }

        public IList<SalesDashboard> SalesDashboard_tbl { get; set; }
        public IList<UserDashboard> UserDashboard_tbl { get; set; }
        public IList<tbl_Order> tbl_Order { get; set; }
        public IList<tbl_dtl_Order> tbl_dtl_Order { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_User> tbl_User { get; set; }

        public class SalesDashboard
        {
            public int Product { get; set; }
            public string Product_Name { get; set; }
            public int Quantity { get; set; }
        }

        public class UserDashboard
        {
            public int user_id { get; set; }
            public string username { get; set; }
            public int Quantity { get; set; }
        }

        public class StackedViewModel
        {
            public int Product { get; set; }
            public List<SalesDashboard> LstData { get; set; }
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
            tbl_dtl_Order = await _context.tbl_dtl_Order.ToListAsync();
            tbl_Order = await _context.tbl_Order.ToListAsync();
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_User = await _context.tbl_User.ToListAsync();
            var lstModel = new List<SalesDashboard>();
            var userModel = new List<UserDashboard>();
            var today = DateTime.Now;
            var max = new DateTime(today.Year, today.Month, today.Day); // first of this month
            var min = max.AddMonths(-1); // first of last month
            TempData["lastMonth"] = min.ToString("MMMM");

            var result = (from dtl in tbl_dtl_Order
                          join ord in tbl_Order on dtl.order_id equals ord.order_id
                          where ord.order_date >= min && ord.order_date < max
                          group dtl by dtl.product_id into newGroup
                          orderby newGroup.Key
                          select new { key = newGroup.Key, cnt = newGroup.Count() 
                          }).ToList();

            int i = 0;
            foreach(var item in result.OrderByDescending(x => x.cnt))
            {
                var product_name = "";
                if(i > 9)
                {
                    break;
                }

                foreach(var xx in tbl_Product.OrderByDescending(x => x.product_id))
                {
                    if(item.key == xx.product_id)
                    {
                        product_name = xx.product_name;

                        if(product_name.Length > 20)
                        {
                            product_name = product_name.Substring(0, 20) + "...";
                        }
                    }
                }
                lstModel.Add(
                    new SalesDashboard { Product = item.key, Product_Name = product_name, Quantity = item.cnt });
                i++;
            }

            var result2 = (from ord in tbl_Order
                          where ord.order_date >= min && ord.order_date < max
                          group ord by ord.user_id into newGroup
                          orderby newGroup.Key
                          select new
                          {
                              key = newGroup.Key,
                              cnt = newGroup.Count()
                          }).ToList();

            int b = 0;
            foreach (var item in result2.OrderByDescending(x => x.cnt))
            {
                var username = "";
                if (b > 9)
                {
                    break;
                }

                foreach (var xx in tbl_User.OrderByDescending(x => x.user_id))
                {
                    if (item.key == xx.user_id)
                    {
                        username = xx.username;

                        if (username.Length > 20)
                        {
                            username = username.Substring(0, 20) + "...";
                        }
                    }
                }
                userModel.Add(
                    new UserDashboard { user_id = item.key, username = username, Quantity = item.cnt });
                b++;
            }

            UserDashboard_tbl = userModel;
            SalesDashboard_tbl = lstModel;

        }
    }
}
