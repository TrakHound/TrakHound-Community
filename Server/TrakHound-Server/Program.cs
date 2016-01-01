// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Forms;
using System.IO;

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
            Device_Manager deviceManager;
            Output_Console console;

            public Run()
            {
                System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                console = new Output_Console();

                LogWriter logWriter = new LogWriter();
                logWriter.Updated += Log_Updated;
                Console.SetOut(logWriter);

                CheckForUpdates();

                server = new Server();
                deviceManager = new Device_Manager();
                controller = new Controller(server, deviceManager, console);

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
                if (console != null) console.AddLine(newline);
            }

            void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            {
                Logger.Log("TrakHound Server :: Unhandled Exception :: " + e.ExceptionObject.ToString());
                Environment.Exit(12);
            }
        }
    }
}
