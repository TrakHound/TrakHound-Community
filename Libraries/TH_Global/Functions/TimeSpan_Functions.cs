using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TH_Global.Functions
{
    public static class TimeSpan_Functions
    {

        public static TimeSpan Parse(string s)
        {
            TimeSpan result = TimeSpan.Zero;

            if (s != null)
            {
                TimeSpan.TryParse(s, out result);
            }

            return result;
        }

        public static TimeSpan ParseSeconds(string s)
        {
            TimeSpan result = TimeSpan.Zero;

            if (s != null)
            {
                double seconds = 0;
                if (double.TryParse(s, out seconds))
                {
                    result = TimeSpan.FromSeconds(seconds);
                }
            }

            return result;
        }

    }
}
