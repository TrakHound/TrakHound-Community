// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Text;

namespace TrakHound.Tools
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

        public static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            bool upper = false;

            foreach (var c in s)
            {
                if (char.IsUpper(c))
                {
                    upper = true;
                    break;
                }
            }

            if (!upper) return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
            return s;
        }

        public static string LowercaseFirstCharacter(string s)
        {
            if (s != null && s.Length > 0) return s.Remove(0, 1).Insert(0, s[0].ToString().ToLower());
            return s;
        }

        public static string ToPhoneNumber(string s)
        {
            string result = null;

            if (s != null)
            {
                // If standard US number
                if (s.Length >= 10 && s.Length <= 12)
                {
                    s = s.Replace(" ", "");

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
            }

            return result;
        }

        static readonly string[] FileSizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string FileSizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + FileSizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, FileSizeSuffixes[mag]);
        }


        public static string ToSpecial(string s)
        {
            string r = s;

            if (r.Contains("%")) r = r.Replace("%", "%25");

            if (r.Contains("!")) r = r.Replace("!", "%21");
            if (r.Contains("\"")) r = r.Replace("\"", "%22");
            if (r.Contains("#")) r = r.Replace("#", "%23");
            if (r.Contains("$")) r = r.Replace("$", "%24");
            if (r.Contains("&")) r = r.Replace("&", "%26");
            if (r.Contains("'")) r = r.Replace("'", "%27");
            if (r.Contains("(")) r = r.Replace("(", "%28");
            if (r.Contains(")")) r = r.Replace(")", "%29");
            if (r.Contains("*")) r = r.Replace("*", "%2A");
            if (r.Contains("+")) r = r.Replace("+", "%2B");
            if (r.Contains(",")) r = r.Replace(",", "%2C");
            //if (r.Contains("-")) r = r.Replace("-", "%2D");
            //if (r.Contains(".")) r = r.Replace(".", "%2E");
            //if (r.Contains("/")) r = r.Replace("/", "%2F");

            //if (r.Contains(":")) r = r.Replace(":", "%3A");
            if (r.Contains(";")) r = r.Replace(";", "%3B");
            if (r.Contains("<")) r = r.Replace("<", "%3C");
            if (r.Contains("=")) r = r.Replace("=", "%3D");
            if (r.Contains(">")) r = r.Replace(">", "%3E");
            if (r.Contains("?")) r = r.Replace("?", "%3F");

            if (r.Contains("@")) r = r.Replace("@", "%40");

            if (r.Contains("[")) r = r.Replace("[", "%5B");
            //if (r.Contains("\\")) r = r.Replace("\\", "%5C");
            if (r.Contains("]")) r = r.Replace("]", "%5D");
            if (r.Contains("^")) r = r.Replace("^", "%5E");
            //if (r.Contains("_")) r = r.Replace("_", "%5F");

            if (r.Contains("`")) r = r.Replace("`", "%60");
            if (r.Contains("{")) r = r.Replace("{", "%7B");
            if (r.Contains("|")) r = r.Replace("|", "%7C");
            if (r.Contains("}")) r = r.Replace("}", "%7D");
            if (r.Contains("~")) r = r.Replace("~", "%7E");

            return r;
        }

        public static string FromSpecial(string s)
        {
            string r = s;

            if (r.Contains("%21")) r = r.Replace("%21", "!");
            if (r.Contains("%22")) r = r.Replace("%22", "\"");
            if (r.Contains("%23")) r = r.Replace("%23", "#");
            if (r.Contains("%24")) r = r.Replace("%24", "$");
            if (r.Contains("%26")) r = r.Replace("%26", "&");
            if (r.Contains("%27")) r = r.Replace("%27", "'");
            if (r.Contains("%28")) r = r.Replace("%28", "(");
            if (r.Contains("%29")) r = r.Replace("%29", ")");
            if (r.Contains("%2A")) r = r.Replace("%2A", "*");
            if (r.Contains("%2B")) r = r.Replace("%2B", "+");
            if (r.Contains("%2C")) r = r.Replace("%2C", ",");
            //if (r.Contains("%2D")) r = r.Replace("%2D", "-");
            //if (r.Contains("%2E")) r = r.Replace("%2E", ".");
            //if (r.Contains("%2F")) r = r.Replace("%2F", "/");

            //if (r.Contains("%3A")) r = r.Replace("%3A", ":");
            if (r.Contains("%3B")) r = r.Replace("%3B", ";");
            if (r.Contains("%3C")) r = r.Replace("%3C", "<");
            if (r.Contains("%3D")) r = r.Replace("%3D", "=");
            if (r.Contains("%3E")) r = r.Replace("%3E", ">");
            if (r.Contains("%3F")) r = r.Replace("%3F", "?");

            if (r.Contains("%40")) r = r.Replace("%40", "@");

            if (r.Contains("%5B")) r = r.Replace("%5B", "[");
            //if (r.Contains("%5C")) r = r.Replace("%5C", "\\");
            if (r.Contains("%5D")) r = r.Replace("%5D", "]");
            if (r.Contains("%5E")) r = r.Replace("%5E", "^");
            //if (r.Contains("%5F")) r = r.Replace("%5F", "_");

            if (r.Contains("%60")) r = r.Replace("%60", "`");
            if (r.Contains("%7B")) r = r.Replace("%7B", "{");
            if (r.Contains("%7C")) r = r.Replace("%7C", "|");
            if (r.Contains("%7D")) r = r.Replace("%7D", "}");
            if (r.Contains("%7E")) r = r.Replace("%7E", "~");

            if (r.Contains("%25")) r = r.Replace("%25", "%");

            return r;
        }

    }
}
