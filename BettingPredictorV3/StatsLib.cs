using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    static class StatsLib
    {
        public static double poissonPDF(double lambda, int k)
        {
            return (Math.Pow(lambda, k) / factorial(k)) * Math.Exp(-lambda);
        }

        public static int factorial(int k)
        {
            int fact = k;
            if (k == 0) return 1;
            for (int i = k - 1; i >= 1; i--)
            {
                fact = fact * i;
            }

            return fact;
        }
    }
}
