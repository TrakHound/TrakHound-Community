using System;
using System.Collections.Generic;

namespace TH_ShiftTable
{
    public class ShiftRowInfo
    {
        public ShiftRowInfo()
        {
            genEventRowInfos = new List<GenEventRowInfo>();
        }

        public string id { get; set; }

        public ShiftDate date { get; set; }
        public string shift { get; set; }

        public int segmentId { get; set; }

        public ShiftTime start { get; set; }
        public ShiftTime end { get; set; }

        public string type { get; set; }

        public int totalTime { get; set; }

        public List<GenEventRowInfo> genEventRowInfos { get; set; }

        //public static string GetId(ShiftDate date, int id, int segmentId)
        //{
        //    return date.year.ToString("0000") + date.month.ToString("00") + date.day.ToString("00") + "_" + id.ToString("00") + "_" + segmentId.ToString("00");
        //}

        public ShiftRowInfo Copy()
        {
            ShiftRowInfo Result = new ShiftRowInfo();
            Result.id = id;
            Result.date = date;
            Result.shift = shift;
            Result.segmentId = segmentId;
            Result.start = start;
            Result.end = end;
            Result.type = type;
            Result.genEventRowInfos = genEventRowInfos;
            return Result;
        }
    }

    public class GenEventRowInfo
    {
        public string columnName { get; set; }
        public int seconds { get; set; }
    }

}
