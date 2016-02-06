using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TH_DeviceCompare
{
    public partial class DeviceCompare
    {

        // Number Display
        void UpdateNumberDisplay(DeviceDisplay dd, string key, double value)
        {
            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, string, double>(UpdateNumberDisplay_GUI), Priority_Context, new object[] { dd, key, value });
        }

        static void UpdateNumberDisplay_GUI(DeviceDisplay dd, string key, double value)
        {
            int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == key.ToLower());
            if (cellIndex >= 0)
            {
                Controls.NumberDisplay ctrl;

                object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                if (ddData == null)
                {
                    ctrl = new Controls.NumberDisplay();
                    dd.ComparisonGroup.column.Cells[cellIndex].Data = ctrl;
                }
                else ctrl = (Controls.NumberDisplay)ddData;

                ctrl.Value_Format = "P2";
                ctrl.Value = value;
            }
        }

    }
}
