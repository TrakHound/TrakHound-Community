// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class Overrides
    {
        public static void Add(DataTable table, List<DataItem> dataItems)
        {
            int i = 0;

            // Add Feedrate Override Values
            var items = dataItems.FindAll(x => (x.Category == DataItemCategory.EVENT && x.Type == "PATH_FEEDRATE_OVERRIDE" && x.SubType == "PROGRAMMED") ||
                (x.Category == DataItemCategory.SAMPLE && x.Type == "PATH_FEEDRATE" && x.Units == "PERCENT"));
            foreach (var item in items)
            {
                string format = "id||{0};name||{1};link||{2};type||feedrate_override;";
                string attributes = string.Format(format, i.ToString("00"), "Feedrate Override " + (i + 1).ToString(), item.Id);

                DeviceConfiguration.EditTable(table, "/Overrides/Override||" + i.ToString("00"), null, attributes);
                i++;
            }

            // Add Spindle Override Values
            items = dataItems.FindAll(x => (x.Category == DataItemCategory.EVENT && x.Type == "ROTARY_VELOCITY_OVERRIDE" && x.SubType == "PROGRAMMED") ||
                (x.Category == DataItemCategory.SAMPLE && x.Type == "SPINDLE_SPEED" && x.Units == "PERCENT"));
            foreach (var item in items)
            {
                string format = "id||{0};name||{1};link||{2};type||spindle_override;";
                string attributes = string.Format(format, i.ToString("00"), "Spindle Override " + (i + 1).ToString(), item.Id);

                DeviceConfiguration.EditTable(table, "/Overrides/Override||" + i.ToString("00"), null, attributes);
                i++;
            }

            // Add Rapid Override Values
            items = dataItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PATH_FEEDRATE_OVERRIDE" && x.SubType == "RAPID");
            foreach (var item in items)
            {
                string format = "id||{0};name||{1};link||{2};type||rapid_override;";
                string attributes = string.Format(format, i.ToString("00"), "Rapid Override " + (i + 1).ToString(), item.Id);

                DeviceConfiguration.EditTable(table, "/Overrides/Override||" + i.ToString("00"), null, attributes);
                i++;
            }
        }
    }
}

