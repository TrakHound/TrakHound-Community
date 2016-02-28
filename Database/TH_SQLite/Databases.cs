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
                        if (!File.Exists(config.DatabasePath)) SQLiteConnection.CreateFile(config.DatabasePath);
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
                        if (File.Exists(config.DatabasePath)) File.Delete(config.DatabasePath);
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
