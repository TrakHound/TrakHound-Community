using System;
using System.Linq;

using TrakHound.Configurations;
using TrakHound.Plugins;

namespace TH_OeeStatus
{
    public partial class OeeStatus
    {

        void Update(EventData data)
        {
            if (data != null && data.Id != null && data.Data01 != null)
            {
                var config = data.Data01 as DeviceConfiguration;
                if (config != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        int index = Rows.ToList().FindIndex(x => x.Configuration.UniqueId == config.UniqueId);
                        if (index >= 0)
                        {
                            var row = Rows[index];
                            row.UpdateData(data);   
                        }
                    }));
                }
            }
        }
        
    }
}
