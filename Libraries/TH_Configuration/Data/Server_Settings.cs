// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TH_Configuration
{
    public class Server_Settings
    {

        public Server_Settings() { Tables = new Tables_Settings(); }

        public Tables_Settings Tables;

    }

    public class Tables_Settings
    {

        public Tables_Settings() { MTConnect = new Tables_MTConnect_Settings(); }

        public Tables_MTConnect_Settings MTConnect;

    }

    public class Tables_MTConnect_Settings
    {

        public bool Probe { get; set; }
        public bool Current { get; set; }
        public bool Sample { get; set; }

    }
}
