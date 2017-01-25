// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using MTConnectDevices = MTConnect.MTConnectDevices;
using MTConnectStreams = MTConnect.MTConnectStreams;
using System;
using System.Collections.Generic;


namespace TrakHound_Server.Plugins.Status
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

        public MTConnect.DataItemCategory Category { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }

        public string Value1 { get; set; }
        public string Value2 { get; set; }

        internal string PreviousValue1 { get; set; }
        internal string PreviousValue2 { get; set; }


        #region "Get (Probe)"

        public static List<StatusInfo> GetList(MTConnectDevices.Document probe)
        {
            var result = new List<StatusInfo>();

            if (probe.Devices != null && probe.Devices.Count > 0)
            {
                var device = probe.Devices[0];

                foreach (var item in device.DataItems) result.Add(ProcessDataItem(item));

                foreach (var component in device.Components.Components)
                {
                    result.AddRange(ProcessComponent(component));

                    result.AddRange(ProcessSubcomponents(component.SubComponents.Components));
                }
            }

            return result;
        }

        private static List<StatusInfo> ProcessSubcomponents(List<MTConnectDevices.Component> subcomponents)
        {
            var result = new List<StatusInfo>();

            foreach (var subcomponent in subcomponents)
            {
                result.AddRange(ProcessComponent(subcomponent));

                result.AddRange(ProcessSubcomponents(subcomponent.SubComponents.Components));
            }

            return result;
        }

        private static List<StatusInfo> ProcessComponent(MTConnectDevices.Component component)
        {
            var result = new List<StatusInfo>();

            foreach (var item in component.DataItems) result.Add(ProcessDataItem(item));

            return result;
        }

        private static StatusInfo ProcessDataItem(MTConnectDevices.DataItem item)
        {
            var info = new StatusInfo();
            info.InfoType = StatusInfoType.MTConnect_Data_Item;

            info.Category = item.Category;
            info.Address = item.TypePath;
            info.Name = item.Name;
            info.Id = item.Id;
            info.Type = item.Type;
            info.SubType = item.SubType;

            return info;
        }

        #endregion

        #region "Process (Current)"

        public static void ProcessList(MTConnectStreams.Document current, List<StatusInfo> infos)
        {
            if (current.DeviceStreams != null && current.DeviceStreams.Count > 0)
            {
                var device = current.DeviceStreams[0];

                foreach (var item in device.DataItems) ProcessDataItem(item, infos);

                foreach (var componentStream in device.ComponentStreams)
                {
                    ProcessComponentStream(componentStream, infos);
                }
            }
        }

        private static void ProcessComponentStream(MTConnectStreams.ComponentStream componentStream, List<StatusInfo> infos)
        {
            foreach (var item in componentStream.DataItems) ProcessDataItem(item, infos);
        }

        private static void ProcessDataItem(MTConnectStreams.DataItem item, List<StatusInfo> infos)
        {
            var info = infos.Find(x => x.Id == item.DataItemId);
            if (info != null)
            {
                info.Timestamp = item.Timestamp;

                info.PreviousValue1 = info.Value1;
                info.Value1 = item.CDATA;

                if (info.Category == MTConnect.DataItemCategory.CONDITION)
                {
                    info.PreviousValue2 = info.Value2;
                    info.Value2 = ((MTConnectStreams.Condition)item).ConditionValue.ToString();
                }
            }
        }

        #endregion

    }
}
