// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using TrakHound.Logging;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public partial class ConfigurationServer
    {
        /// <summary>
        /// API Configuration Server Port
        /// </summary>
        public const int PORT = 8472; // ASCII Dec for 'T' and 'H'

        private TcpListener tcpListener;
        private Thread listenThread;

        public ConfigurationServer()
        {
            tcpListener = new TcpListener(IPAddress.Any, PORT);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
        }

        public static ApiConfiguration Get(IPAddress address)
        {
            if (address != null)
            {
                try
                {
                    var serverEndPoint = new IPEndPoint(address, PORT);

                    using (var client = new TcpClient())
                    {
                        client.Connect(serverEndPoint);

                        using (var clientStream = client.GetStream())
                        {
                            // Read Response
                            byte[] response = GetResponse(clientStream);
                            if (response != null)
                            {
                                string json = Encoding.ASCII.GetString(response);

                                return JSON.ToType<ApiConfiguration>(json);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Error Connecting to " + address.ToString() + " :: " + ex.Message);
                }
            }

            return null;
        }

        private void ListenForClients()
        {
            tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = tcpListener.AcceptTcpClient();

                //create a thread to handle communication
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        /// <summary>
        /// Write the API Configuration File (JSON) back to the client
        /// </summary>
        /// <param name="client"></param>
        private void HandleClientComm(object client)
        {
            using (var tcpClient = (TcpClient)client)
            using (var clientStream = tcpClient.GetStream())
            {
                var apiConfig = ApiConfiguration.Read();
                if (apiConfig != null)
                {
                    string json = JSON.FromObject(apiConfig);
                    if (json != null)
                    {
                        byte[] buffer = Encoding.ASCII.GetBytes(json);

                        clientStream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        public static byte[] GetResponse(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream memStream = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memStream.Write(buffer, 0, read);
                }

                return memStream.ToArray();
            }
        }
    }

}
