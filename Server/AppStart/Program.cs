using System;
using System.Diagnostics;
using System.Threading;

using Microsoft.Win32;
using System.Reflection;

using TH_Global;
using TH_WPF;

namespace AppStart
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //CheckForUpdates();

            // Reopen Server if it closed for any reason other than normal termination
            while (!OpenServer())
            {
                OpenServer();

                Thread.Sleep(2000);
            }
        }

        static bool OpenServer()
        {
            bool result = false;

            try
            {
                string serverPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "TrakHound-Server.exe";

                //string serverPath = @"F:\feenux\TrakHound\TrakHound\Server\TrakHound-Server\bin\Debug\trakhound-server.exe";

                Process p = new Process();
                p.StartInfo.FileName = serverPath;
                p.Start();
                p.WaitForExit();

                Logger.Log("Server Exited with Code : " + p.ExitCode.ToString());

                result = ProcessExitCode(p.ExitCode);

            }
            catch (Exception ex) { Logger.Log("AppStart.OpenServer() :: " + ex.Message); }

            return result;
        }

        static bool ProcessExitCode(int code)
        {
            if (code == 0) return true; // User Exit (Normal)
            if (code < 0) return true;
            if (code == 1) return false; // End Task (Task Manager)
            else return false; // Exit Codes with positive integers (Exceptions)
        }

        static void CheckForUpdates()
        {
            string[] keyNames = GetRegistryKeyNames();
            if (keyNames != null)
            {
                if (keyNames.Length > 0)
                {
                    bool? yesClicked = TH_WPF.MessageBox.Show(
                            "Updates are available for TrakHound Server" + Environment.NewLine + "Click Yes to Install",
                            "TrakHound Updates Available",
                            TH_WPF.MessageBoxButtons.YesNo
                            );

                    if (yesClicked == true)
                    {
                        OpenUpdater();
                    }
                }
            }
        }

        static void OpenUpdater()
        {
            try
            {
                string updaterPath = AppDomain.CurrentDomain.BaseDirectory + "\\Updater\\" + "TrakHound-Server-Updater.exe";

                //string updaterPath = @"F:\feenux\TrakHound\TrakHound\Server\TrakHound-Server-Updater\bin\Debug\trakhound-server-updater.exe";

                Process p = new Process();
                p.StartInfo.FileName = updaterPath;
                p.Start();
                p.WaitForExit();
            }
            catch (Exception ex) { Logger.Log("TrakHound-Server-Updater.OpenUpdater() :: " + ex.Message); }
        }

        public static string[] GetRegistryKeyNames()
        {
            string[] Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("Server-Updates");

                Result = updatesKey.GetSubKeyNames();
            }
            catch (Exception ex)
            {
                Logger.Log("AutoUpdater_GetRegistryKeys() : " + ex.Message);
            }

            return Result;
        }

    }
}
