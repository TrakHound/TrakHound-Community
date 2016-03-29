using System;

using TH_Updater;

namespace TestConsole
{
    class Program
    {

        static void Main(string[] args)
        {
            var info = AppInfo.Get("http://www.feenux.com/trakhound/appinfo/th/bundle-appinfo.json");
            if (info != null) { }

            Console.ReadLine();
        }

    }
}
