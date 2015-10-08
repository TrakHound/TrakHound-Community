using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Sockets;

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
        string CreateMessage(List<DataItem> items)
        {
            string Result = null;

            Result = DateTime.Now.ToUniversalTime().ToString();

            return Result;
        }

        /// <summary>
        /// Send string Message to LocalHost using specified Port
        /// </summary>
        void Send(int port, string message)
        {
            try
            {

                TcpClient client = new TcpClient("localhost", port);

                NetworkStream stream = client.GetStream();
                
                Byte[] data = Encoding.ASCII.GetBytes(message);

                stream.Write(data, 0, data.Length);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Send() :: " + ex.Message);
            }
        }

        #endregion

    }

}
