using Blessed_Party.Models;
using Blessed_Party.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.CollaborativeFiltering
{
    public class CollaFil
    {
        private readonly Data.BPartyContext _context;

        public CollaFil(Data.BPartyContext context)
        {
            this._context = context;
        }

        public IList<tbl_User> tbl_User { get; set; }
        public IList<tbl_Product> tbl_Product { get; set; }
        public IList<tbl_Rating_Product> tbl_Rating_Product { get; set; }
        public IList<tbl_Prediction> tbl_Prediction { get; set; }
        

        public async Task<List<ItemRating>> RatingAvg()
        {
            List<ItemRating> RatingAvgList = new List<ItemRating>();
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_User = await _context.tbl_User.ToListAsync();

            foreach (var item in tbl_Product)
            {
                tbl_Rating_Product = await _context.tbl_Rating_Product.Where(x => x.product_id == item.product_id).ToListAsync();
                int divisor = tbl_Rating_Product.Count;
                var rating_item_sum_i = _context.tbl_Rating_Product.Where(x => x.product_id == item.product_id).Sum(x => x.rating_number);
                ItemRating sumRating = new ItemRating();
                sumRating.product_id = item.product_id;
                if (divisor != 0)
                {
                    sumRating.rating_total = rating_item_sum_i / divisor;
                }
                else
                {
                    sumRating.rating_total = 0;
                }

                RatingAvgList.Add(sumRating);
            }

            return RatingAvgList;
        }

        public async Task<List<Similarity>> PearsonCorrelation()
        {
            
            List<ItemRating> ratingAvg = await RatingAvg();
            var products = await _context.tbl_Product.Select(x => x.product_id).ToListAsync();
            tbl_Rating_Product = await _context.tbl_Rating_Product.OrderBy(x => x.user_id).ToListAsync();
            var users = await _context.tbl_User.Where(x => _context.tbl_Rating_Product.Select(x => x.user_id).Distinct().Contains(x.user_id)).Select(x => x.user_id).ToListAsync();
            List<Similarity> similarities = new List<Similarity>();

            var pairs = (from i in products
                         from j in products
                         from k in users
                         where i != j &&
                         (from tbl_rating in tbl_Rating_Product where (tbl_rating.user_id == k && tbl_rating.product_id == i) || (tbl_rating.user_id == k && tbl_rating.product_id == j) select tbl_rating.product_id).ToList().Count == 2 
                         select Tuple.Create(i, j, k)).OrderBy(x => x.Item1).ToList();

            int temp_product_1 = 0;
            int temp_product_2 = 0;
            int count = 0;
            decimal? pearsonr = 0;
            decimal? fungsi_atas = 0;
            decimal? fungsi_bawah = 0;
            decimal? akar_i = 0;
            decimal? akar_j = 0;

            foreach (var item in pairs)
            {

                if(count == 0)
                {
                    temp_product_1 = item.Item1;
                    temp_product_2 = item.Item2;
                    count++;
                }

                if(temp_product_1 != item.Item1 || temp_product_2 != item.Item2)
                {
                    if (pearsonr != 0)
                    {
                        Similarity sim = new Similarity();
                        sim.product_1 = temp_product_1;
                        sim.product_2 = temp_product_2;
                        sim.similarityScore = Math.Round((decimal)pearsonr, 3);
                        similarities.Add(sim);
                    }


                    pearsonr = 0;
                    fungsi_atas = 0;
                    fungsi_bawah = 0;
                    akar_i = 0;
                    akar_j = 0;

                    var res1 = _context.tbl_Rating_Product.Where(x => x.product_id == item.Item1 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;
                    var res2 = _context.tbl_Rating_Product.Where(x => x.product_id == item.Item2 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                    if (res1 != 0 && res2 != 0)
                    {
                        decimal? rating_res1 = res1 - ratingAvg.Where(vv => vv.product_id == item.Item1).Select(vv => vv.rating_total).FirstOrDefault();
                        decimal? rating_res2 = res2 - ratingAvg.Where(vv => vv.product_id == item.Item2).Select(vv => vv.rating_total).FirstOrDefault();
                        fungsi_atas = fungsi_atas + (rating_res1 * rating_res2);
                        akar_i = akar_i + (rating_res1 * rating_res1);
                        akar_j = akar_j + (rating_res2 * rating_res2);
                        fungsi_bawah = decimal.Parse((Math.Sqrt(double.Parse(akar_i.ToString())) * Math.Sqrt(double.Parse(akar_j.ToString()))).ToString());

                        if (fungsi_bawah != 0)
                        {
                            pearsonr = fungsi_atas / fungsi_bawah;
                        }
                    }
                }
                else
                {
                    var res1 = _context.tbl_Rating_Product.Where(x => x.product_id == item.Item1 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;
                    var res2 = _context.tbl_Rating_Product.Where(x => x.product_id == item.Item2 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                    if (res1 != 0 && res2 != 0)
                    {
                        decimal? rating_res1 = res1 - ratingAvg.Where(vv => vv.product_id == item.Item1).Select(vv => vv.rating_total).FirstOrDefault();
                        decimal? rating_res2 = res2 - ratingAvg.Where(vv => vv.product_id == item.Item2).Select(vv => vv.rating_total).FirstOrDefault();
                        fungsi_atas = fungsi_atas + (rating_res1 * rating_res2);
                        akar_i = akar_i + (rating_res1 * rating_res1);
                        akar_j = akar_j + (rating_res2 * rating_res2);
                        fungsi_bawah = decimal.Parse((Math.Sqrt(double.Parse(akar_i.ToString())) * Math.Sqrt(double.Parse(akar_j.ToString()))).ToString());

                        if (fungsi_bawah != 0)
                        {
                            pearsonr = fungsi_atas / fungsi_bawah;
                        }
                    }
                }

                temp_product_1 = item.Item1;
                temp_product_2 = item.Item2;
            }

            return similarities;
        }

        public async Task Prediction(int user_id)
        {
            _context.tbl_Prediction.RemoveRange(_context.tbl_Prediction.Where(x => x.user_id == user_id));
            await _context.SaveChangesAsync();

            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_User = await _context.tbl_User.ToListAsync();
            List<Similarity> similarItem = await PearsonCorrelation();
            List<PredictedItem> predictedItems = new List<PredictedItem>();

            double prediksi = 0;
            decimal? fungsi_atas = 0;
            decimal? fungsi_bawah = 0;
            var temp_id = 0;
            int count = 0;
            foreach (var item in similarItem)
            {
                if (count == 0)
                {
                    temp_id = item.product_1;
                }

                if (count == similarItem.Count - 1)
                {
                    if (item.product_1 != temp_id)
                    {
                        prediksi = 0;
                        fungsi_atas = 0;
                        fungsi_bawah = 0;

                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_Product.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }

                        if (fungsi_bawah != 0)
                        {
                            prediksi = double.Parse(decimal.Divide(decimal.Parse(fungsi_atas.ToString()), decimal.Parse(fungsi_bawah.ToString())).ToString());
                        }

                        if (prediksi != 0)
                        {
                            tbl_Prediction newAdd = new tbl_Prediction();
                            newAdd.product_id = item.product_1;
                            newAdd.user_id = user_id;
                            newAdd.prediction_score = Math.Round((decimal)prediksi, 3);
                            newAdd.created_date = DateTime.Now;

                            _context.tbl_Prediction.Add(newAdd);
                            await _context.SaveChangesAsync();

                            //PredictedItem PredictedItem = new PredictedItem();
                            //PredictedItem.product_predict = item.product_1;
                            //PredictedItem.predictionScore = prediksi;

                            //predictedItems.Add(PredictedItem);
                        }
                    }
                    else
                    {
                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_Product.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }

                        if (fungsi_bawah != 0)
                        {
                            prediksi = double.Parse(decimal.Divide(decimal.Parse(fungsi_atas.ToString()), decimal.Parse(fungsi_bawah.ToString())).ToString());
                        }

                        if (prediksi != 0)
                        {
                            tbl_Prediction newAdd = new tbl_Prediction();
                            newAdd.product_id = item.product_1;
                            newAdd.user_id = user_id;
                            newAdd.prediction_score = Math.Round((decimal)prediksi, 3);
                            newAdd.created_date = DateTime.Now;

                            _context.tbl_Prediction.Add(newAdd);
                            await _context.SaveChangesAsync();
                        }

                    }
                }
                else
                {
                    if (item.product_1 != temp_id)
                    {
                        if (fungsi_bawah != 0)
                        {
                            prediksi = double.Parse(decimal.Divide(decimal.Parse(fungsi_atas.ToString()), decimal.Parse(fungsi_bawah.ToString())).ToString());
                        }

                        if (prediksi != 0)
                        {
                            tbl_Prediction newAdd = new tbl_Prediction();
                            newAdd.product_id = temp_id;
                            newAdd.user_id = user_id;
                            newAdd.prediction_score = Math.Round((decimal)prediksi, 3);
                            newAdd.created_date = DateTime.Now;

                            _context.tbl_Prediction.Add(newAdd);
                            await _context.SaveChangesAsync();
                        }

                        prediksi = 0;
                        fungsi_atas = 0;
                        fungsi_bawah = 0;

                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_Product.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }
                    }
                    else
                    {
                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_Product.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }

                    }
                }

                count++;
                temp_id = item.product_1;
            }
        }


        // ===============================================================================
        // 40 user 
        // ===============================================================================

        public IList<tbl_Rating_40_User> tbl_Rating_40_User { get; set; }
        public IList<tbl_Prediction40> tbl_Prediction40 { get; set; }


        public async Task<List<ItemRating>> RatingAvg40()
        {
            List<ItemRating> RatingAvgList = new List<ItemRating>();
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_User = await _context.tbl_User.ToListAsync();

            foreach (var item in tbl_Product)
            {
                tbl_Rating_40_User = await _context.tbl_Rating_40_User.Where(x => x.product_id == item.product_id).ToListAsync();
                int divisor = tbl_Rating_40_User.Count;
                var rating_item_sum_i = _context.tbl_Rating_40_User.Where(x => x.product_id == item.product_id).Sum(x => x.rating_number);
                ItemRating sumRating = new ItemRating();
                sumRating.product_id = item.product_id;
                if (divisor != 0)
                {
                    sumRating.rating_total = rating_item_sum_i / divisor;
                }
                else
                {
                    sumRating.rating_total = 0;
                }

                RatingAvgList.Add(sumRating);
            }

            return RatingAvgList;
        }

        public async Task<List<Similarity>> PearsonCorrelation40()
        {

            List<ItemRating> ratingAvg = await RatingAvg();
            var products = await _context.tbl_Product.Select(x => x.product_id).ToListAsync();
            tbl_Rating_40_User = await _context.tbl_Rating_40_User.OrderBy(x => x.user_id).ToListAsync();
            var users = await _context.tbl_User.Where(x => _context.tbl_Rating_40_User.Select(x => x.user_id).Distinct().Contains(x.user_id)).Select(x => x.user_id).ToListAsync();
            List<Similarity> similarities = new List<Similarity>();

            var pairs = (from i in products
                         from j in products
                         from k in users
                         where i != j &&
                         (from tbl_rating in tbl_Rating_40_User where (tbl_rating.user_id == k && tbl_rating.product_id == i) || (tbl_rating.user_id == k && tbl_rating.product_id == j) select tbl_rating.product_id).ToList().Count == 2
                         select Tuple.Create(i, j, k)).OrderBy(x => x.Item1).ToList();

            int temp_product_1 = 0;
            int temp_product_2 = 0;
            int count = 0;
            decimal? pearsonr = 0;
            decimal? fungsi_atas = 0;
            decimal? fungsi_bawah = 0;
            decimal? akar_i = 0;
            decimal? akar_j = 0;

            foreach (var item in pairs)
            {

                if (count == 0)
                {
                    temp_product_1 = item.Item1;
                    temp_product_2 = item.Item2;
                    count++;
                }

                if (temp_product_1 != item.Item1 || temp_product_2 != item.Item2)
                {
                    if (pearsonr != 0)
                    {
                        Similarity sim = new Similarity();
                        sim.product_1 = temp_product_1;
                        sim.product_2 = temp_product_2;
                        sim.similarityScore = Math.Round((decimal)pearsonr, 3);
                        similarities.Add(sim);
                    }


                    pearsonr = 0;
                    fungsi_atas = 0;
                    fungsi_bawah = 0;
                    akar_i = 0;
                    akar_j = 0;

                    var res1 = _context.tbl_Rating_40_User.Where(x => x.product_id == item.Item1 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;
                    var res2 = _context.tbl_Rating_40_User.Where(x => x.product_id == item.Item2 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                    if (res1 != 0 && res2 != 0)
                    {
                        decimal? rating_res1 = res1 - ratingAvg.Where(vv => vv.product_id == item.Item1).Select(vv => vv.rating_total).FirstOrDefault();
                        decimal? rating_res2 = res2 - ratingAvg.Where(vv => vv.product_id == item.Item2).Select(vv => vv.rating_total).FirstOrDefault();
                        fungsi_atas = fungsi_atas + (rating_res1 * rating_res2);
                        akar_i = akar_i + (rating_res1 * rating_res1);
                        akar_j = akar_j + (rating_res2 * rating_res2);
                        fungsi_bawah = decimal.Parse((Math.Sqrt(double.Parse(akar_i.ToString())) * Math.Sqrt(double.Parse(akar_j.ToString()))).ToString());

                        if (fungsi_bawah != 0)
                        {
                            pearsonr = fungsi_atas / fungsi_bawah;
                        }
                    }
                }
                else
                {
                    var res1 = _context.tbl_Rating_40_User.Where(x => x.product_id == item.Item1 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;
                    var res2 = _context.tbl_Rating_40_User.Where(x => x.product_id == item.Item2 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                    if (res1 != 0 && res2 != 0)
                    {
                        decimal? rating_res1 = res1 - ratingAvg.Where(vv => vv.product_id == item.Item1).Select(vv => vv.rating_total).FirstOrDefault();
                        decimal? rating_res2 = res2 - ratingAvg.Where(vv => vv.product_id == item.Item2).Select(vv => vv.rating_total).FirstOrDefault();
                        fungsi_atas = fungsi_atas + (rating_res1 * rating_res2);
                        akar_i = akar_i + (rating_res1 * rating_res1);
                        akar_j = akar_j + (rating_res2 * rating_res2);
                        fungsi_bawah = decimal.Parse((Math.Sqrt(double.Parse(akar_i.ToString())) * Math.Sqrt(double.Parse(akar_j.ToString()))).ToString());

                        if (fungsi_bawah != 0)
                        {
                            pearsonr = fungsi_atas / fungsi_bawah;
                        }
                    }
                }

                temp_product_1 = item.Item1;
                temp_product_2 = item.Item2;
            }

            return similarities;
        }

        public async Task Prediction40(int user_id)
        {
            _context.tbl_Prediction40.RemoveRange(_context.tbl_Prediction40.Where(x => x.user_id == user_id));
            await _context.SaveChangesAsync();

            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_User = await _context.tbl_User.ToListAsync();
            List<Similarity> similarItem = await PearsonCorrelation40();
            List<PredictedItem> predictedItems = new List<PredictedItem>();

            double prediksi = 0;
            decimal? fungsi_atas = 0;
            decimal? fungsi_bawah = 0;
            var temp_id = 0;
            int count = 0;
            foreach (var item in similarItem)
            {
                if (count == 0)
                {
                    temp_id = item.product_1;
                }

                if (count == similarItem.Count - 1)
                {
                    if (item.product_1 != temp_id)
                    {
                        prediksi = 0;
                        fungsi_atas = 0;
                        fungsi_bawah = 0;

                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_40_User.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }

                        if (fungsi_bawah != 0)
                        {
                            prediksi = double.Parse(decimal.Divide(decimal.Parse(fungsi_atas.ToString()), decimal.Parse(fungsi_bawah.ToString())).ToString());
                        }

                        if (prediksi != 0)
                        {
                            tbl_Prediction40 newAdd = new tbl_Prediction40();
                            newAdd.product_id = item.product_1;
                            newAdd.user_id = user_id;
                            newAdd.prediction_score = Math.Round((decimal)prediksi, 3);
                            newAdd.created_date = DateTime.Now;

                            _context.tbl_Prediction40.Add(newAdd);
                            await _context.SaveChangesAsync();

                            //PredictedItem PredictedItem = new PredictedItem();
                            //PredictedItem.product_predict = item.product_1;
                            //PredictedItem.predictionScore = prediksi;

                            //predictedItems.Add(PredictedItem);
                        }
                    }
                    else
                    {
                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_40_User.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }

                        if (fungsi_bawah != 0)
                        {

                            prediksi = double.Parse(decimal.Divide(decimal.Parse(fungsi_atas.ToString()), decimal.Parse(fungsi_bawah.ToString())).ToString());
                        }

                        if (prediksi != 0)
                        {
                            tbl_Prediction40 newAdd = new tbl_Prediction40();
                            newAdd.product_id = item.product_1;
                            newAdd.user_id = user_id;
                            newAdd.prediction_score = Math.Round((decimal)prediksi, 3);
                            newAdd.created_date = DateTime.Now;

                            _context.tbl_Prediction40.Add(newAdd);
                            await _context.SaveChangesAsync();
                        }

                    }
                }
                else
                {
                    if (item.product_1 != temp_id)
                    {
                        if (fungsi_bawah != 0)
                        {
                            prediksi = double.Parse(decimal.Divide(decimal.Parse(fungsi_atas.ToString()), decimal.Parse(fungsi_bawah.ToString())).ToString());
                        }

                        if (prediksi != 0)
                        {
                            tbl_Prediction40 newAdd = new tbl_Prediction40();
                            newAdd.product_id = temp_id;
                            newAdd.user_id = user_id;
                            newAdd.prediction_score = Math.Round((decimal)prediksi, 3);
                            newAdd.created_date = DateTime.Now;

                            _context.tbl_Prediction40.Add(newAdd);
                            await _context.SaveChangesAsync();
                        }

                        prediksi = 0;
                        fungsi_atas = 0;
                        fungsi_bawah = 0;

                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_40_User.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }
                    }
                    else
                    {
                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_40_User.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }

                    }
                }

                count++;
                temp_id = item.product_1;
            }
        }

        // ===============================================================================
        // 80 user 
        // ===============================================================================

        public IList<tbl_Rating_80_User> tbl_Rating_80_User { get; set; }
        public IList<tbl_Prediction80> tbl_Prediction80 { get; set; }


        public async Task<List<ItemRating>> RatingAvg80()
        {
            List<ItemRating> RatingAvgList = new List<ItemRating>();
            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_User = await _context.tbl_User.ToListAsync();

            foreach (var item in tbl_Product)
            {
                tbl_Rating_80_User = await _context.tbl_Rating_80_User.Where(x => x.product_id == item.product_id).ToListAsync();
                int divisor = tbl_Rating_80_User.Count;
                var rating_item_sum_i = _context.tbl_Rating_80_User.Where(x => x.product_id == item.product_id).Sum(x => x.rating_number);
                ItemRating sumRating = new ItemRating();
                sumRating.product_id = item.product_id;
                if (divisor != 0)
                {
                    sumRating.rating_total = rating_item_sum_i / divisor;
                }
                else
                {
                    sumRating.rating_total = 0;
                }

                RatingAvgList.Add(sumRating);
            }

            return RatingAvgList;
        }

        public async Task<List<Similarity>> PearsonCorrelation80()
        {

            List<ItemRating> ratingAvg = await RatingAvg();
            var products = await _context.tbl_Product.Select(x => x.product_id).ToListAsync();
            tbl_Rating_80_User = await _context.tbl_Rating_80_User.OrderBy(x => x.user_id).ToListAsync();
            var users = await _context.tbl_User.Where(x => _context.tbl_Rating_80_User.Select(x => x.user_id).Distinct().Contains(x.user_id)).Select(x => x.user_id).ToListAsync();
            List<Similarity> similarities = new List<Similarity>();

            var pairs = (from i in products
                         from j in products
                         from k in users
                         where i != j &&
                         (from tbl_rating in tbl_Rating_80_User where (tbl_rating.user_id == k && tbl_rating.product_id == i) || (tbl_rating.user_id == k && tbl_rating.product_id == j) select tbl_rating.product_id).ToList().Count == 2
                         select Tuple.Create(i, j, k)).OrderBy(x => x.Item1).ToList();

            int temp_product_1 = 0;
            int temp_product_2 = 0;
            int count = 0;
            decimal? pearsonr = 0;
            decimal? fungsi_atas = 0;
            decimal? fungsi_bawah = 0;
            decimal? akar_i = 0;
            decimal? akar_j = 0;

            foreach (var item in pairs)
            {

                if (count == 0)
                {
                    temp_product_1 = item.Item1;
                    temp_product_2 = item.Item2;
                    count++;
                }

                if (temp_product_1 != item.Item1 || temp_product_2 != item.Item2)
                {
                    if (pearsonr != 0)
                    {
                        Similarity sim = new Similarity();
                        sim.product_1 = temp_product_1;
                        sim.product_2 = temp_product_2;
                        sim.similarityScore = Math.Round((decimal)pearsonr, 3);
                        similarities.Add(sim);
                    }


                    pearsonr = 0;
                    fungsi_atas = 0;
                    fungsi_bawah = 0;
                    akar_i = 0;
                    akar_j = 0;

                    var res1 = _context.tbl_Rating_80_User.Where(x => x.product_id == item.Item1 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;
                    var res2 = _context.tbl_Rating_80_User.Where(x => x.product_id == item.Item2 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                    if (res1 != 0 && res2 != 0)
                    {
                        decimal? rating_res1 = res1 - ratingAvg.Where(vv => vv.product_id == item.Item1).Select(vv => vv.rating_total).FirstOrDefault();
                        decimal? rating_res2 = res2 - ratingAvg.Where(vv => vv.product_id == item.Item2).Select(vv => vv.rating_total).FirstOrDefault();
                        fungsi_atas = fungsi_atas + (rating_res1 * rating_res2);
                        akar_i = akar_i + (rating_res1 * rating_res1);
                        akar_j = akar_j + (rating_res2 * rating_res2);
                        fungsi_bawah = decimal.Parse((Math.Sqrt(double.Parse(akar_i.ToString())) * Math.Sqrt(double.Parse(akar_j.ToString()))).ToString());

                        if (fungsi_bawah != 0)
                        {
                            pearsonr = fungsi_atas / fungsi_bawah;
                        }
                    }
                }
                else
                {
                    var res1 = _context.tbl_Rating_80_User.Where(x => x.product_id == item.Item1 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;
                    var res2 = _context.tbl_Rating_80_User.Where(x => x.product_id == item.Item2 && x.user_id == item.Item3).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                    if (res1 != 0 && res2 != 0)
                    {
                        decimal? rating_res1 = res1 - ratingAvg.Where(vv => vv.product_id == item.Item1).Select(vv => vv.rating_total).FirstOrDefault();
                        decimal? rating_res2 = res2 - ratingAvg.Where(vv => vv.product_id == item.Item2).Select(vv => vv.rating_total).FirstOrDefault();
                        fungsi_atas = fungsi_atas + (rating_res1 * rating_res2);
                        akar_i = akar_i + (rating_res1 * rating_res1);
                        akar_j = akar_j + (rating_res2 * rating_res2);
                        fungsi_bawah = decimal.Parse((Math.Sqrt(double.Parse(akar_i.ToString())) * Math.Sqrt(double.Parse(akar_j.ToString()))).ToString());

                        if (fungsi_bawah != 0)
                        {
                            pearsonr = fungsi_atas / fungsi_bawah;
                        }
                    }
                }

                temp_product_1 = item.Item1;
                temp_product_2 = item.Item2;
            }

            return similarities;
        }

        public async Task Prediction80(int user_id)
        {
            _context.tbl_Prediction80.RemoveRange(_context.tbl_Prediction80.Where(x => x.user_id == user_id));
            await _context.SaveChangesAsync();

            tbl_Product = await _context.tbl_Product.ToListAsync();
            tbl_User = await _context.tbl_User.ToListAsync();
            List<Similarity> similarItem = await PearsonCorrelation80();
            List<PredictedItem> predictedItems = new List<PredictedItem>();

            double prediksi = 0;
            decimal? fungsi_atas = 0;
            decimal? fungsi_bawah = 0;
            var temp_id = 0;
            int count = 0;
            foreach (var item in similarItem)
            {
                if (count == 0)
                {
                    temp_id = item.product_1;
                }

                if (count == similarItem.Count - 1)
                {
                    if (item.product_1 != temp_id)
                    {
                        prediksi = 0;
                        fungsi_atas = 0;
                        fungsi_bawah = 0;

                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_80_User.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }

                        if (fungsi_bawah != 0)
                        {
                            prediksi = double.Parse(decimal.Divide(decimal.Parse(fungsi_atas.ToString()), decimal.Parse(fungsi_bawah.ToString())).ToString());
                        }

                        if (prediksi != 0)
                        {
                            tbl_Prediction80 newAdd = new tbl_Prediction80();
                            newAdd.product_id = item.product_1;
                            newAdd.user_id = user_id;
                            newAdd.prediction_score = Math.Round((decimal)prediksi, 3);
                            newAdd.created_date = DateTime.Now;

                            _context.tbl_Prediction80.Add(newAdd);
                            await _context.SaveChangesAsync();

                            //PredictedItem PredictedItem = new PredictedItem();
                            //PredictedItem.product_predict = item.product_1;
                            //PredictedItem.predictionScore = prediksi;

                            //predictedItems.Add(PredictedItem);
                        }
                    }
                    else
                    {
                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_80_User.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }

                        if (fungsi_bawah != 0)
                        {
                            prediksi = double.Parse(decimal.Divide(decimal.Parse(fungsi_atas.ToString()), decimal.Parse(fungsi_bawah.ToString())).ToString());
                        }

                        if (prediksi != 0)
                        {
                            tbl_Prediction80 newAdd = new tbl_Prediction80();
                            newAdd.product_id = item.product_1;
                            newAdd.user_id = user_id;
                            newAdd.prediction_score = Math.Round((decimal)prediksi, 3);
                            newAdd.created_date = DateTime.Now;

                            _context.tbl_Prediction80.Add(newAdd);
                            await _context.SaveChangesAsync();
                        }

                    }
                }
                else
                {
                    if (item.product_1 != temp_id)
                    {
                        if (fungsi_bawah != 0)
                        {
                            prediksi = double.Parse(decimal.Divide(decimal.Parse(fungsi_atas.ToString()), decimal.Parse(fungsi_bawah.ToString())).ToString());
                        }

                        if (prediksi != 0)
                        {
                            tbl_Prediction80 newAdd = new tbl_Prediction80();
                            newAdd.product_id = temp_id;
                            newAdd.user_id = user_id;
                            newAdd.prediction_score = Math.Round((decimal)prediksi, 3);
                            newAdd.created_date = DateTime.Now;

                            _context.tbl_Prediction80.Add(newAdd);
                            await _context.SaveChangesAsync();
                        }

                        prediksi = 0;
                        fungsi_atas = 0;
                        fungsi_bawah = 0;

                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_80_User.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }
                    }
                    else
                    {
                        if (item.similarityScore > 0)
                        {
                            var rating_user_j = _context.tbl_Rating_80_User.Where(x => x.user_id == user_id && x.product_id == item.product_2).Select(x => x.rating_number).FirstOrDefault() ?? 0;

                            if (rating_user_j != 0)
                            {
                                fungsi_atas = fungsi_atas + (rating_user_j * item.similarityScore);
                                fungsi_bawah = fungsi_bawah + Math.Abs((decimal)item.similarityScore);
                            }
                        }

                    }
                }

                count++;
                temp_id = item.product_1;
            }
        }



    }
}
