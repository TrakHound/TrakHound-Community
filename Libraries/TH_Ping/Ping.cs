// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using MySql.Data.MySqlClient;
using TH_Configuration;


namespace TH_Ping
{
    /// <summary>
    /// Used to detect whether a port is reachable or not
    /// </summary>
    public class PortPing
    {

        System.Timers.Timer Ping_TIMER;

        public string Address { get; set; }
        public int Port { get; set; }
        public int Interval { get; set; }

        public delegate void PortPingDelly(bool PingResult);
        public event PortPingDelly PingResult;

        public void Start()
        {
            Ping_TIMER = new System.Timers.Timer();
            Ping_TIMER.Interval = 100;
            Ping_TIMER.Elapsed += Ping_TIMER_Elapsed;

            Ping_TIMER.Enabled = true;
        }

        void Ping_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Ping_TIMER.Interval = Interval;

            try
            {
                bool Result = false;

                if (Port > 0)
                    Result = PingPort(Address, Port);
                else
                {
                    string IPAddress = GetIPFromHost(Address);
                    if (IPAddress != null) Result = PingPort(IPAddress, 80);
                }

                PortPingDelly handler = PingResult;
                if (handler != null) handler(Result);
            }
            catch { }
        }


        private bool PingPort(string _HostURI, int _PortNumber)
        {
            try
            {
                TcpClient client = new TcpClient(_HostURI, _PortNumber);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetIPFromHost(string HostName)
        {
            string Result = null;

            IPHostEntry hostEntry;

            hostEntry = Dns.GetHostEntry(HostName);

            if (hostEntry.AddressList.Length > 0)
            {
                Result = hostEntry.AddressList[0].ToString();
            }

            return Result;
        }

        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = new Ping();

            try
            {
                PingReply reply = pinger.Send(nameOrAddress);

                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }

            return pingable;
        }

    }

    /// <summary>
    /// Used to detect whether the Database server is reachable or not
    /// </summary>
    public class MySQLPing
    {

        System.Timers.Timer Ping_TIMER;

        public SQL_Settings Settings = new SQL_Settings();

        public int SuccessInterval = 5000;
        public int FailureInterval = 10000;

        public delegate void MySQLPingDelly(bool PingResult);
        public event MySQLPingDelly MySQLPingResult;

        public MySQLPing()
        {
            Ping_TIMER = new System.Timers.Timer();
            Ping_TIMER.Interval = 100;
            Ping_TIMER.Elapsed += Ping_TIMER_Elapsed;
        }

        public void Start()
        {
            Ping_TIMER.Start();
        }


        void Ping_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool Result;

            Result = Ping(Settings);

            if (Result) Ping_TIMER.Interval = SuccessInterval;
            else Ping_TIMER.Interval = FailureInterval;

            MySQLPingDelly handler = MySQLPingResult;
            if (handler != null) handler(Result);
        }

        public static bool Ping(SQL_Settings SQL)
        {
            try
            {
                // ******* Need to find a way to ping for PHP ***********

                if (SQL.PHP_Server == null)
                {

                    MySqlConnection Ping;
                    Ping = new MySqlConnection();
                    Ping.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";

                    Ping.Open();
                    Ping.Close();

                    return true;

                }

                else return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }

}
