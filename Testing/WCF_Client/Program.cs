using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

using TH_Global.Functions;

namespace WCF_Client
{
    public class MessageCallback : WCF_Functions.IMessageCallback
    {
        public void OnCallback(WCF_Functions.MessageData data)
        {
            switch(data.Id)
            {
                case "start": Console.WriteLine(data.Data.ToString()); break;

                case "stop": Console.WriteLine(data.Data.ToString()); break;

                case "progress_value": Console.WriteLine(data.Data.ToString()); break;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var callback = new MessageCallback();

            var proxy = WCF_Functions.Client.GetWithCallback(callback);

            while (true) // Loop indefinitely
            {
                Console.WriteLine(">"); // Prompt
                string line = Console.ReadLine(); // Get string from user

                switch (line.ToLower())
                {
                    case "check":

                        proxy.SendData(new WCF_Functions.MessageData("check", null));
                        break;

                    case "apply": 

                        proxy.SendData(new WCF_Functions.MessageData("apply", null));
                        break;

                    case "clear": 

                        proxy.SendData(new WCF_Functions.MessageData("clear", null));
                        break;
                }
            }
        }

    }
}
