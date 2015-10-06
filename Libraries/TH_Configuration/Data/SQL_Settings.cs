// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TH_Configuration
{

    public class SQL_Settings
    {

        public string Server { get; set; }
        public int Port { get; set; }

        public string Database { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public delegate void SQLErrorHandler(string Message);
        public event SQLErrorHandler SQL_Error;

        public void SendError(string Message)
        {         
            SQLErrorHandler handler = SQL_Error;
            if (handler != null) SQL_Error(Message);
        }

        public SQL_Settings AdminSQL;

        public string PHP_Server { get; set; }
        public string PHP_Directory { get; set; }
        public string Database_Prefix { get; set; }

    }

}
