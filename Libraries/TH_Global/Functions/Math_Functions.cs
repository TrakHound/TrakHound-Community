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

        public static bool IsOdd(int val)
        {
            return val % 2 != 0;
        }

    }
}
