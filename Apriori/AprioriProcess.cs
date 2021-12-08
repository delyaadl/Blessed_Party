using Blessed_Party.Models;
using Blessed_Party.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Apriori
{
    public class AprioriProcess
    {
        private readonly Data.BPartyContext _context;

        public AprioriProcess(Data.BPartyContext context)
        {
            this._context = context;
        }

        // Step 1
        // membuat list transaksi yang isinya array dari tiap index produk dalam transaksi yang sama
        public async Task<List<int[]>> CreateTransactions()
        {
            List<int> allproducts = await ListAllProducts();

            List<int> orderIDs = await _context.tbl_Order.Select(x => x.order_id).ToListAsync();

            int[] product;

            List<int[]> transactionindex = new List<int[]>();

            List<int> yy;

            // looping seluruh order yang ada
            foreach (var orderid in orderIDs) 
            { 
                product = new int[] { };
                yy = new List<int>();

                // ngambil detail order, kayak produk apa aja yang ada di order x
                var orderdetails = await _context.tbl_dtl_Order.Where(c => c.order_id == orderid).OrderBy(x => x.product_id).ToListAsync();

                // ngambil semua product_id yang ada di order detail order x
                product = orderdetails.Select(v => v.product_id).ToArray();

                // ngambil index dari setiap id produknya, index mulai dari 0, jadi kalo index di db - 1
                foreach (var p in product)
                {
                    int y = allproducts.IndexOf(p);
                    yy.Add(y);
                }

                // bikin array of array integer, yang value arraynya adalah array index product dalam sebuah order yang sama
                transactionindex.Add(yy.ToArray());
            }
            return transactionindex;
        }

        // get all products, dibikin fungsi biar ga ngulang2 ngetik code yang sama
        public async Task<List<int>> ListAllProducts()
        {
            List<int> allproducts =  await _context.tbl_Product.Select(x => x.product_id).ToListAsync();
            return allproducts;
        }

        // step 2
        public List<AdvicedProduct2> DoApriori(List<int> productids, List<int> allproducts, int N, List<int[]> transactionindex)
        {
            //int minSupport = 6; // inisiasi min sup
            int minSupport = 12; // inisiasi min sup
            double minConfidence = 0.75; // minimum confidence

            // cari frequent itemsets yang memenuhi minimum support, minimal 2, maksimal 3 item
            List<ItemSet> frequentItemSets = Apriori.GetFrequentItemSets(N, transactionindex, minSupport);
            
            List<Rule> goodRules = Apriori.GetHighConfRules(frequentItemSets, transactionindex, minConfidence);

            List<int> Product_Ids = new List<int>();
            foreach (var id in productids)
            {
                int Product_Id = allproducts.IndexOf(id);//Finds the index of the selected product's id from allproducts
                Product_Ids.Add(Product_Id);
            }
            var advices = new List<AdvicedProduct2>();//suggestion for products from modelview class (created to define in view.)
            
            List<int> xx = new List<int>();
            List<int> cc;

            //looks at each of the extracted association rules
            //  foreach (var rule in goodRules)
            for (int i = 0; i < goodRules.Count; ++i)
            {
                cc = new List<int>();
                for (int c = 0; c < goodRules[i].antecedent.Length; c++)
                {
                    cc.Add(goodRules[i].antecedent[c]);
                }

                for (int c = 0; c < goodRules[i].consequent.Length; c++)
                {
                    cc.Add(goodRules[i].consequent[c]);
                }

                cc.Sort();

                if (cc.SequenceEqual(xx))
                {
                    continue;
                }

                xx = new List<int>();

                AddatoAdvices(allproducts, goodRules, advices, i);

                xx = cc;
                xx.Sort();
            }
            return advices;
        }

        private void AddatoAdvices(List<int> allproducts, List<Rule> goodRules, List<AdvicedProduct2> advices, int i)
        {
            List<int> antecedent_id = new List<int>();
            List<int> consequent_id = new List<int>();

            for (int b = 0; b < goodRules[i].consequent.Length; b++)
            {
                int cons_id = allproducts[goodRules[i].consequent[b]];
                antecedent_id.Add(cons_id);
            }

            for (int a = 0; a < goodRules[i].antecedent.Length; a++)//Submit all suggestions for the product we selected.
            {
                //consequent tells us the index of the product in allproducts, so we went and found the product id from there.
                int ant_id = allproducts[goodRules[i].antecedent[a]];
                antecedent_id.Add(ant_id);
            }

            advices.Add(new AdvicedProduct2
            {
                antecedents_id = antecedent_id
            }); //we added to the suggestions.
        }
    }
}
