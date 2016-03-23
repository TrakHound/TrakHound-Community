using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using TH_Global;
using TH_Global.Functions;

namespace TestConsole1
{
    class Program
    {
        static void Main(string[] args)
        {
            string appDomain = "TestConsole.vshost";

            var reader = new Logger.LogReader(appDomain);
            reader.LineAdded += Reader_LineAdded;

            Console.ReadLine();
        }

        static DateTime previousTimestamp = DateTime.MinValue;

        private static void Reader_LineAdded(Logger.Line line)
        {
            Console.WriteLine(line.Text);
        }
    }
}
