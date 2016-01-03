// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using System.Threading;
using System.Threading.Tasks;

//using System.Threading;
//using System.Windows;
//using System.Windows.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Updater;
using TH_UserManagement.Management;

using TrakHound_Server_Core;

namespace TrakHound_Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var run = new Run();
        }

        class Run
        {
            Server server;
            Controller controller;
            ConsoleData consoleData;
            //Device_Manager deviceManager;
            //Output_Console console;

            public Run()
            {
                System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                //console = new Output_Console();
                consoleData = new ConsoleData();

                LogWriter logWriter = new LogWriter();
                logWriter.Updated += Log_Updated;
                Console.SetOut(logWriter);

                CheckForUpdates();

                server = new Server();
                //deviceManager = new Device_Manager();
                //controller = new Controller(server, deviceManager, console);
                controller = new Controller(server, consoleData);

                Database_Settings userDatabaseSettings = GetRememberMe();
                controller.userDatabaseSettings = userDatabaseSettings;

                Application.Run(controller);

                Environment.ExitCode = 0;
            }

            #region "Updates"

            void CheckForUpdates()
            {
                Updater_Start();
            }

            void Updater_Start()
            {
                UpdateCheck updateCheck = new UpdateCheck();
                updateCheck.AppInfoReceived += Updater_AppInfoReceived;
                updateCheck.Start("http://www.feenux.com/trakhound/appinfo/th/server-appinfo.json");
            }

            void Updater_AppInfoReceived(UpdateCheck.AppInfo info)
            {
                if (info != null)
                {
                    // Print Auto Update info to Console
                    Console.WriteLine("---- Auto-Update Info ----");
                    Console.WriteLine("TrakHound - Server");
                    Console.WriteLine("Release Type : " + info.releaseType);
                    Console.WriteLine("Version : " + info.version);
                    Console.WriteLine("Build Date : " + info.buildDate);
                    Console.WriteLine("Download URL : " + info.downloadUrl);
                    Console.WriteLine("Update URL : " + info.updateUrl);
                    Console.WriteLine("File Size : " + info.size);
                    Console.WriteLine("--------------------------");

                    // Check if version is Up-to-date
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    Version version = assembly.GetName().Version;

                    Version latestVersion = null;
                    Version.TryParse(info.version, out latestVersion);

                    if (latestVersion != null)
                    {
                        if (version < latestVersion)
                        {
                            // Run Updater
                            Updater updater = new Updater();
                            updater.UpdateKeyGroup = "Server-Updates";
                            updater.assembly = System.Reflection.Assembly.GetExecutingAssembly();
                            updater.Start(info.updateUrl);

                            Console.WriteLine("Update Available : " + latestVersion.ToString());
                        }
                    }
                }
            }

            #endregion

            Database_Settings GetRememberMe()
            {
                Database_Settings result = ReadUserManagementSettings();

                // Remember Me
                UserConfiguration rememberUser = RememberMe.Get(RememberMeType.Server, result);
                if (rememberUser != null)
                {
                    RememberMe.Set(rememberUser, RememberMeType.Server, result);

                    if (server != null) server.Login(rememberUser);
                }
                else
                {
                    Login login = new Login();
                    login.CurrentUserChanged += login_CurrentUserChanged;
                    login.ShowDialog();
                }

                return result;
            }

            Database_Settings ReadUserManagementSettings()
            {
                Database_Settings result = null;

                DatabasePluginReader dpr = new DatabasePluginReader();

                string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
                string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

                string configPath;

                // systemPath takes priority (easier for user to navigate to)
                if (File.Exists(systemPath)) configPath = systemPath;
                else configPath = localPath;

                UserManagementSettings userSettings = UserManagementSettings.ReadConfiguration(configPath);

                if (userSettings != null)
                {
                    if (userSettings.Databases.Databases.Count > 0)
                    {
                        result = userSettings.Databases;
                        Global.Initialize(result);
                    }
                }

                return result;
            }

            void login_CurrentUserChanged(TH_UserManagement.Management.UserConfiguration userConfig)
            {
                if (server != null) server.Login(userConfig);
            }

            void Log_Updated(string newline)
            {
                if (consoleData != null) consoleData.AddLogLine(newline);

                //if (console != null) console.AddLine(newline);

                //var ci = new Console_Item();
                //ci.Row = rowIndex;
                //ci.Timestamp = DateTime.Now;
                //ci.Text = newline;

                //rowIndex++;

                //AddLogLine(ci);
            }

            //ObservableCollection<Output_Console.Console_Item> console_output;
            //public ObservableCollection<Output_Console.Console_Item> Console_Output
            //{
            //    get
            //    {
            //        if (console_output == null) console_output = new ObservableCollection<Output_Console.Console_Item>();
            //        return console_output;
            //    }
            //    set { console_output = value; }
            //}

            //Int64 rowIndex = 0;
            //const int MaxLines = 500;

            //void AddLogLine(Output_Console.Console_Item ci)
            //{
            //    Console_Output.Add(ci);

            //    if (Console_Output.Count > MaxLines)
            //    {
            //        int first = Console_Output.Count - MaxLines;

            //        for (int x = 0; x < first; x++) Console_Output.RemoveAt(0);
            //    }

            //    //dg.ScrollIntoView(ci);

            //    //scrollviewer.ScrollToEnd();

            //    //var scrollerViewer = GetScrollViewer();
            //    //if (scrollerViewer != null) scrollerViewer.ScrollToEnd();
            //}

            void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            {
                Logger.Log("TrakHound Server :: Unhandled Exception :: " + e.ExceptionObject.ToString());
                Environment.Exit(12);
            }
        }

    }

    class ConsoleData
    {
        private static object _lock = new object();

        public Output_Console console;

        ObservableCollection<Console_Item> console_output;
        public ObservableCollection<Console_Item> Console_Output
        {
            get
            {
                if (console_output == null)
                {
                    console_output = new ObservableCollection<Console_Item>();
                }
                return console_output;
            }
            set { console_output = value; }
        }

        Int64 rowIndex = 0;
        const int MaxLines = 500;

        public void AddLogLine(string line)
        {
            var ci = new Console_Item();
            ci.Row = rowIndex;
            ci.Timestamp = DateTime.Now;
            ci.Text = line;

            rowIndex++;

            AddLogLine_GUI(ci);
        }

        void AddLogLine_GUI(Console_Item ci)
        {
            Console_Output.Add(ci);

            if (Console_Output.Count > MaxLines)
            {
                int first = Console_Output.Count - MaxLines;

                for (int x = 0; x < first; x++) Console_Output.RemoveAt(0);
            }
        }
    }

    public class Console_Item
    {
        public Int64 Row { get; set; }
        public DateTime Timestamp { get; set; }
        public string Text { get; set; }
    }

}
