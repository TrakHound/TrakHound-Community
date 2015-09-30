using System;

namespace TH_ShiftTable
{
    public class ShiftTime
    {
        public ShiftTime() { }
        public ShiftTime(DateTime dt)
        {
            DateTime local = dt.ToLocalTime();

            hour = local.Hour;
            minute = local.Minute;
            second = local.Second;
        }
        public ShiftTime(DateTime dt, bool convertToLocal)
        {
            if (convertToLocal)
            {
                DateTime local = dt.ToLocalTime();

                hour = local.Hour;
                minute = local.Minute;
                second = local.Second;
            }
            else
            {
                hour = dt.Hour;
                minute = dt.Minute;
                second = dt.Second;
            }
        }

        public static bool TryParse(string text, out ShiftTime time)
        {
            return TryParse(text, out time, 0);
        }

        public static bool TryParse(string text, out ShiftTime time, int dayOffset)
        {
            bool Result = false;
            time = null;

            if (text.Contains(":"))
            {
                int colon = text.IndexOf(':');

                int h = -1;
                int m = -1;

                bool pm = false;

                // Get AM or PM
                if (text.Contains("AM")) { text.Substring(text.IndexOf("AM"), 2); }
                if (text.Contains("am")) { text.Substring(text.IndexOf("am"), 2); }
                if (text.Contains("PM")) { text.Substring(text.IndexOf("PM"), 2); pm = true; }
                if (text.Contains("pm")) { text.Substring(text.IndexOf("pm"), 2); pm = true; }

                // Hour and Minute
                string sHour = text.Substring(0, colon);
                string sMinute = text.Substring(colon + 1, 2);

                if (int.TryParse(sHour, out h) && int.TryParse(sMinute, out m))
                {
                    time = new ShiftTime();
                    time.hour = h;
                    time.minute = m;
                    time.second = 0;

                    time.dayOffset = dayOffset;

                    if (pm) time.hour += 12;

                    Result = true;
                }
            }

            return Result;
        }

        public int hour { get; set; }
        public int minute { get; set; }
        public int second { get; set; }

        public int dayOffset { get; set; }

        public override string ToString()
        {
            return hour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00");
        }

        public string ToString(bool Hour12)
        {
            int adjHour = hour;

            string meridian = "AM";
            if (hour > 12)
            {
                meridian = "PM";
                adjHour -= 12;
            }

            return adjHour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00") + " " + meridian;
        }

        //public ShiftTime AdjustForDayValue()
        //{
        //    ShiftTime Result = new ShiftTime();
        //    Result.hour = hour + (24 * dayValue);
        //    Result.minute = minute;
        //    Result.second = second;
        //    Result.dayValue = dayValue;

        //    return Result;
        //}

        #region "Operator Overrides"

