using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TrakHound.Configurations;
using TrakHound.Tools;
using TrakHound.Plugins;


namespace TH_StatusTimeline
{
    public partial class StatusTimeline
    {

        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;
        const System.Windows.Threading.DispatcherPriority PRIORITY_CONTEXT_IDLE = System.Windows.Threading.DispatcherPriority.ContextIdle;

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
