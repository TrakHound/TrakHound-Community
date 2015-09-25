// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TH_MySQL
{
    public static class MySQL_Tools
    {

        public const string VarChar = "varchar(90)";
        public const string BigInt = "bigint";
        public const string Double = "double";
        public const string Datetime = "datetime";

        public const string DateString = "yyyy-MM-dd H:mm:ss";

        /// <summary>
        /// Returns DateString as a string in the format needed to be imported into MySQL table
        /// Returns null (as string for mysql) if it cannot parse
        /// </summary>
        /// <param name="DateString">DateTime as a string</param>
        /// <returns></returns>
        public static string ConvertDateStringtoMySQL(string DateString)
        {
            string Result = "null";

            DateTime TS;
            if (DateTime.TryParse(DateString, out TS)) Result = TS.ToString(MySQL_Tools.DateString);

            return Result;
        }

        public static string ConvertToSafe(string s)
        {
            string r = s;
            if (r.Contains("'")) r = r.Replace("'", "\'");
            return r;
        }

    }
}
