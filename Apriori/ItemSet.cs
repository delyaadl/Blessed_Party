using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Apriori
{
    public class ItemSet
    {
        public int TProd; // total jumlah produk yang ada di tabel produk
        public int k;
        public int[] data;
        public int hashValue;
        public int ct;

        public ItemSet(int N, int[] items, int ct)
        {
            this.TProd = N;
            this.k = items.Length;
            this.data = new int[this.k];
            Array.Copy(items, this.data, items.Length);
            this.hashValue = ComputeHashValue(items);
            this.ct = ct;
        }

        private static int ComputeHashValue(int[] data)//to keep the itemset like a single integer, we do reverse connection
                                                       //if itemset (0, 2, 5) is hashed as 520
        {
            int value = 0;
            int multiplier = 1;
            for (int i = 0; i < data.Length; ++i)
            {
                value = value + (data[i] * multiplier);
                if(data[i].ToString().Length == 1)
                {
                    multiplier = multiplier * 10;
                } 
                else if (data[i].ToString().Length == 2)
                {
                    multiplier = multiplier * 100;
                }
                else if (data[i].ToString().Length == 3)
                {
                    multiplier = multiplier * 1000;
                }
            }
            return value;
        }



        public bool IsSubsetOf(int[] trans)
        //Method IsSubsetOf returns true if the item-set object is a subset of a transaction:

        {
            // The trans array is sequential
            int foundIdx = -1;
            for (int j = 0; j < this.data.Length; ++j)
            {
                foundIdx = IndexOf(trans, this.data[j], 0);
                if (foundIdx == -1) return false;
            }
            return true;
        }

        //  Method IndexOf also takes advantage of ordering.
        private static int IndexOf(int[] array, int item, int startIdx)
        {
            for (int i = startIdx; i < array.Length; ++i)
            {
                if (array[i] == item) return i;
            }
            return -1;
        }

    }
}
