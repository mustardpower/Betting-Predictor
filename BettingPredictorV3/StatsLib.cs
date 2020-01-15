using BettingPredictorV3.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    public static class StatsLib
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

        public static double ChiSquaredValue(List<double> actualFrequencySample, List<double> expectedFrequencySample)
        {
            if(actualFrequencySample == null || expectedFrequencySample == null)
            {
                throw new ArgumentException(Resources.SamplesMustNotBeNull);
            }

            if(actualFrequencySample.Count != expectedFrequencySample.Count)
            {
                throw new ArgumentException(Resources.SampleSizesDoNotMatch);
            }

            double chiSquared = 0.0;

            for(int x = 0; x < actualFrequencySample.Count; x++)
            {
                double observed = actualFrequencySample.ElementAt(x);
                double expected = expectedFrequencySample.ElementAt(x);
                chiSquared += Math.Pow(observed - expected, 2) / expected;
            }

            return chiSquared;
        }
    }
}
