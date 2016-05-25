// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_GeneratedData.GeneratedEvents;

namespace TH_ShiftTable
{
    public class Tools
    {

        #region "DateTime"

        public static DateTime GetDateTimeFromShiftTime(ShiftTime time, ShiftDate date)
        {
            return getDateTimeFromShift(time, date, true);
        }

        public static DateTime GetDateTimeFromShiftTime(ShiftTime time, ShiftDate date, bool addDayValue)
        {
            return getDateTimeFromShift(time, date, addDayValue);
        }

        public static DateTime GetDateTimeFromShiftTime(ShiftTime time, ShiftDate date, int dayOffset)
        {
            DateTime Result = new DateTime(date.year, date.month, date.day, time.adjHour, time.minute, time.second);
            Result = Result.AddDays(dayOffset);

            return Result;
        }

        public static DateTime getDateTimeFromShift(ShiftTime time, ShiftDate date, bool addDayValue)
        {
            return new DateTime(date.year, date.month, date.day, time.adjHour, time.minute, time.second);
        }

        #endregion

        #region "Formatting"

        public static string GetShiftId(ShiftDate date, Segment segment)
        {
            return date.year.ToString("0000") + date.month.ToString("00") + date.day.ToString("00") + "_" + segment.shift.id.ToString("00") + "_" + segment.id.ToString("00");
        }

        public static string FormatColumnName(string eventName, int resultNumValue, string resultValue)
        {
            return eventName.ToUpper() + "__" + resultValue.Replace(' ', '_').ToUpper();
        }

        public static string FormatColumnName(Event genEvent, Value value)
        {
            return genEvent.Name.ToUpper() + "__" + value.Result.Value.Replace(' ', '_').ToUpper();
        }

        #endregion

        #region "Logging"

        static bool Debug = true;

        public static void Log(string line)
        {
            if (Debug) TH_Global.Logger.Log(line);
        }

        #endregion


        /// <summary>
        /// Get Total Time using ShiftRowInfo.End and currentData from last MTC Current Request
        /// </summary>
        /// <param name="info"></param>
        /// <param name="currentData"></param>
        /// <returns></returns>
        public static int GetTotalShiftSeconds(ShiftRowInfo info, MTConnect.Application.Streams.ReturnData currentData)
        {
            int Result = 0;

            DateTime current = currentData.Header.CreationTime.ToLocalTime();

            DateTime start = Tools.GetDateTimeFromShiftTime(info.Start, info.Date, info.Start.dayOffset);
            DateTime end = Tools.GetDateTimeFromShiftTime(info.End, info.Date, info.End.dayOffset);

            if (end < start) end = end.AddDays(1);

            // If currently still within the shift segment
            if (current < end)
            {
                double total = (current - start).TotalSeconds;
                if (total >= 0) Result = Convert.ToInt32(total);
            }
            // If past current shift
            else
            {
                double total = (end - start).TotalSeconds;
                if (total >= 0) Result = Convert.ToInt32(total);
            }

            return Result;
        }    


    }
}
