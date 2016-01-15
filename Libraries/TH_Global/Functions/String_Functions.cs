using System;
using System.Text;

namespace TH_Global.Functions
{
    public static class String_Functions
    {

        static Random random = new Random();
        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(System.Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string ToString(object o)
        {
            if (o != null) return o.ToString();
            return null;
        }

        public static string ToLower(object o)
        {
            if (o != null)
            {
                return o.ToString().ToLower();
            }
            return null;
        }

    }
}
