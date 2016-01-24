// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
            var group = new ServerGroup();
        }
    }

    public class ServerGroup
    {
        public Server Server { get; set; }
        public Controller Controller { get; set; }
        public Device_Manager DeviceManager { get; set; }
        public Output_Console OutputConsole { get; set; }
        public Login login;


        Database_Settings userDatabaseSettings;
        Database_Settings UserDatabaseSettings
        {
            get { return userDatabaseSettings; }
            set
            {
                userDatabaseSettings = value;
            }
        }

        public ServerGroup()
        {
            //System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            OutputConsole = new Output_Console();

            LogWriter logWriter = new LogWriter();
            logWriter.Updated += Log_Updated;
            Console.SetOut(logWriter);

            //CheckForUpdates();

            Server = new Server();
            Server.CurrentUserChanged += Server_CurrentUserChanged;
            Controller = new Controller(this);

            userDatabaseSettings = ReadUserManagementSettings();

            GetRememberMe(userDatabaseSettings);

            //DEBUG $$$$
            //OpenDeviceManager();

            if (Properties.Settings.Default.autostart) StartServer();

            Application.Run(Controller);

            Environment.ExitCode = 0;
        }

        public void Close()
        {
            if (login != null) login.Close();
            if (DeviceManager != null) DeviceManager.Close();
        }


        void Server_CurrentUserChanged(UserConfiguration userConfig)
        {
            if (DeviceManager != null) DeviceManager.CurrentUser = userConfig;
        }


        public void OpenDeviceManager()
        {
            if (DeviceManager == null) DeviceManager = new Device_Manager(this);

            if (Server != null) DeviceManager.CurrentUser = Server.CurrentUser;

            if (login != null) login.Hide();
            DeviceManager.ShowDialog();
        }

        public void OpenOutputConsole()
        {
            if (OutputConsole != null) OutputConsole.Show();
        }


        void GetRememberMe(Database_Settings db)
        {
            // Remember Me
            UserConfiguration rememberUser = RememberMe.Get(RememberMeType.Server, db);
            if (rememberUser != null)
            {
                RememberMe.Set(rememberUser, RememberMeType.Server, db);

                if (Server != null) Server.Login(rememberUser);
            }
            else
            {
                Login();
            }
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


        public void Login()
        {
            bool deviceManagerOpen = false;
            if (DeviceManager != null)
            {
                deviceManagerOpen = true;
                DeviceManager.Hide();
            }

            if (login == null)
            {
                login = new Login();
                login.userDatabaseSettings = UserDatabaseSettings;
                login.CurrentUserChanged += login_CurrentUserChanged;
            }

            login.ShowDialog();

            if (deviceManagerOpen)
            {
                DeviceManager.Loading = true;
                DeviceManager.ShowDialog();
            }
        }

        void login_CurrentUserChanged(TH_UserManagement.Management.UserConfiguration userConfig)
        {
            if (Server != null) Server.Login(userConfig);
        }

        public void Logout()
        {
            RememberMe.Clear(RememberMeType.Server, userDatabaseSettings);

            if (Server != null) Server.Logout();
        }

        public void StartServer()
        {
            if (Server != null) Server.Start();
        }

        public void StopServer()
        {
            if (Server != null) Server.Stop();
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
     

        void Log_Updated(string newline)
        {
            if (OutputConsole != null) OutputConsole.AddLine(newline);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log("TrakHound Server :: Unhandled Exception :: " + e.ExceptionObject.ToString());
            Environment.Exit(12);
        }
    }

}
