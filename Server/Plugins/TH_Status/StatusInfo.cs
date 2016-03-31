using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TH_MTConnect.Streams;
using TH_MTConnect.Components;

namespace TH_Status
{
    public enum StatusInfoType
    {
        MTConnect_Data_Item,
        Generated_Event,
        Variable,
    }

    public class StatusInfo
    {

        public StatusInfoType InfoType { get; set; }

        public string Address { get; set; }

        public DateTime Timestamp { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }

        public string Value { get; set; }


        #region "Get (Probe)"

        public static List<StatusInfo> GetList(TH_MTConnect.Components.ReturnData probe)
        {
            var result = new List<StatusInfo>();

            if (probe.Devices != null && probe.Devices.Count > 0)
            {
                var device = probe.Devices[0];

                foreach (var component in device.Components)
                {
                    result.AddRange(ProcessComponent(component));

                    result.AddRange(ProcessSubcomponents(component.Components));
                }
            }

            return result;
        }

        private static List<StatusInfo> ProcessSubcomponents(List<Component> subcomponents)
        {
            var result = new List<StatusInfo>();

            foreach (var subcomponent in subcomponents)
            {
                result.AddRange(ProcessComponent(subcomponent));

                result.AddRange(ProcessSubcomponents(subcomponent.Components));
            }

            return result;
        }

        private static List<StatusInfo> ProcessComponent(Component component)
        {
            var result = new List<StatusInfo>();

            foreach (var item in component.DataItems.Conditions) result.Add(ProcessDataItem(item));
            foreach (var item in component.DataItems.Events) result.Add(ProcessDataItem(item));
            foreach (var item in component.DataItems.Samples) result.Add(ProcessDataItem(item));

            return result;
        }

        private static StatusInfo ProcessDataItem(TH_MTConnect.Components.DataItem item)
        {
            var info = new StatusInfo();
            info.InfoType = StatusInfoType.MTConnect_Data_Item;

            info.Address = item.FullAddress;
            info.Name = item.Name;
            info.Id = item.Id;
            info.Type = item.Type;
            info.SubType = item.SubType;

            return info;
        }

        #endregion

        #region "Process (Current)"

        public static void ProcessList(TH_MTConnect.Streams.ReturnData current, List<StatusInfo> infos)
        {
            if (current.DeviceStreams != null && current.DeviceStreams.Count > 0)
            {
                var device = current.DeviceStreams[0];

                foreach (var componentStream in device.ComponentStreams)
                {
                    ProcessComponentStream(componentStream, infos);
                }
            }
        }

        private static void ProcessComponentStream(ComponentStream componentStream, List<StatusInfo> infos)
        {
            foreach (var item in componentStream.DataItems.Conditions) ProcessDataItem(item, infos);
            foreach (var item in componentStream.DataItems.Events) ProcessDataItem(item, infos);
            foreach (var item in componentStream.DataItems.Samples) ProcessDataItem(item, infos);
        }

        private static void ProcessDataItem(TH_MTConnect.Streams.DataItem item, List<StatusInfo> infos)
        {
            var info = infos.Find(x => x.Id == item.DataItemId);
            if (info != null)
            {
                info.Timestamp = item.Timestamp;
                info.Value = item.CDATA;
            }
        }

        #endregion

    }
}
