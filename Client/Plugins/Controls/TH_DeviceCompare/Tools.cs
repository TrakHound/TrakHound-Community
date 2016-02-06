using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

namespace TH_DeviceCompare
{
    public partial class DeviceCompare
    {

        static double GetDoubleFromDataRow(string key, DataRow row)
        {
            double result = 0;

            string val = null;
            if (row.Table.Columns.Contains(key)) if (row[key] != null) val = row[key].ToString();

            if (val != null) double.TryParse(val, out result);

            return result;
        }

    }
}
