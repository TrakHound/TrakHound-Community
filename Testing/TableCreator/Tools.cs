using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableCreator
{
    public static class Tools
    {

        public static Random rnd = new Random();

        public static int[] GetRandomNumbers(int sum, int count)
        {
            var result = new int[count];

            // Get Random Numbers
            for (var x = 0; x <= count - 1; x++) result[x] = rnd.Next(0, sum);

            // Get total of generated numbers
            int total = 0;
            for (var x = 0; x <= result.Length - 1; x++) total += result[x];

            for (var x = 0; x <= result.Length - 1; x++) result[x] = Convert.ToInt32((result[x] * sum) / total);

            return result;
        }

        public static int ScaleNumber(int total, int sum, int value)
        {
            return (value * total) / sum;
        }

    }
}
