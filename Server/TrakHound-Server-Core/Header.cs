using System;
using System.Linq;
using System.IO;

namespace TrakHound_Server_Core
{
    public partial class Server
    {

        void PrintHeader()
        {
            Console.WriteLine("------------------------------------------------");

            string trakhoundlogo_path = AppDomain.CurrentDomain.BaseDirectory + @"\" + "Header.txt";

            if (File.Exists(trakhoundlogo_path))
            {
                using (StreamReader reader = new StreamReader(trakhoundlogo_path))
                {
                    string trakhoundlogo = reader.ReadToEnd();

                    if (trakhoundlogo.Contains("[v]")) trakhoundlogo = trakhoundlogo.Replace("[v]", GetVersion());

                    Console.WriteLine(trakhoundlogo);
                }
            }

            Console.WriteLine("------------------------------------------------");
        }

        static string GetVersion()
        {
            // Build Information
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            return "v" + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();
        }

    }
}
