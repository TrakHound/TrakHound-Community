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
    }

    class Worker
    {
        public Worker()
        {
            TH_Database.DatabasePluginReader.ReadPlugins();

            var server = new Server();

            UserConfiguration rememberUser = RememberMe.Get(RememberMeType.Server);
            if (rememberUser != null)
            {
                RememberMe.Set(rememberUser, RememberMeType.Server);

                if (server != null) server.Login(rememberUser);
            }

            server.Start();

            Console.ReadLine();
        }
    }
}
