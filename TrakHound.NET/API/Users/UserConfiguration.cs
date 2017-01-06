// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Newtonsoft.Json;
using TrakHound.Tools;
using TrakHound.Tools.Web;

namespace TrakHound.API.Users
{
    /// <summary>
    /// TrakHound User Account information
    /// </summary>
    public class UserConfiguration
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("zipcode")]
        public string Zipcode { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("session_token")]
        public string SessionToken { get; set; }

        [JsonProperty("last_login")]
        public DateTime LastLogin { get; set; }


        public static UserConfiguration Get(string json)
        {
            return JSON.ToType<UserConfiguration>(json);
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
                s = String_Functions.UppercaseFirst(word);
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



        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {

            var other = obj as UserConfiguration;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        private static bool EqualTo(UserConfiguration c1, UserConfiguration c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.Username == c2.Username && c1.SessionToken == c2.SessionToken;
        }

        private static bool NotEqualTo(UserConfiguration c1, UserConfiguration c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.Username != c2.Username || c1.SessionToken != c2.SessionToken;
        }

        public static bool operator ==(UserConfiguration c1, UserConfiguration c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(UserConfiguration c1, UserConfiguration c2)
        {
            return NotEqualTo(c1, c2);
        }

    }
}
