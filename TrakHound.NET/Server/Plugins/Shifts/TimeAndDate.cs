// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TrakHound.Server.Plugins.Shifts
{
    public class ShiftTime : IComparable
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

                    if (pm) time.hour += 12;

                    Result = true;
                }
            }

            return Result;
        }

        public int hour { get; set; }
        public int minute { get; set; }
        public int second { get; set; }

        public int dayOffset
        {
            get
            {
                double span = (double)hour / 24;
                double floor = Math.Floor(span);

                return Convert.ToInt32(floor);
            }
        }

        public int adjHour
        {
            get
            {
                return hour - dayOffset * 24;
            }
        }

        public ShiftTime ToUTC()
        {
            TimeZone zone = TimeZone.CurrentTimeZone;
            DateTime time = new DateTime(1, 1, 1, adjHour, minute, second);
            TimeSpan offset = zone.GetUtcOffset(time);

            var result = new ShiftTime();
            result.hour = hour - offset.Hours; // Offset is given as negative number
            if (result.hour < 0) result.hour = 24 - result.hour;
            result.minute = minute - offset.Minutes;
            result.second = second = offset.Seconds;

            return result;
        }

        public string To24HourString()
        {
            ShiftTime result = new ShiftTime();
            result.hour = hour;
            result.minute = minute;
            result.second = second;

            if (result.hour >= 24) result.hour -= dayOffset * 24;

            string h = result.hour.ToString("00");
            string m = result.minute.ToString("00");
            string s = result.second.ToString("00");

            return h + ":" + m + ":" + s;
        }

        public string ToFullString()
        {
            ShiftTime result = new ShiftTime();
            result.hour = hour;
            result.minute = minute;
            result.second = second;

            string h = result.hour.ToString("00");
            string m = result.minute.ToString("00");
            string s = result.second.ToString("00");

            return h + ":" + m + ":" + s;
        }

        public override string ToString()
        {
            //int adjHour = hour;
            int adjHour = (hour - (dayOffset * 24));

            string meridian = "AM";
            if (adjHour == 0)
            {
                adjHour = 12;
            }
            else if (adjHour == 12)
            {
                meridian = "PM";
            }
            else if (adjHour > 12)
            {
                meridian = "PM";
                adjHour -= 12;
            }

            return adjHour.ToString() + ":" + minute.ToString("00") + ":" + second.ToString("00") + " " + meridian;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            ShiftTime s = obj as ShiftTime;
            if (s != null)
            {
                if (s > this) return -1;
                else if (s < this) return 1;
                else return 0;
            }
            else return 1;
        }

        public ShiftTime Copy()
        {
            if (this != null)
            {
                ShiftTime result = new ShiftTime();
                result.hour = hour;
                result.minute = minute;
                result.second = second;

                return result;
            }
            else return null;
        }

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
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.hour == c2.hour && c1.minute == c2.minute && c1.second == c2.second;
        }

        static bool NotEqualTo(ShiftTime c1, ShiftTime c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.hour != c2.hour || c1.minute != c2.minute || c1.second != c2.second;
        }

        static bool LessThan(ShiftTime c1, ShiftTime c2)
        {
            int c1Hour = c1.hour;
            int c2Hour = c2.hour;

            if (c1Hour > c2Hour) return false;
            else if (c1Hour == c2Hour && c1.minute > c2.minute) return false;
            else if (c1Hour == c2Hour && c1.minute == c2.minute && c1.second >= c2.second) return false;
            else return true;
        }

        static bool GreaterThan(ShiftTime c1, ShiftTime c2)
        {
            int c1Hour = c1.hour;
            int c2Hour = c2.hour;

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

        public ShiftTime AddShiftTime(ShiftTime time)
        {
            ShiftTime result = new ShiftTime();

            int h = hour + time.hour;

            int m = minute + time.minute;
            if (m > 59) { h += 1; m = m - 60; }

            int s = second + time.second;
            if (s > 59) { m += 1; s = s - 60; }

            //if (second > 59)
            //{
            //    int m = 0;
            //    int s = second - 60;
            //    while (s > 59)
            //    {
            //        m += 1;
            //        s = second - 60;
            //    }

            //    minute += m;
            //    second = s;
            //}

            //if (minute > 59)
            //{
            //    int h = 0;
            //    int m = minute - 60;
            //    while (m > 59)
            //    {
            //        h += 1;
            //        m = second - 60;
            //    }

            //    hour += h;
            //    minute = m;
            //}

            result.hour = h;
            result.minute = m;
            result.second = s;

            return result;
        }
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
            Source = local;
        }

        public ShiftDate(DateTime dt, bool convertToLocal)
        {
            if (convertToLocal)
            {
                DateTime local = dt.ToLocalTime();

                year = local.Year;
                month = local.Month;
                day = local.Day;
                Source = local;
            }
            else
            {
                year = dt.Year;
                month = dt.Month;
                day = dt.Day;
                Source = dt;
            }
        }

        public DateTime Source { get; set; }

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

        public static ShiftDate operator -(ShiftDate sd1, int days)
        {
            ShiftDate Result = sd1;

            DateTime d1 = DateTime.MinValue;

            if (DateTime.TryParse(sd1.ToString(), out d1))
            {
                d1 = d1.Subtract(TimeSpan.FromDays(days));
                Result = new ShiftDate(d1, false);
            }

            return Result;
        }

        #endregion

    }
}
