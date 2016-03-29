using System;
using System.IO;

using TH_Updater;

namespace TestConsole
{
    class Program
    {

        static void Main(string[] args)
        {
            AppInfo info = Update.Get("http://www.feenux.com/trakhound/appinfo/th/test-appinfo.json");

            Update.Run(info);

            Console.ReadLine();
        }

    }
}
