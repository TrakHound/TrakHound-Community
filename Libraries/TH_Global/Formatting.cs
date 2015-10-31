// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text.RegularExpressions;

namespace TH_Global
{
    public static class Formatting
    {

        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }


        public static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());

            //char[] a = s.ToCharArray();
            //a[0] = char.ToUpper(a[0]);
            //return new string(a);
        }


        public static string ToPhoneNumber(string s)
        {
            string result = null;

            // If standard US number
            if (s.Length >= 10 && s.Length <= 12)
            {
                s = s.Replace(" ","");

                string areaCode;
                string first;
                string last;

                // No Country Code, all numbers
                if (s.Length == 10)
                {
                    areaCode = s.Substring(0, 3);
                    first = s.Substring(3, 3);
                    last = s.Substring(6, 4);
                }
                else
                {
                    int areaEnd = 0;

                    // Get area code
                    if ((s[0] == '(' || s[0] == '[') && (s[4] == ')' || s[4] == ']'))
                    {
                        areaCode = s.Substring(1, 3);
                        areaEnd = 4;
                    }
                    else
                    {
                        areaCode = s.Substring(0, 3);
                        areaEnd = 2;
                    }

                    // If 123-456...
                    if (s[areaEnd + 1] == '-') areaEnd += 1;

                    // Get First three digits
                    first = s.Substring(areaEnd + 1, 3);

                    int firstEnd = areaEnd + 3;

                    // If ..456-7891
                    if (s[firstEnd + 1] == '-') firstEnd += 1;

                    // Get Last Four Digits
                    last = s.Substring(firstEnd + 1);
                }

                result = "(" + areaCode + ") " + first + "-" + last;
            }
            else result = s;

            return result;
        }


    }
}
