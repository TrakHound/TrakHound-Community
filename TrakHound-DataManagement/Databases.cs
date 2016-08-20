using System;
using System.IO;
using System.Data.SQLite;

using TrakHound.Tools;
using TrakHound.Databases;
using TrakHound.Plugins;
using TrakHound.Plugins.Database;
using TrakHound.Logging;

namespace TrakHound_DataManagement
{
    public partial class Database
    {

        static string GetName(Configuration config)
        {
            return Path.GetFileName(config.DatabasePath);
        }

        public static string GetPath(Configuration config)
        {
            return Environment.ExpandEnvironmentVariables(config.DatabasePath);
        }

        public bool Create(object settings)
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

        public bool Drop(object settings)
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
