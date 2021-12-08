using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blessed_Party.Apriori;
using Blessed_Party.CollaborativeFiltering;
using Blessed_Party.Models;
using Blessed_Party.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Blessed_Party.Pages
{
    public class IndexModel : PageModel
    {

        private readonly Data.BPartyContext _context;
        private static Data.BPartyContext _context2;

        public IndexModel(Data.BPartyContext context)
        {
            _context = context;
            _context2 = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Category> tbl_Category { get; set; }
        public IList<AdvicedProds> AdvP { get; set; }
        public IList<AdvicedProds> advPCf { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Rating_Product> tbl_Rating_Product { get; set; }
        public IList<tbl_Prediction> tbl_Prediction { get; set; }

        public class AdvicedProds
        {
            public int apriori_id { get; set; }
            public List<int> product_id { get; set; }
            public List<string> product_name { get; set; }
            public List<string> description { get; set; }
            public List<int> product_weight { get; set; }
            public List<decimal> price { get; set; }
            public List<byte[]> product_image { get; set; }
            public List<DateTime> created_date { get; set; }
            public List<string> flag_available { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadAll();
        }

        async Task LoadAll()
        {
            AdvP = new List<AdvicedProds>();
            int userid = 0;
            AprioriProcess AP = new AprioriProcess(_context2);
            CollaFil CF = new CollaFil(_context2);

            tbl_User = await _context.tbl_User.ToListAsync();
            tbl_Category = await _context.tbl_Category.ToListAsync();
            tbl_Product = await _context.tbl_Product.Where(x => x.flag_available == "Y").OrderByDescending(x => x.created_date).ToListAsync();

            List<int[]> transactionlist = await AP.CreateTransactions();
            List<int> allproducts = await AP.ListAllProducts();
            List<AdvicedProduct2> advices = AP.DoApriori(allproducts, allproducts, allproducts.Count, transactionlist);

            var count = 1;
            foreach(var item in advices)
            {
                List<int> product_id = new List<int>();
                List<string> product_name = new List<string>();
                List<string> description = new List<string>();
                List<int> product_weight = new List<int>();
                List<decimal> price = new List<decimal>();
                List<byte[]> product_image = new List<byte[]>();
                List<DateTime> created_date = new List<DateTime>();
                List<string> flag_available = new List<string>();

                foreach (var x in item.antecedents_id)
                {
                    tbl_Product res = _context.tbl_Product.Where(y => y.product_id == x).FirstOrDefault();
                    product_id.Add(res.product_id);
                    product_name.Add(res.product_name);
                    description.Add(res.description);
                    product_weight.Add(res.product_weight);
                    price.Add(res.price);
                    product_image.Add(res.product_image);
                    created_date.Add(res.created_date);
                    flag_available.Add(res.flag_available);
                }

                if (!flag_available.Contains("N"))
                {
                    AdvP.Add(new AdvicedProds
                    {
                        apriori_id = count,
                        product_id = product_id,
                        product_name = product_name,
                        product_weight = product_weight,
                        description = description,
                        price = price,
                        product_image = product_image,
                        created_date = created_date,
                        flag_available = flag_available
                    });
                    count++;
                }
            }

            if (HttpContext.User.FindFirst("sUserID")?.Value != null)
            {
                userid = int.Parse(HttpContext.User.FindFirst("sUserID")?.Value);
                List<tbl_Rating_Product> rating_List = _context.tbl_Rating_Product.Where(x => x.user_id == userid).ToList();
                //var max_date = _context.tbl_Prediction.Where(x => x.user_id == userid).OrderByDescending(e => e.created_date).Select(x => x.created_date).FirstOrDefault();
                //var min_date = max_date.AddMinutes(-0.2);
                tbl_Prediction = await _context.tbl_Prediction.Where(x => x.user_id == userid).ToListAsync();

                advPCf = new List<AdvicedProds>();
                
                foreach(var item in tbl_Prediction)
                {
                    List<AdvicedProds> newAdd = AdvP.Where(x => x.product_id.Contains(item.product_id)).ToList();
                    foreach(var x in newAdd) 
                    {
                        if(advPCf.Where(y => y.apriori_id == x.apriori_id).FirstOrDefault() == null)
                        {
                            advPCf.Add(x);
                        }
                    }
                }
            }
        }
    }
}
