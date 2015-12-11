using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;

using TH_Configuration;
using TH_Database;
using TH_Device_Server;
using TH_Global;
using TH_UserManagement;
using TH_UserManagement.Management;

namespace TrakHound_Server_Core
{
    public partial class Server
    {

        UserConfiguration currentuser;

        Database_Settings userDatabaseSettings;

        void Login()
        {
            // Remember Me
            UserConfiguration rememberUser = RememberMe.Get(RememberMeType.Server, userDatabaseSettings);
            if (rememberUser != null)
            {
                LoginUser(rememberUser);
            }
            else
            {
                RememberMeMonitor_Start();
                Console.WriteLine("Login failed : Login through Control Panel and set 'Remember Me'");
            }
        }

        void LoginUser(UserConfiguration userConfig)
        {
            RememberMe.Set(userConfig, RememberMeType.Server, userDatabaseSettings);

            currentuser = userConfig;

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(TH_Global.Formatting.UppercaseFirst(userConfig.username));
            Console.ResetColor();

            Console.Write(" Logged in Successfully" + Environment.NewLine);
        }

        void ReadUserManagementSettings()
        {
            DatabasePluginReader dpr = new DatabasePluginReader();

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

            string configPath;

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            //Logger.Log(configPath);

            UserManagementSettings userSettings = UserManagementSettings.ReadConfiguration(configPath);

            if (userSettings != null)
            {
                if (userSettings.Databases.Databases.Count > 0)
                {
                    userDatabaseSettings = userSettings.Databases;
                    Global.Initialize(userDatabaseSettings);
                }
            }
        }

        #region "Remember Me Monitor"

        System.Timers.Timer rememberMe_TIMER;

        void RememberMeMonitor_Start()
        {
            rememberMe_TIMER = new System.Timers.Timer();
            rememberMe_TIMER.Interval = 5000;
            rememberMe_TIMER.Elapsed += rememberMe_TIMER_Elapsed;
            rememberMe_TIMER.Enabled = true;
        }

        void rememberMe_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(RememberMeMonitor_Worker));
        }

        void RememberMeMonitor_Worker(object o)
        {
           UserConfiguration rememberMe = RememberMe.Get(RememberMeType.Server, userDatabaseSettings);
            if (rememberMe != null && currentuser == null)
            {
                if (rememberMe_TIMER != null) rememberMe_TIMER.Enabled = false;
                LoginUser(rememberMe);
            }
        }

        #endregion

    }
}
