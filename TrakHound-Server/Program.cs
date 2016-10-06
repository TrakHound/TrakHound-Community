// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Servers.DataProcessing;
using TrakHound.Servers.DataStorage;
using TrakHound.Tools;

namespace TrakHound_Server
{
    static class Program
    {
        static bool serverServiceWasRunning = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static void Main(string[] args)
        {
            Init(args);
        }

        private static void Init(string[] args)
        {
            try
            {
                PrintHeader();

                UpdateUserSettings();

                if (args.Length > 0)
                {
                    if (args.Length > 1)
                    {
                        string authenticationParameter = args[1];
                        switch (authenticationParameter)
                        {
                            case "login": SetServerCredentials(); break;

                            case "logout": ClearServerCredentials(); break;
                        }
                    }

                    string installParameter = args[0];
                    switch (installParameter)
                    {
                        case "debug": StartDebug(); break;

                        case "console": StartConsole(); break;

                        case "shell": StartShell(); break;

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
            catch (Exception ex)
            {
                Console.WriteLine("TrakHound Server Error :: Restarting Server in 5 Seconds..");
            }
            finally
            {
                System.Threading.Thread.Sleep(5000);

                Init(args);
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

        private static ProcessingServer deviceServer;
        private static LocalStorageServer dataServer;

        private static void StartServers()
        {
            deviceServer = new ProcessingServer();
            deviceServer.Login();
            deviceServer.Start();

            dataServer = new LocalStorageServer();
            dataServer.Start();
        }

        private static void StopServers()
        {
            if (deviceServer != null) deviceServer.Stop();

            if (dataServer != null) dataServer.Stop();
        }

        private static void StartDebug()
        {
            StartServers();

            Console.ReadLine();

            RestartServerService();
        }

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

        private static void StartShell()
        {
            while (true) // Loop indefinitely
            {
                Console.WriteLine("Enter input:"); // Prompt
                string line = Console.ReadLine(); // Get string from user
                switch (line)
                {
                    case "start": StartServers(); break;
                    case "stop": StopServers(); break;
                }
            }
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


        private static void UpdateUserSettings()
        {
            if (Properties.Settings.Default.UpdateSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateSettings = false;
                Properties.Settings.Default.Save();
            }
        }


        private static void SetServerCredentials()
        {
            // Ask for Username input
            Console.WriteLine("Enter Username:");
            string username = Console.ReadLine();

            // Ask for Password input
            Console.WriteLine("Enter password:");
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    password += info.KeyChar;
                    info = Console.ReadKey(true);
                    Console.Write("*");
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        password = password.Substring
                        (0, password.Length - 1);
                    }
                    info = Console.ReadKey(true);
                }
            }

            Console.WriteLine();

            // Login using API
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Logging " + username + " in...");

                var userConfig = UserManagement.CreateTokenLogin(username, password, "TrakHound-Server-Console-Login");
                if (userConfig != null) ServerCredentials.Create(userConfig);
                else ServerCredentials.Remove();
            }
        }

        private static void ClearServerCredentials()
        {
            ServerCredentials.Remove();
        }


        private static void PrintHeader()
        {
            Console.WriteLine("--------------------------------------------------");

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "TrakHound_Server.Header.txt";

            try
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string header = reader.ReadToEnd();

                    if (header.Contains("[v]")) header = header.Replace("[v]", GetVersion());

                    Console.WriteLine(header);
                }
            }
            catch (Exception ex) { }

            Console.WriteLine("--------------------------------------------------");
        }

        private static string GetVersion()
        {
            // Build Information
            var assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            return "v" + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();
        }
    }
}
