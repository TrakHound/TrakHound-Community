using System;
using System.Collections.ObjectModel;
using System.Data;

using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Cycles
{
    public class CycleRowInfo
    {
        public string CycleId { get; set; }
        public string CycleInstanceId { get; set; }

        public string Name { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

        public TimeSpan Duration { get; set; }

        public static CycleRowInfo FromDataTable(string cycleId, DataTable dt)
        {
            var result = new CycleRowInfo();

            var dv = dt.AsDataView();
            dv.RowFilter = "cycle_id='" + cycleId + "'";
            var temp_dt = dv.ToTable();

            if (temp_dt.Rows.Count > 0)
            {
                result = FromDataRow(temp_dt.Rows[0]);

                DateTime start = result.StartTime;
                DateTime stop = result.StopTime;
                TimeSpan duration = TimeSpan.Zero;

                foreach (DataRow row in temp_dt.Rows)
                {
                    var detail = CycleRowDetail.FromDataRow(row);
                    if (detail != null)
                    {
                        if (detail.StartTime < start) start = detail.StartTime;
                        if (detail.StopTime > stop) stop = detail.StopTime;
                        duration += detail.Duration;

                        result.CycleRowDetails.Add(detail);
                    }
                }

                result.StartTime = start;
                result.StopTime = stop;
                result.Duration = duration;
            }

            return result;
        }

        public static CycleRowInfo FromDataRow(DataRow row)
        {
            CycleRowInfo result = null;

            string shiftId = DataTable_Functions.GetRowValue("shift_id", row);

            string cycleName = DataTable_Functions.GetRowValue("name", row);
            string cycleId = DataTable_Functions.GetRowValue("cycle_id", row);
            string cycleInstanceId = DataTable_Functions.GetRowValue("instance_id", row);

            DateTime startTimeUtc = DateTime_Functions.Parse(DataTable_Functions.GetRowValue("start_time", row));
            DateTime stopTimeUtc = DateTime_Functions.Parse(DataTable_Functions.GetRowValue("stop_time", row));
            TimeSpan duration = TimeSpan_Functions.ParseSeconds(DataTable_Functions.GetRowValue("duration", row));

            if (shiftId != null && cycleName != null)
            {
                result = new CycleRowInfo();

                result.CycleId = cycleId;
                result.CycleInstanceId = cycleInstanceId;

                result.Name = cycleName;

                result.StartTime = startTimeUtc;
                result.StopTime = stopTimeUtc;
                result.Duration = duration;
            }

            return result;
        }

        ObservableCollection<CycleRowDetail> cycleRowDetails;
        public ObservableCollection<CycleRowDetail> CycleRowDetails
        {
            get
            {
                if (cycleRowDetails == null)
                {
                    cycleRowDetails = new ObservableCollection<CycleRowDetail>();
                }

                return cycleRowDetails;
            }

            set
            {
                cycleRowDetails = value;
            }
        }

        public class CycleRowDetail
        {
            public string CycleId { get; set; }
            public string CycleInstanceId { get; set; }

            public string EventName { get; set; }

            public DateTime StartTime { get; set; }
            public DateTime StopTime { get; set; }

            public TimeSpan Duration { get; set; }

            public static CycleRowDetail FromDataRow(DataRow row)
            {
                CycleRowDetail result = null;

                string eventName = DataTable_Functions.GetRowValue("event", row);
                string cycleId = DataTable_Functions.GetRowValue("cycle_id", row);
                string cycleInstanceId = DataTable_Functions.GetRowValue("instance_id", row);

                DateTime startTimeUtc = DateTime_Functions.Parse(DataTable_Functions.GetRowValue("start_time", row));
                DateTime stopTimeUtc = DateTime_Functions.Parse(DataTable_Functions.GetRowValue("stop_time", row));
                TimeSpan duration = TimeSpan_Functions.ParseSeconds(DataTable_Functions.GetRowValue("duration", row));

                if (eventName != null)
                {
                    result = new CycleRowDetail();

                    result.CycleId = cycleId;
                    result.CycleInstanceId = cycleInstanceId;

                    result.EventName = eventName;

                    result.StartTime = startTimeUtc;
                    result.StopTime = stopTimeUtc;
                    result.Duration = duration;
                }

                return result;
            }


        }

    }
}
