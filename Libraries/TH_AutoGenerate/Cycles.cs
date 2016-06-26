using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using TH_Configuration;
using TH_Global.Functions;

using MTConnect.Application.Components;

namespace TH_AutoGenerate
{
    public static class Cycles
    {
        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/CycleEventName", "value", "program_execution");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/StoppedEventValue", "value", "Program Stopped");

            var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PROGRAM");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/CycleNameLink", "value", item.Id);

            // Add Production Types
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/ProductionTypes/Value", "attributes", "id||00;name||Program Started;type||IN_PRODUCTION;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/ProductionTypes/Value", "attributes", "id||01;name||Program Paused;type||PAUSED;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/ProductionTypes/Value", "attributes", "id||02;name||Program Stopped;type||STOPPED;");

            // Add Override Values
            var ovrItems = probeItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PATH_FEEDRATE_OVERRIDE" && x.SubType != "JOG");
            if (ovrItems != null)
            {
                for (var x = 0; x < ovrItems.Count; x++)
                {
                    DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/OverrideLinks/Value", "attributes", "id||" + x.ToString("00"));
                    DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/OverrideLinks/Value", "value", ovrItems[x].Id);
                }
            }
        }
    }
}
