using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TH_Global
{
    public static class Functions
    {

        /// <summary>
        /// Convert string to UTC DateTime (DateTime.TryParse seems to always convert to local time even with DateTimeStyle.AssumeUniveral)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static DateTime ConvertStringToUTC(string s)
        {
            string sYear = s.Substring(0, 4);
            string sMonth = s.Substring(5, 2);
            string sDay = s.Substring(8, 2);

            string sHour = s.Substring(11, 2);
            string sMinute = s.Substring(14, 2);
            string sSecond = s.Substring(17, 2);

            int year = Convert.ToInt16(sYear);
            int month = Convert.ToInt16(sMonth);
            int day = Convert.ToInt16(sDay);

            int hour = Convert.ToInt16(sHour);
            int minute = Convert.ToInt16(sMinute);
            int second = Convert.ToInt16(sSecond);

            if (s.Length > 20)
            {
                string sFraction = s.Substring(20, 6);
                int fraction = Convert.ToInt32(sFraction);
                int millisecond = fraction / 1000;
                return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
            }
            else return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }

        public static DateTime ConvertDateTimeToUTC(DateTime dt)
        {         
            int year = dt.Year;
            int month = dt.Month;
            int day = dt.Day;

            int hour = dt.Hour;
            int minute = dt.Minute;
            int second = dt.Second;
            int millisecond = dt.Millisecond;

            return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
        }
    }
}
