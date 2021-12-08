using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blessed_Party.Apriori
{
    public class Rule
    {
        public int[] antecedent;//IF 1 AND 3 ARE PURCHASED TOGETHER
        public int[] consequent;//LIKE 2 ALSO RECEIVED
        public double confidence;
        public double lift_ratio;
        //antecedent=>consequent
        public Rule(int[] antecedent, int[] consequent, double confidence, double lift_ratio)
        {
            this.antecedent = new int[antecedent.Length];
            Array.Copy(antecedent, this.antecedent, antecedent.Length);
            this.consequent = new int[consequent.Length];
            Array.Copy(consequent, this.consequent, consequent.Length);
            this.confidence = confidence;
            this.lift_ratio = lift_ratio;
        }
    }
}
