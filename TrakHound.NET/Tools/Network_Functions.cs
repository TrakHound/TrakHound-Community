// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace TrakHound.Tools
{
    public static class Network_Functions
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static IPAddress GetHostIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return null;
        }

        public static string GetMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
        }

        public static bool IsPortOpen(string host, int port, int timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                }

            }
            catch
            {
                return false;
            }
            return true;
        }

        public static IPAddress GetSubnetMask(IPAddress ip)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (ip.Equals(unicastIPAddressInformation.Address))
                        {
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }

            return null;
        }

        public class PingNodes
        {
            private int returnedIndexes = 0;
            private int expectedIndexes = 0;

            private int timeout = 2000;

            private List<IPAddress> addressRange;

            public delegate void PingReply_Handler(IPAddress ip, PingReply reply);
            public event PingReply_Handler PingReplied;

            public delegate void PingError_Handler(IPAddress ip, string msg);
            public event PingError_Handler PingError;

            public delegate void Finished_Handler();
            public event Finished_Handler Finished;

            public PingNodes() { }

            public PingNodes(List<IPAddress> _addressRange)
            {
                addressRange = _addressRange;
            }

            public PingNodes(List<IPAddress> _addressRange, int _timeout)
            {
                addressRange = _addressRange;
                timeout = _timeout;
            }

            public PingNodes(int _timeout)
            {
                timeout = _timeout;
            }

            public void Start()
            {
                returnedIndexes = 0;
                expectedIndexes = 0;

                var ip = GetHostIP();
                if (ip != null)
                {
                    var hostIp = IPNetwork.Parse(ip.ToString());
                    var list = addressRange == null ? new System.Net.IPAddressCollection(hostIp).ToList() : addressRange;
                    if (list.Count > 0)
                    {
                        expectedIndexes = list.Count;

                        for (var x = 0; x <= list.Count - 1; x++)
                        {
                            int index = x;
                            StartPing(list[x], index);
                        }
                    }
                }
            }

            public void Cancel()
            {
                foreach (var request in queuedPings) request.SendAsyncCancel();
            }

            private static byte[] GetSubnetBytes(IPAddress ip, IPAddress subnetMask)
            {
                var ibytes = ip.GetAddressBytes();
                var sbytes = subnetMask.GetAddressBytes();

                var b = new List<byte>();

                for (var x = 0; x <= sbytes.Length - 1; x++)
                {
                    if (sbytes[x] == Convert.ToByte(0)) return b.ToArray();
                    else b.Add(ibytes[x]);
                }

                return null;
            }

            public List<Ping> queuedPings = new List<Ping>();

            private void StartPing(IPAddress ipAddress, int index)
            {
                try
                {
                    var ping = new Ping();
                    ping.PingCompleted += Ping_PingCompleted;
                    queuedPings.Add(ping);
                    ping.SendAsync(ipAddress, timeout, ipAddress);
                }
                catch (Exception ex)
                {
                    PingError?.Invoke(ipAddress, ex.Message);
                    logger.Error(ex);
                }  
            }

            private void Ping_PingCompleted(object sender, PingCompletedEventArgs e)
            {
                try
                {
                    if (!e.Cancelled)
                    {
                        if (e.UserState != null) PingReplied?.Invoke((IPAddress)e.UserState, e.Reply);

                        returnedIndexes += 1;
                        if (returnedIndexes >= expectedIndexes) Finished?.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    if (e != null && e.Reply != null && e.Reply.Address != null) PingError?.Invoke(e.Reply.Address, ex.Message);

                    logger.Error(ex);
                }
            }
        }

        public class IPAddressRange
        {
            readonly AddressFamily addressFamily;
            readonly byte[] lowerBytes;
            readonly byte[] upperBytes;

            public IPAddressRange(IPAddress lower, IPAddress upper)
            {
                // Assert that lower.AddressFamily == upper.AddressFamily
                addressFamily = lower.AddressFamily;
                lowerBytes = lower.GetAddressBytes();
                upperBytes = upper.GetAddressBytes();
            }

            public bool IsInRange(IPAddress address)
            {
                if (address.AddressFamily != addressFamily)
                {
                    return false;
                }

                byte[] addressBytes = address.GetAddressBytes();

                bool lowerBoundary = true, upperBoundary = true;

                for (int i = 0; i < this.lowerBytes.Length &&
                    (lowerBoundary || upperBoundary); i++)
                {
                    if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                        (upperBoundary && addressBytes[i] > upperBytes[i]))
                    {
                        return false;
                    }

                    lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                    upperBoundary &= (addressBytes[i] == upperBytes[i]);
                }

                return true;
            }
        }

    }
}
