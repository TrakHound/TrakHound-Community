using System;
using System.Collections.Generic;
using System.IO;

using TH_MTConnect.Components;
using TH_MTConnect.Streams;
using TH_Status;

namespace TestConsole
{
    class Program
    {

        static void Main(string[] args)
        {
            var infos = new List<StatusInfo>();

            var probeData = TH_MTConnect.Components.Requests.Get("http://127.0.0.1:5000/OKUMA.Lathe/probe", null, 2000, 1);
            if (probeData != null) infos = TH_Status.StatusInfo.GetList(probeData);

            var currentData = TH_MTConnect.Streams.Requests.Get("http://127.0.0.1:5000/OKUMA.Lathe/current", null, 2000, 1);
            if (currentData != null)
            {
                TH_Status.StatusInfo.ProcessList(currentData, infos);
                foreach (var info in infos)
                {
                    Console.WriteLine(info.Address + " :: " + info.Id + " = " + info.Value);
                }
            }

            Console.ReadLine();
        }

    }
}
