using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using TH_Database;
using TH_Configuration;
using TH_ShiftTable;

namespace TableCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true) // Loop indefinitely
            {
                Console.WriteLine("Enter input:"); // Prompt
                string line = Console.ReadLine(); // Get string from user

                ReadCommand(line);

                //if (line == "exit") // Check string
                //{
                //    break;
                //}
                Console.Write("You typed "); // Report output
                Console.Write(line.Length);
                Console.WriteLine(" character(s)");
            }
        }

        private static void ReadCommand(string s)
        {
            var args = s.Split(' ');

            string configPath = args[0];
            string command = args[1];

            switch (command.ToLower())
            {
                case "shifts":

                    DatabasePluginReader.ReadPlugins();

                    var config = Configuration.Read(configPath);
                    if (config != null)
                    {
                        Global.Initialize(config.Databases_Server);

                        DateTime start = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                        DateTime end = DateTime.Now;

                        if (args.Length > 2)
                        {
                            DateTime.TryParse(args[2], out start);
                            DateTime.TryParse(args[3], out end);
                        }

                        CreateShifts(config, start, end);
                    }
                    else Console.WriteLine("ERROR :: Configuration Not Found");

                    break;
            }
        }

        private static void CreateShifts(Configuration config, DateTime start, DateTime end)
        {
            var shiftsPlugin = new ShiftTable();
            var genEventsPlugin = new TH_GeneratedData.GeneratedEvents.GeneratedData();
            genEventsPlugin.Initialize(config);
            shiftsPlugin.Initialize(config);
            AddShiftRows(config, shiftsPlugin, start, end);
        }

        private static void AddShiftRows(Configuration config, ShiftTable plugin, DateTime start, DateTime end)
        {
            var infos = new List<ShiftRowInfo>();

            var sc = ShiftConfiguration.Get(config);
            var gec = TH_GeneratedData.GeneratedEvents.GeneratedEventsConfiguration.Get(config);

            DateTime timestamp = start;

            while (timestamp < end)
            {
                foreach (var shift in sc.shifts)
                {
                    foreach (var segment in shift.segments)
                    {
                        var info = new ShiftRowInfo();
                        info.shift = shift.name;
                        info.date = new ShiftDate(timestamp);
                        info.id = timestamp.ToString("yyyyMMdd") + "_" + shift.id.ToString("00") + "_" + segment.id.ToString("00");
                        info.totalTime = 300;
                        info.start = segment.beginTime;
                        info.end = segment.endTime;
                        info.start_utc = segment.beginTime.ToUTC();
                        info.end_utc = segment.endTime.ToUTC();
                        info.type = segment.type;
                        info.segmentId = segment.id;

                        var duration = segment.endTime - segment.beginTime;

                        
                        foreach (var e in gec.Events)
                        {
                            int[] seconds = GetRandomNumbers(Convert.ToInt32(duration.TotalSeconds), e.Values.Count + 1);
                            int secondsIndex = 0;

                            foreach (var value in e.Values)
                            {
                                var gInfo = new GenEventRowInfo();
                                gInfo.columnName = TH_ShiftTable.Tools.FormatColumnName(e, value);

                                gInfo.seconds = seconds[secondsIndex];
                                secondsIndex += 1;

                                info.genEventRowInfos.Add(gInfo);
                            }

                            var defaultInfo = new GenEventRowInfo();
                            defaultInfo.columnName = TH_ShiftTable.Tools.FormatColumnName(e.Name, 0, e.Default.Value);

                            defaultInfo.seconds = seconds[secondsIndex];
                            secondsIndex += 1;

                            info.genEventRowInfos.Add(defaultInfo);
                        }

                        infos.Add(info);
                    }
                }

                // Increment a day
                timestamp = timestamp.AddDays(1);
            }

            plugin.UpdateShiftRows(infos);       
        }

        static Random rnd = new Random();

        private static int[] GetRandomNumbers(int sum, int count)
        {
            var result = new int[count];

            // Get Random Numbers
            for (var x = 0; x <= count - 1; x++) result[x] = rnd.Next(0, sum);

            // Get total of generated numbers
            int total = 0;
            for (var x = 0; x <= result.Length - 1; x++) total += result[x];

            for (var x = 0; x <= result.Length - 1; x++) result[x] = Convert.ToInt32((result[x] * sum) / total);

            return result;
        }

        private static int ScaleNumber(int total, int sum, int value)
        {
            return (value * total) / sum;
        }
    }
}
