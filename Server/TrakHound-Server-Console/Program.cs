// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
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
        Server server;

        public Worker()
        {
            TH_Database.DatabasePluginReader.ReadPlugins();

            server = new Server();
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

            Console.ReadLine();
        }

        private void FileSystemWatcher_UserLogin_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("UserLogin File Changed!");

            if (server != null) server.Login();
        }

    }
}
