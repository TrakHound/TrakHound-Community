// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;

namespace TrakHound.Tools
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

        public static string ToFormattedString(TimeSpan ts)
        {

            if (ts >= new TimeSpan(1, 0, 0, 0))
            {
                return ts.ToString("d' Days 'h'h 'm'm 's's'");
            }
            else if (ts >= new TimeSpan(1, 0, 0))
            {
                return ts.ToString("h'h 'm'm 's's'");
            }
            else if (ts >= new TimeSpan(0, 1, 0))
            {
                return ts.ToString("m'm 's's'");
            }
            else if (ts >= new TimeSpan(0, 0, 1))
            {
                return ts.ToString("s\\.ff' Seconds'");
            }
            else
            {
                return ts.ToString("fff' Milliseconds'");
            }


        }

    }
}
