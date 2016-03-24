using System;


namespace TestConsole
{
    class Program
    {


        static void Main(string[] args)
        {
            TH_Database.DatabasePluginReader.ReadPlugins();


            var configs = TH_Configuration.Configuration.ReadAll(TH_Global.FileLocations.Devices);

            foreach (var config in configs)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(StartDeviceServer), config);

               
            }


            Console.ReadLine();
        }

        private static void StartDeviceServer(object obj)
        {
            var config = (TH_Configuration.Configuration)obj;

            var server = new TH_Device_Server.Device_Server(config);
            server.Start();
        }

    }
}
