using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.IO;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_UserManagement.Management;

using TrakHound_Server_Core;

namespace TrakHound_Server_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var worker = new Worker();
        }

        

        //static Database_Settings ReadUserManagementSettings()
        //{
        //    Database_Settings result = null;

        //    DatabasePluginReader dpr = new DatabasePluginReader();

        //    string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
        //    string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

        //    string configPath;

        //    // systemPath takes priority (easier for user to navigate to)
        //    if (File.Exists(systemPath)) configPath = systemPath;
        //    else configPath = localPath;

        //    UserManagementSettings userSettings = UserManagementSettings.ReadConfiguration(configPath);

        //    if (userSettings != null)
        //    {
        //        if (userSettings.Databases.Databases.Count > 0)
        //        {
        //            result = userSettings.Databases;
        //            Global.Initialize(result);
        //        }
        //    }

        //    return result;
        //}
    }

    public class Worker
    {
        public Worker()
        {
            var server = new Server();

            UserConfiguration rememberUser = RememberMe.Get(RememberMeType.Server, null);
            if (rememberUser != null)
            {
                RememberMe.Set(rememberUser, RememberMeType.Server, null);

                if (server != null) server.Login(rememberUser);
            }

            server.Start();

            Console.ReadLine();
        }
    }
}
