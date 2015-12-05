// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using TH_Configuration;
using TH_Database;
using TH_Ping;

namespace TH_Device_Server
{
    public partial class Device_Server
    {

        bool CheckDatabaseConnections(Configuration config)
        {
            bool result = true;

            if (UseDatabases)
            {
                foreach (Database_Configuration db_config in config.Databases_Server.Databases)
                {
                    if (!TH_Database.Global.Ping(db_config)) { result = false; break; }
                }
            }

                return result;

            //bool error = true;

            //while (error)
            //{
            //    error = false;

            //    foreach (Database_Configuration db_config in config.Databases.Databases)
            //    {
            //        if (!TH_Database.Global.Ping(db_config)) { error = true; break; }
            //    }

            //    if (error)
            //    {
            //        UpdateProcessingStatus("Error Connecting to Databases... Retrying in 1000 ms");

            //        Thread.Sleep(1000);
            //    }
            //}

            //UpdateProcessingStatus("Database Connection Established");
        }












        public bool MTC_PingResult = false;
        public bool SQL_PingResult = false;
        public bool PHP_PingResult = false;

        void Ping_Agent_MTCPingResult(bool PingResult) { MTC_PingResult = PingResult; }

        void Ping_DB_MySQLPingResult(bool PingResult) { SQL_PingResult = PingResult; }

        void Ping_PHP_PingResult(bool PingResult) { PHP_PingResult = PingResult; }


        void Ping_Agent_Initialize(Configuration lSettings)
        {
            PortPing Agent_Ping = new PortPing();
            Agent_Ping.Address = lSettings.Agent.IP_Address;
            Agent_Ping.Port = lSettings.Agent.Port;
            Agent_Ping.Interval = 2000;
            Agent_Ping.PingResult += Ping_Agent_MTCPingResult;
            Agent_Ping.Start();
        }

        void Ping_DB_Initialize(Configuration lSettings)
        {
            //if (lSettings.SQL.PHP_Server == null)
            //{
            //    MySQLPing DB_Ping = new MySQLPing();
            //    DB_Ping.Settings = lSettings.SQL;
            //    DB_Ping.MySQLPingResult += Ping_DB_MySQLPingResult;
            //    DB_Ping.Start();
            //}
            //else SQL_PingResult = true;
        }

        void Ping_PHP_Initialize(Configuration lSettings)
        {
            //if (lSettings.SQL.PHP_Server != null)
            //{
            //    PortPing PHP_Ping = new PortPing();
            //    PHP_Ping.Address = lSettings.SQL.PHP_Server;
            //    PHP_Ping.Port = 0;
            //    PHP_Ping.Interval = 2000;
            //    PHP_Ping.PingResult += Ping_PHP_PingResult;
            //    PHP_Ping.Start();
            //}
            //else PHP_PingResult = true;
        }

    }
}
