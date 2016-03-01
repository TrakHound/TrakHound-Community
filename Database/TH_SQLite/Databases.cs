using System;
using System.IO;
using System.Data.SQLite;

using TH_Global;

namespace TH_SQLite
{
    public partial class Plugin
    {

        static string GetDatabaseName(SQLite_Configuration config)
        {
            return Path.GetFileName(config.DatabasePath);
        }

        public static string GetDatabasePath(SQLite_Configuration config)
        {
            return Environment.ExpandEnvironmentVariables(config.DatabasePath);
        }

        public bool Database_Create(object settings)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    try
                    {
                        string path = GetDatabasePath(config);

                        string dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        if (!File.Exists(path))
                        {
                            SQLiteConnection.CreateFile(path);
                            Logger.Log("SQLite Database File Created : " + path);
                        }
                        else Logger.Log("SQLite Database File Found : " + path);

                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message);
                    }
                }
            }

            return result;
        }

        public bool Database_Drop(object settings)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    try
                    {
                        if (File.Exists(GetDatabasePath(config))) File.Delete(config.DatabasePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message);
                    }
                }
            }

            return result;
        }

    }
}
