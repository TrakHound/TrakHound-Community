// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data.SQLite;
using System.IO;

using TrakHound.Logging;

namespace TrakHound.DataManagement
{
    public static class Database
    {
        public static Configuration Configuration { get; set; }

        public static bool Ping(Configuration config)
        {
            return File.Exists(GetPath(config));
        }

        static string GetName(Configuration config)
        {
            return Path.GetFileName(GetPath(config));
        }

        public static string GetPath(Configuration config)
        {
            return Environment.ExpandEnvironmentVariables(config.DatabasePath);
        }

        public static bool Create(object settings)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    try
                    {
                        string path = GetPath(config);

                        string dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        if (!File.Exists(path))
                        {
                            SQLiteConnection.CreateFile(path);
                            Logger.Log("SQLite Database File Created : " + path, LogLineType.Notification);
                        }
                        else Logger.Log("SQLite Database File Found : " + path, LogLineType.Notification);

                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message, LogLineType.Error);
                    }
                }
            }

            return result;
        }

        public static bool Drop(object settings)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    try
                    {
                        if (File.Exists(GetPath(config))) File.Delete(config.DatabasePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message, LogLineType.Error);
                    }
                }
            }

            return result;
        }

    }
}
