using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TH_Adapter
{
    public class Adapter
    {

        #region "Properties"

        public string DeviceName { get; set; }

        public int Port { get; set; }

        public int Heartbeat { get; set; }

        #endregion

        #region "SubClasses"

        public class DataItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        #endregion

        #region "Methods"

        public void Update()
        {



        }

        /// <summary>
        /// Create MTConnect® Adapter data string
        /// </summary>
        public string CreateMessage(List<DataItem> items)
        {
            string result = null;

            result = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffff'Z'");

            foreach (DataItem item in items)
            {
                result += "|" + DeviceName + ":" + item.Name + "|" + item.Value;
            }

            return result;
        }

        // 2014-09-29T23:59:33.460470Z|device1:current|12|device2:current|11|device3:current|10

        /// <summary>
        /// Send string Message to LocalHost using specified Port
        /// </summary>
        public void Send(int port, string message)
        {

            Console.WriteLine("Sending to LocalLoop back " + port.ToString());

  //          try {
  //  using (TcpClient client = new TcpClient()) {
  //    IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);
  //    client.Connect(endPoint);
  //    // code past this never hits because the line above fails
  //  }
  //} catch (SocketException err) {
  //  Console.WriteLine("Send() :: " + err.Message);
  //}


            try
            {

                //TcpClient client = new TcpClient("127.0.0.1", port);
                //client.SendTimeout = 5000;
                //client.ReceiveTimeout = 5000;

                //Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                //NetworkStream stream = client.GetStream();
                //stream.Write(data, 0, data.Length);
                //Console.WriteLine("Sent: {0}", message);

                ////data = new Byte[256];
                ////String responseData = String.Empty;

                ////Int32 bytes = stream.Read(data, 0, data.Length);
                ////responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                ////Console.WriteLine("Received: {0}", responseData);

                //// Close everything.
                //stream.Flush();
                //stream.Close();
                //client.Close();         



                IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);

                using (TcpClient client = new TcpClient(endPoint))
                {
                    Console.WriteLine("Client Created");

                    client.Connect(endPoint);
                    client.SendTimeout = 5000;
                    client.ReceiveTimeout = 5000;

                    //Console.WriteLine("Client Connected");

                    Stream stream = client.GetStream();
                    Byte[] data = Encoding.ASCII.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    //data = new Byte[1024];

                    //while (stream.CanRead)
                    //{
                    //    stream.Read(data, 0, data.Length);
                    //}

                    //Console.WriteLine(System.Text.Encoding.UTF8.GetString(data));

                    stream.Flush();
                    stream.Close();

                    //client.Client.SendTo(data, endPoint);

                    Console.WriteLine("Data Sent");

                    //using (StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.ASCII))
                    //{
                    //    Console.WriteLine("Client Stream Connected");

                    //    //byte[] data = Encoding.ASCII.GetBytes(message);

                    //    writer.Write(message);
                    //}
                }
            }
            catch (System.Net.Sockets.SocketException nex)
            {
                Console.WriteLine("Send Data() :: Socket Exception :: " + nex.Message + " : " + nex.ErrorCode.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send Data() :: Exception :: " + ex.Message);
            }
        }

        #endregion

    }

}