        public override bool Equals(object obj)
        {

            ShiftTime other = obj as ShiftTime;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        #region "Private"

        static bool EqualTo(ShiftTime c1, ShiftTime c2)
        {
            return c1.hour == c2.hour && c1.minute == c2.minute && c1.second == c2.second;
        }

        static bool NotEqualTo(ShiftTime c1, ShiftTime c2)
        {
            return c1.hour != c2.hour || c1.minute != c2.minute || c1.second != c2.second;
        }

        static bool LessThan(ShiftTime c1, ShiftTime c2)
        {
            // Adjust hours to account for dayValue (ex. c1.hour = 18 & c2.hour = 0 when (c1 = 6 PM & c2 = 12 AM (the next day)))
            int c1Hour = c1.hour + (24 * c1.dayOffset);
            int c2Hour = c2.hour + (24 * c2.dayOffset);

            if (c1Hour > c2Hour) return false;
            else if (c1Hour == c2Hour && c1.minute > c2.minute) return false;
            else if (c1Hour == c2Hour && c1.minute == c2.minute && c1.second >= c2.second) return false;
            else return true;
        }

        static bool GreaterThan(ShiftTime c1, ShiftTime c2)
        {
            // Adjust hours to account for dayValue (ex. c1.hour = 18 & c2.hour = 0 when (c1 = 6 PM & c2 = 12 AM (the next day)))
            int c1Hour = c1.hour + (c1.hour * c1.dayOffset);
            int c2Hour = c2.hour + (c2.hour * c2.dayOffset);

            if (c1Hour < c2Hour) return false;
            else if (c1Hour == c2Hour && c1.minute < c2.minute) return false;
            else if (c1Hour == c2Hour && c1.minute == c2.minute && c1.second <= c2.second) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(ShiftTime c1, ShiftTime c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(ShiftTime c1, ShiftTime c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(ShiftTime c1, ShiftTime c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(ShiftTime c1, ShiftTime c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(ShiftTime c1, ShiftTime c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(ShiftTime c1, ShiftTime c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }


        public static TimeSpan operator -(ShiftTime c1, ShiftTime c2)
        {
            int hour = c1.hour - c2.hour;
            int minute = c1.minute - c2.minute;
            int second = c1.second - c2.second;

            return new TimeSpan(hour, minute, second);
        }

        public static TimeSpan operator +(ShiftTime c1, ShiftTime c2)
        {
            int hour = c1.hour + c2.hour;
            int minute = c1.minute + c2.minute;
            int second = c1.second + c2.second;

            return new TimeSpan(hour, minute, second);
        }

        #endregion
    }

    public class ShiftDate
    {
        public ShiftDate() { }
        public ShiftDate(DateTime dt)
        {
            DateTime local = dt.ToLocalTime();

            year = local.Year;
            month = local.Month;
            day = local.Day;
        }

        public ShiftDate(DateTime dt, bool convertToLocal)
        {
            if (convertToLocal)
            {
                DateTime local = dt.ToLocalTime();

                year = local.Year;
                month = local.Month;
                day = local.Day;
            }
            else
            {
                year = dt.Year;
                month = dt.Month;
                day = dt.Day;
            }
        }

        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        public override string ToString()
        {
            return year.ToString() + "/" + month.ToString() + "/" + day.ToString();
        }

        public string ToString(char delimiter)
        {
            return year.ToString() + delimiter + month.ToString() + delimiter + day.ToString();
        }

        #region "operator overrides"

        public override bool Equals(object obj)
        {
            ShiftDate other = obj as ShiftDate;
            if (object.ReferenceEquals(other, null)) return false;

            if (year == other.year && month == other.month && day == other.day) return true;

            return false;
        }

        public override int GetHashCode()
        {
            int hashYear = year == null ? 0 : year.GetHashCode();
            int hashMonth = month == null ? 0 : month.GetHashCode();
            int hashDay = day == null ? 0 : day.GetHashCode();

            return hashYear ^ hashMonth ^ hashDay;
        }


        public static bool operator ==(ShiftDate sd1, ShiftDate sd2)
        {
            if (ReferenceEquals(sd1, null) && ReferenceEquals(sd2, null)) return true;
            else if (ReferenceEquals(sd1, null) && !ReferenceEquals(sd2, null)) return false;
            else if (!ReferenceEquals(sd1, null) && ReferenceEquals(sd2, null)) return false;
            else if (sd1.year == sd2.year && sd1.month == sd2.month && sd1.day == sd2.day) return true;
            else return false;
        }

        public static bool operator !=(ShiftDate sd1, ShiftDate sd2)
        {
            if (ReferenceEquals(sd1, null) && ReferenceEquals(sd2, null)) return false;
            else if (ReferenceEquals(sd1, null) && !ReferenceEquals(sd2, null)) return true;
            else if (!ReferenceEquals(sd1, null) && ReferenceEquals(sd2, null)) return true;
            if (sd1.year != sd2.year || sd1.month != sd2.month || sd1.day != sd2.day) return true;
            else return false;
        }


        public static bool operator <(ShiftDate sd1, ShiftDate sd2)
        {
            if (sd1.year > sd2.year) return false;
            else if (sd1.year == sd2.year && sd1.month > sd2.month) return false;
            else if (sd1.year == sd2.year && sd1.month == sd2.month && sd1.day >= sd2.day) return false;
            else return true;
        }

        public static bool operator >(ShiftDate sd1, ShiftDate sd2)
        {
            if (sd1.year < sd2.year) return false;
            else if (sd1.year == sd2.year && sd1.month < sd2.month) return false;
            else if (sd1.year == sd2.year && sd1.month == sd2.month && sd1.day <= sd2.day) return false;
            else return true;
        }


        public static bool operator <=(ShiftDate sd1, ShiftDate sd2)
        {
            return sd1 < sd2 || sd1 == sd2;
        }

        public static bool operator >=(ShiftDate sd1, ShiftDate sd2)
        {
            return sd1 > sd2 || sd1 == sd2;
        }

        public static int operator -(ShiftDate sd1, ShiftDate sd2)
        {
            int Result = 0;

            DateTime d1 = DateTime.MinValue;
            DateTime d2 = DateTime.MinValue;

            if (DateTime.TryParse(sd1.ToString(), out d1) && DateTime.TryParse(sd2.ToString(), out d2))
            {
                TimeSpan ts = d1 - d2;
                Result = ts.Days;
            }

            return Result;
        }

        #endregion

    }
}
