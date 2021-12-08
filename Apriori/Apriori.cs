using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Apriori
{
    public class Apriori
    {
        // Step 3
        public static List<ItemSet> GetFrequentItemSets(int N, List<int[]> transactions, int minSupport)
        {
            // create a List of frequent ItemSet objects that are in transactions
            // frequent means occurs in minSupportPct percent of transactions
            // N is total number of items
            // uses a variation of the Apriori algorithm

            Dictionary<int, bool> frequentDict = new Dictionary<int, bool>(); // key = int representation of an ItemSet, val = is in List of frequent ItemSet objects
            List<ItemSet> frequentList = new List<ItemSet>(); // item set objects that meet minimum count (in transactions) requirement 
            List<int> validItems = new List<int>(); // inidividual items/values at any given point in time to be used to construct new ItemSet (which may or may not meet threshhold count)

            // total tiap item muncul berapa kali di seluruh transaksi
            int[] counts = new int[N]; // index is the item/value, cell content is the count
            for (int i = 0; i < transactions.Count; ++i)
            {
                for (int j = 0; j < transactions[i].Length; ++j)
                {
                    int v = transactions[i][j];
                    ++counts[v];
                }
            }

            // nyaring itemset ke-1 yang lewat minimum support
            for (int i = 0; i < counts.Length; ++i)
            {
                if (counts[i] >= minSupport) // frequent item
                {
                    validItems.Add(i); // i is the item/value
                    int[] d = new int[1]; // the ItemSet ctor wants an array
                    d[0] = i;
                    ItemSet ci = new ItemSet(N, d, 1); // an ItemSet with size 1, ct 1
                    frequentList.Add(ci); // it's frequent
                    frequentDict.Add(ci.hashValue, true); // 
                } // else skip this item
            }

            // inisiasi variabel buat nentuin kapan berhenti looping
            // selama masih kebentuk itemset baru, berarti tetap false, tapi di skripsi dikasih batasan 3 itemset doang maks
            // jadi kalo udah 3 itemset kelar semua kebentuk, maka looping bakal diberhentiin
            bool done = false;

            // looping bikin itemset mulai dari 2 itemset
            for (int k = 2; done == false; ++k)
            {
                done = true; // assume no new item-sets will be created
                int numFreq = frequentList.Count; // List size modified so store first

                for (int i = 0; i < numFreq; ++i) // use existing frequent item-sets to create new freq item-sets with size+1
                {
                    // notes : keknya si k buat nyimpen jumkah item dalam itemset? jadi kalo cuma ada 1 item, k nya 1, kalo ada 2 item k nya 2, dst.
                    // terus kalo ada yang k nya lebih dari itemset yang sekarang - 1, itu juga gadipake
                    // frequentList itu gak ke reset, jadi kalo udah k = 3, itemset 1 masih ada di dalem situ, jadi yang dicari yang itemset 2 aja
                    if (frequentList[i].k != k - 1) continue; // only use those ItemSet objects with size 1 less than new ones being created

                    for (int j = 0; j < validItems.Count; ++j)
                    {
                        int[] newData = new int[k]; // data for a new item-set

                        // masukin item dari itemset sebelumnya dulu
                        for (int p = 0; p < k - 1; ++p)
                            newData[p] = frequentList[i].data[p];

                        // kayanya ini tiap item dari k-2 kebelakang, itu udah item lama, jadi gaperlu dilanjut lagi
                        if(k == 2)
                        {
                            if (validItems[j] <= newData[k - k]) continue; // because item-values are in order we can skip sometimes
                        } else
                        {
                            if (validItems[j] <= newData[k - k] || validItems[j] == newData[k - (k-1)]) continue;
                        }

                        newData[k - 1] = validItems[j]; // new item-value
                        ItemSet ci = new ItemSet(N, newData, -1); // ct to be determined

                        if (frequentDict.ContainsKey(ci.hashValue) == true) // this new ItemSet has already been added
                            continue;
                        int ct = CountTimesInTransactions(ci, transactions); // ngitung total transaksi yang mengandung item-item itemset baru dalam 1 transaksi
                        if (ct >= minSupport) // we have a winner!
                        {
                            ci.ct = ct; // now we know the ct
                            frequentList.Add(ci);
                            frequentDict.Add(ci.hashValue, true);
                            done = false; // a new item-set was created, so we're not done
                        }
                    } // j
                } // i

                // update valid items -- quite subtle
                validItems.Clear();
                Dictionary<int, bool> validDict = new Dictionary<int, bool>(); // track new list of valid items
                for (int idx = 0; idx < frequentList.Count; ++idx)
                {
                    if (frequentList[idx].k != k) continue; // only looking at the just-created item-sets
                    for (int j = 0; j < frequentList[idx].data.Length; ++j)
                    {
                        int v = frequentList[idx].data[j]; // item
                        if (validDict.ContainsKey(v) == false)
                        {
                            //Console.WriteLine("adding " + v + " to valid items list");
                            validItems.Add(v);
                            validDict.Add(v, true);
                        }
                    }
                }
                validItems.Sort(); // keep valid item-values ordered so item-sets will always be ordered

                if(k == 3)
                {
                    done = true;
                }
            } // next k

            // transfer to return result, filtering by minItemSetCount
            List<ItemSet> result = new List<ItemSet>();
            List<int> lengths = new List<int>();

            for (int i = 0; i < frequentList.Count; ++i)
            {
                lengths.Add(frequentList[i].k);
                lengths.Sort();

            }
            for (int i = 0; i < frequentList.Count; ++i)
            {
                if (frequentList[i].k == 2 || frequentList[i].k == 3)
                    result.Add(new ItemSet(frequentList[i].TProd, frequentList[i].data, frequentList[i].ct));
            }

            return result;
        }

        private static int CountTimesInTransactions(ItemSet itemSet, List<int[]> transactions)
        {
            // number of times itemSet occurs in transactions
            int ct = 0;
            for (int i = 0; i < transactions.Count; ++i)
            {
                if (itemSet.IsSubsetOf(transactions[i]) == true)
                    ++ct;
            }
            return ct;
        }

        public static List<Rule> GetHighConfRules(List<ItemSet> freqItemSets, List<int[]> trans, double minConfidencePct)
        {
            // generate candidate rules from freqItemSets, save rules that meet min confidence against transactions
            List<Rule> result = new List<Rule>(); // yang bakal di return

            Dictionary<int[], int> itemSetCountDict = new Dictionary<int[], int>(); // count of item sets

            for (int i = 0; i < freqItemSets.Count; ++i) // each freq item-set generates multiple candidate rules
            {
                int[] currItemSet = freqItemSets[i].data; // for clarity only

                int ctItemSet = freqItemSets[i].ct;
                // int ctItemSet = CountInTrans(currItemSet, trans, itemSetCountDict); // needed for each candidate rule

                for (int len = 1; len <= currItemSet.Length - 1; ++len) // antecedent len = 1, 2, 3, . .
                {
                    int[] c = NewCombination(len); // nyimpen index kombinasi, yang mana bakal jadi ante indexnya disimpen disini, kalo conse yang selain disini

                    while (c != null) // each combination makes a candidate rule
                    {
                        int[] ante = MakeAntecedent(currItemSet, c);
                        int[] cons = MakeConsequent(currItemSet, c); // could defer this until known if needed
                      
                        int ctAntecendent = CountInTrans(ante, trans, itemSetCountDict); // use lookup if possible 
                        int ctConsequent = CountInTrans(cons, trans, itemSetCountDict); // use lookup if possible 

                        //double supportAnB = double.Parse((decimal.Divide(ctItemSet, trans.Count)).ToString());
                        //double supportA = double.Parse((decimal.Divide(ctAntecendent, trans.Count)).ToString());
                        //double supportB = double.Parse((decimal.Divide(ctConsequent, trans.Count)).ToString());

                        double expected_confidence = double.Parse((decimal.Divide(ctConsequent, trans.Count)).ToString());
                        double confidence = double.Parse((decimal.Divide(ctItemSet, ctAntecendent)).ToString());
                        double lift_ratio = double.Parse((decimal.Divide((decimal)confidence, (decimal)expected_confidence)).ToString());

                        if (confidence >= minConfidencePct && lift_ratio > 1) // we have a winner!
                        {
                            Rule r = new Rule(ante, cons, Math.Round(confidence, 3), Math.Round(lift_ratio, 3));
                            result.Add(r); // if freq item-sets are distinct, no dup rules ever created
                        }
                        c = NextCombination(c, currItemSet.Length); // buat ngebalik kombinasi, aturannya kan ngikutin antecedent, jadi kalo dibalik, berubah lg valuenya, jadi semua kebagian jadi ante dan conse
                    } // while each combination
                } // len each possible antecedent for curr item-set
            } // i each freq item-set

            return result;
        } // GetHighConfRules

        static int[] NewCombination(int k)
        {
            // if k = 3, return is (0 1 2). n is external somewhere
            int[] result = new int[k];
            for (int i = 0; i < result.Length; ++i)
                result[i] = i;
            return result;
        }

        static int[] NextCombination(int[] comb, int n)
        {
            // if n = 5, combination = (0 3 4) next is (1 2 3)
            // if n = 5, combination = (3 4 5) next is null
            int[] result = new int[comb.Length];
            int k = comb.Length;

            if (comb[0] == n - k) return null;
            Array.Copy(comb, result, comb.Length);
            int i = k - 1;
            while (i > 0 && result[i] == n - k + i)
                --i;
            ++result[i];
            for (int j = i; j < k - 1; ++j)
                result[j + 1] = result[j] + 1;
            return result;
        }

        static int[] MakeAntecedent(int[] itemSet, int[] comb)
        {
            // combination itu list indexnya, kalo combinationnya 0 2 berarti yang diambil value di index 0 dan 2 jadi antecedentnya
            // if item-set = (1 3 4 6 8) and combination = (0 2) 
            // then antecedent = (1 4)
            int[] result = new int[comb.Length];
            for (int i = 0; i < comb.Length; ++i)
            {
                int idx = comb[i];
                result[i] = itemSet[idx];
            }
            return result;
        }

        static int[] MakeConsequent(int[] itemSet, int[] comb)
        {
            // ini kebalikan dari antecedent, kalo misal comb 0 2 berarti yang diambil yang selain itu, jadi consequent
            // if item-set = (1 3 4 6 8) and combination = (0 2) 
            // then consequent = (3 6 8)
            int[] result = new int[itemSet.Length - comb.Length];
            int j = 0; // ptr into combination
            int p = 0; // ptr into result
            for (int i = 0; i < itemSet.Length; ++i)
            {
                if (j < comb.Length && i == comb[j]) // we are at an antecedent
                    ++j; // so continue
                else
                    result[p++] = itemSet[i]; // at a consequent so add it
            }
            return result;
        }

        // ngecek berapa kali si itemset muncul dalam suatu transaksi
        static int CountInTrans(int[] itemSet, List<int[]> trans, Dictionary<int[], int> countDict)
        {
            // number of times itemSet occurs in transactions, using a lookup dict
            if (countDict.ContainsKey(itemSet) == true)
                return countDict[itemSet]; // use already computed count

            int ct = 0;
            for (int i = 0; i < trans.Count; ++i)
                if (IsSubsetOf(itemSet, trans[i]) == true)
                    ++ct;
            countDict.Add(itemSet, ct); // notes : tanya, ini kok variabel local tapi diassign dari fungsi lain dia langsung ngikut ngisi ada itemnya nambah
            return ct;
        }

        static bool IsSubsetOf(int[] itemSet, int[] trans)
        {
            // 'trans' is an ordered transaction like [0 1 4 5 8]
            int foundIdx = -1;
            for (int j = 0; j < itemSet.Length; ++j)
            {
                foundIdx = IndexOf(trans, itemSet[j], 0);
                if (foundIdx == -1) return false;
            }
            return true;
        }

        static int IndexOf(int[] array, int item, int startIdx)
        {
            for (int i = startIdx; i < array.Length; ++i)
            {
                // if (i > item) return -1; // i is past where the target could possibly be
                if (array[i] == item) return i;
            }
            return -1;
        }
    }
}
