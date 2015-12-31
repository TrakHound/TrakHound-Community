using System;
using System.Diagnostics;
using System.Threading;

namespace AppStart
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            Login();

            // Reopen Server if it closed for any reason other than normal termination
            while (!OpenServer())
            {
                OpenServer();

                Thread.Sleep(500);
            }

        }


        static bool Login()
        {
            bool result = false;

            try
            {
                //string serverPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "trakhound-server-login.exe";

                string serverPath = @"F:\feenux\TrakHound\TrakHound\Server\TrakHound-Server-Login\bin\Debug\trakhound-server-login.exe";

                Process p = new Process();

                p.StartInfo.FileName = serverPath;
                p.Start();
                p.WaitForExit();
            }
            catch (Exception ex) { Console.WriteLine("AppStart.Login() :: " + ex.Message); }

            return result;
        }

        static bool OpenServer()
        {
            bool result = false;

            try
            {
                //string serverPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "trakhound-server-console.exe";

                string serverPath = @"F:\feenux\TrakHound\TrakHound\Server\TrakHound-Server-Console\bin\Debug\trakhound-server-console.exe";

                Process p = new Process();

                p.StartInfo.FileName = serverPath;

                p.Start();

                p.WaitForExit();

                Console.WriteLine("Server Exited with Code : " + p.ExitCode.ToString());

                if (p.ExitCode <= 0) result = true;

            }
            catch (Exception ex) { Console.WriteLine("AppStart.OpenServer() :: " + ex.Message); }

            return result;
        }

    }
}
