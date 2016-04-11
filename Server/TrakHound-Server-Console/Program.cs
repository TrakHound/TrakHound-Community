// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

using System.Runtime.InteropServices;
using System.Threading;

using TH_Global;
using TH_Global.Functions;
using Microsoft.Win32;

using TrakHound_Server_Core;

namespace TrakHound_Server_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            var worker = new Worker();

            Console.ReadLine();

            //hold the console so it doesn’t run off the end
            while (!exitSystem)
            {
                Thread.Sleep(500);
            }
        }

        static bool exitSystem = false;

        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Worker.RestartServerService();

            Thread.Sleep(500);

            //allow main to run off
            exitSystem = true;

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }
        #endregion
    }

    class Worker
    {
        Server server;

        static bool serverServiceWasRunning = false;

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
            //server.Stopped += Server_Stopped;
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

        public static void RestartServerService()
        {
            // If Server Service was running when Console was started then start the service again
            if (serverServiceWasRunning) StartServerService();
        }

        private void FileSystemWatcher_UserLogin_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("UserLogin File Changed!");

            if (server != null) server.Login();
        }

        public static bool StartServerService()
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

        public static bool StopServerService()
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
