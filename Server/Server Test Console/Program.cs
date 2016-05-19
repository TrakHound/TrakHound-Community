// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using TH_Global;
using TH_Global.Functions;

using TrakHound_Server_Core;

namespace Server_Test_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "simulation")
                {
                    Variables.SIMULATION_MODE = true;
                    Console.Write("Simulation Mode Enabled");
                }
            }


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
            Init();
        }

        private void Init()
        {
            //FileLocations.CreateAllDirectories();

            //TH_Database.DatabasePluginReader.ReadPlugins();

            //TH_UserManagement.Management.UserManagementSettings.ReadConfiguration();

            server = new Server();
            server.Login();

            //UserLoginFileMonitor_Start();

            server.Start();
        }

        private void UserLoginFileMonitor_Start()
        {
            string dir = FileLocations.AppData;
            string filename = "nigolresu";

            var watcher = new FileSystemWatcher(dir, filename);
            watcher.Changed += UserLoginFileMonitor_Changed;
            watcher.Created += UserLoginFileMonitor_Changed;
            watcher.Deleted += UserLoginFileMonitor_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private void UserLoginFileMonitor_Changed(object sender, FileSystemEventArgs e)
        {
            if (server != null) server.Login();
        }

    }
}
