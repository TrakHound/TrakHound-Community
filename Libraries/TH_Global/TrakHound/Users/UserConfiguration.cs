// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TH_Global.TrakHound.Users
{
    /// <summary>
    /// TrakHound User Account information
    /// </summary>
    public class UserConfiguration
    {
        public string Username { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zipcode { get; set; }

        public string ImageUrl { get; set; }

        public string Token { get; set; }
        public string SessionToken { get; set; }

        public DateTime LastLogin { get; set; }

        public int DeviceLimit { get; set; }
        public int DeviceCount { get; set; }
        public int UserLevel { get; set; }
        public int PlanType { get; set; }

        public static UserConfiguration Get(string json)
        {
            return Web.JSON.ToType<UserConfiguration>(json);
        }

        public static UserConfiguration GetFromDataRow(DataRow row)
        {
            UserConfiguration result = new UserConfiguration();

            foreach (System.Reflection.PropertyInfo info in typeof(UserConfiguration).GetProperties())
            {
                if (info.Name == "last_login") result.LastLogin = DateTime.UtcNow;
                else
                {
                    string col = ToJsonVariable(info.Name);

                    if (row.Table.Columns.Contains(col))
                    {
                        object value = row[col];

                        if (value != DBNull.Value)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(value, t), null);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Convert from 'first_second_third' to 'FirstSecondThird'
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        private static string FromJsonVariable(string var)
        {
            var builder = new StringBuilder();

            string[] words = var.Split('_');

            string s;
            foreach (string word in words)
            {
                s = Functions.String_Functions.UppercaseFirst(word);
                builder.Append(s);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Convert from 'FirstSecondThird' to 'first_second_third'
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        private static string ToJsonVariable(string var)
        {
            string[] words = SplitByUppercase(var);

            var builder = new StringBuilder();
            string s;

            for (var i = 0; i < words.Length; i++)
            {
                s = words[1].ToLower();
                builder.Append(s);

                if (words.Length > 1 && i < words.Length) builder.Append("_");
            }

            return builder.ToString();
        }

        private static string[] SplitByUppercase(string s)
        {
            var words = new List<string>();

            int start = 0;
            string word;

            // Get individual words delimited by an Uppercase Character
            for (var i = 0; i < s.Length; i++)
            {
                if (char.IsUpper(s[i]) && i > 0)
                {
                    word = s.Substring(start, i);
                    words.Add(word);
                    start = i;
                }

                if (i == s.Length - 1)
                {
                    word = s.Substring(start, i + 1);
                    words.Add(word);
                }
            }

            return words.ToArray();
        }
    }
}
