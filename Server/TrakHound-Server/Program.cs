using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

using TH_Configuration;
using TH_Database;
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
            Control_Panel controlPanel;

            public Run()
            {
                server = new Server();
                controlPanel = new Control_Panel();

                controller = new Controller(server, controlPanel);

                //Database_Settings userDatabaseSettings = GetRememberMe();
                //controller.userDatabaseSettings = userDatabaseSettings;

                //server.Start();

                Application.Run(controller);
            }

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

        }
    }
}
