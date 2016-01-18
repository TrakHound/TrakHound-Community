using System;
using MySql.Data.MySqlClient;

using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace TH_MySQL
{

    public class PHPPing
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

        public static bool PingHost(string nameOrAddress, out string msg)
        {
            bool result = false;
            msg = null;

            try
            {
                Ping pinger = new Ping();

                PingReply reply = pinger.Send(nameOrAddress);

                msg = "MySQL Successfully connected to : " + nameOrAddress;
                result = reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                // Discard PingExceptions and return false;
            }

            return result;
        }

    }


    /// <summary>
    /// Used to detect whether the Database server is reachable or not
    /// </summary>
    public class MySQLPing
    {

        System.Timers.Timer Ping_TIMER;

        public MySQL_Configuration configuration;

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

            Result = Ping(configuration);

            if (Result) Ping_TIMER.Interval = SuccessInterval;
            else Ping_TIMER.Interval = FailureInterval;

            MySQLPingDelly handler = MySQLPingResult;
            if (handler != null) handler(Result);
        }

        public static bool Ping(MySQL_Configuration config, out string msg)
        {
            bool result = false;
            msg = null;

            try
            {
                MySqlConnection Ping;
                Ping = new MySqlConnection();
                Ping.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";

                Ping.Open();
                Ping.Close();

                msg = "MySQL Successfully connected to : " + config.Database + " @ " + config.Server + ":" + config.Port;
                result = true;
            }
            catch (MySqlException sqex)
            {
                msg = "MySQL Error connecting to : " + config.Database + " @ " + config.Server + ":" + config.Port + Environment.NewLine;
                msg += sqex.Message;
                result = false;
            }
            catch (Exception ex)
            {
                msg = "MySQL Error connecting to : " + config.Database + " @ " + config.Server + ":" + config.Port + Environment.NewLine;
                msg += ex.Message;
                result = false;
            }

            return result;
        }

    }
}
