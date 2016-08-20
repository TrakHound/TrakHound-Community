// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

using TrakHound;
using TrakHound.Servers.DataProcessing;
using TrakHound.Servers.DataStorage;
using TrakHound.Tools;

namespace TrakHound_Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string installParameter = args[0];
                switch (installParameter)
                {
                    case "debug": StartDebug(); break;

                    case "console": StartConsole(); break;

                    case "install": InstallService(); break;

                    case "uninstall": UninstallService(); break;
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new Service1()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static void InstallService()
        {
            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
        }

        private static void UninstallService()
        {
            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
        }

        private static void StartServers()
        {
            var server = new ProcessingServer();
            server.Login();

            server.Start();

            var dataServer = new LocalStorageServer();
            dataServer.Start();
        }

        private static void StartDebug()
        {
            StartServers();

            Console.ReadLine();

            RestartServerService();
        }


        static bool serverServiceWasRunning = false;

        private static void StartConsole()
        {
            // Check to see if Server Service is Running
            var serverStatus = Service_Functions.GetServiceStatus(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (serverStatus == ServiceStatus.Running) serverServiceWasRunning = true;

            if (StopServerService())
            {
                StartServers();
            }
            else Console.WriteLine("Error :: Server Service Could Not Be Stopped :: Aborting Console");
        }

        public static void RestartServerService()
        {
            // If Server Service was running when Console was started then start the service again
            if (serverServiceWasRunning) StartServerService();
        }

        public static bool StartServerService()
        {
            bool result = true;

            var service = Service_Functions.GetServiceController(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (service != null)
            {
                Console.WriteLine("Starting Server Service...");
                service.Start();
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running) result = false;
                else Console.WriteLine("Server Service Started");
            }

            return result;
        }

        public static bool StopServerService()
        {
            bool result = true;

            var service = Service_Functions.GetServiceController(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (service != null)
            {
                Console.WriteLine("Stopping Server Service...");
                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running) service.Stop();
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Stopped) result = false;
                else Console.WriteLine("Server Service Stopped");
            }

            return result;
        }

    }
}
