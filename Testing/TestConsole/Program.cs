using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using TH_Configuration;
using TH_DeviceManager;
using TH_Global.Functions;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = @"C:\TrakHound\Devices\APMZSRSQDJHVHTNTILHQ.xml";

            //var config = Configuration.Read(path);

            //config.ClientEnabled = false;

            ////var nodes = new Network_Functions.PingNodes();

            var dm = new DeviceManager();
            dm.LoadDevices();


            Console.ReadLine();
        }

    }
}
