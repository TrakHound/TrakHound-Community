using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TH_DeviceCompare_OEE
{
    public static class Tools
    {

        //static void UpdateNumberDisplay_GUI(DeviceDisplay dd, string key, double value)
        //{
        //    int cellIndex = dd.Group.Column.Cells.ToList().FindIndex(x => x.Link.ToLower() == key.ToLower());
        //    if (cellIndex >= 0)
        //    {
        //        Controls.NumberDisplay ctrl;

        //        object ddData = dd.Group.Column.Cells[cellIndex].Data;

        //        if (ddData == null)
        //        {
        //            ctrl = new Controls.NumberDisplay();
        //            dd.Group.Column.Cells[cellIndex].Data = ctrl;
        //        }
        //        else ctrl = (Controls.NumberDisplay)ddData;

        //        ctrl.Value_Format = "P2";
        //        ctrl.Value = value;
        //    }
        //}

    }
}
