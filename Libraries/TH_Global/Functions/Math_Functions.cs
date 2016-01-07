using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TH_Global.Functions
{
    public static class Math_Functions
    {

        public static double GetMedian(double[] vals)
        {
            int size = vals.Length;
            int index = size / 2;
            double median = -1;

            if (vals.Length > 1)
            {
                if (IsOdd(index)) median = vals[index];
                else median = (double)(vals[index] + vals[index - 1]) / 2;
            }
            else median = vals[0];

            return median;
        }

        public static double StdDev(this double[] vals)
        {
            double result = 0;
            int count = vals.Count();
            if (count > 1)
            {
                //Compute the Average
                double avg = vals.Average();

                //Perform the Sum of (value-avg)^2
                double sum = vals.Sum(d => (d - avg) * (d - avg));

                //Put it all together
                result = Math.Sqrt(sum / count);
            }
            return result;
        }

        public static bool IsOdd(int val)
        {
            return val % 2 != 0;
        }

    }
}
