// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

using TH_Global;
using TH_Global.Functions;

using TrakHound_Server_Core;

namespace TrakHound_Server_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var worker = new Worker();

            Console.ReadLine();
        }
    }

    class Worker
    {
        Server server;

        bool serverServiceWasRunning = false;

        public Worker()
        {
            // Check to see if Server Service is Running
            var serverStatus = Service_Functions.GetServiceStatus(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (serverStatus == ServiceStatus.Running) serverServiceWasRunning = true;

            if (StopServerService()) Init();
            else Logger.Log("Error :: Server Service Could Not Be Stopped :: Aborting Console");
        }

        private void Init()
        {
            FileLocations.CreateAllDirectories();

            TH_Database.DatabasePluginReader.ReadPlugins();

            TH_UserManagement.Management.UserManagementSettings.ReadConfiguration();

            server = new Server();
            server.Stopped += Server_Stopped;
            server.Login();

            string path = TH_Global.FileLocations.AppData + @"\nigolresu";
            if (File.Exists(path))
            {
                string dir = Path.GetDirectoryName(path);

                var watcher = new FileSystemWatcher(dir);
                watcher.Changed += FileSystemWatcher_UserLogin_Changed;
                watcher.Created += FileSystemWatcher_UserLogin_Changed;
                watcher.Deleted += FileSystemWatcher_UserLogin_Changed;
                watcher.EnableRaisingEvents = true;
            }

            server.Start();
        }

        private void Server_Stopped()
        {
            // If Server Service was running when Console was started then start the service again
            if (serverServiceWasRunning) StartServerService();
        }

        private void FileSystemWatcher_UserLogin_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("UserLogin File Changed!");

            if (server != null) server.Login();
        }

        private static bool StartServerService()
        {
            bool result = true;

            var service = Service_Functions.GetServiceController(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (service != null)
            {
                Logger.Log("Starting Server Service...");
                service.Start();
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running) result = false;
                else Logger.Log("Server Service Started");
            }

            return result;
        }

        private static bool StopServerService()
        {
            bool result = true;

            var service = Service_Functions.GetServiceController(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (service != null)
            {
                Logger.Log("Stopping Server Service...");
                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running) service.Stop();
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Stopped) result = false;
                else Logger.Log("Server Service Stopped");
            }

            return result;
        }

    }
}
