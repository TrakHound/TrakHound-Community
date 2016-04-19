using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TH_Database;
using TH_Configuration;
using TH_OEE;
using TH_ShiftTable;

namespace TableCreator
{
    public class Data
    {
        public ShiftRowInfo ShiftInfo { get; set; }

        public static List<Data> Generate(Configuration config, DateTime start, DateTime end)
        {
            var result = new List<Data>();

            var sc = ShiftConfiguration.Get(config);
            var gec = TH_GeneratedData.GeneratedEvents.GeneratedEventsConfiguration.Get(config);

            DateTime timestamp = start;

            while (timestamp < end)
            {
                foreach (var shift in sc.shifts)
                {
                    foreach (var segment in shift.segments)
                    {
                        var data = new Data();
                        data.ShiftInfo = Shifts.Get(shift, segment, timestamp, gec);
                        

                        result.Add(data);
                    }
                }

                // Increment a day
                timestamp = timestamp.AddDays(1);
            }

            return result;
        }

    }
}
