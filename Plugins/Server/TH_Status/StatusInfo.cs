// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using MTConnect.Application.Components;
using MTConnect.Application.Streams;

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

        public MTConnect.Application.Components.DataItemCategory Category { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }

        public string Value1 { get; set; }
        public string Value2 { get; set; }

        #region "Get (Probe)"

        public static List<StatusInfo> GetList(MTConnect.Application.Components.ReturnData probe)
        {
            var result = new List<StatusInfo>();

            if (probe.Devices != null && probe.Devices.Count > 0)
            {
                var device = probe.Devices[0];

                foreach (var item in device.DataItems) result.Add(ProcessDataItem(item));

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

            foreach (var item in component.DataItems) result.Add(ProcessDataItem(item));

            return result;
        }

        private static StatusInfo ProcessDataItem(MTConnect.Application.Components.DataItem item)
        {
            var info = new StatusInfo();
            info.InfoType = StatusInfoType.MTConnect_Data_Item;

            info.Category = item.Category;
            info.Address = item.FullAddress;
            info.Name = item.Name;
            info.Id = item.Id;
            info.Type = item.Type;
            info.SubType = item.SubType;

            return info;
        }

        #endregion

        #region "Process (Current)"

        public static void ProcessList(MTConnect.Application.Streams.ReturnData current, List<StatusInfo> infos)
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

        private static void ProcessComponentStream(ComponentStream componentStream, List<StatusInfo> infos)
        {
            foreach (var item in componentStream.DataItems) ProcessDataItem(item, infos);
        }

        private static void ProcessDataItem(MTConnect.Application.Streams.DataItem item, List<StatusInfo> infos)
        {
            var info = infos.Find(x => x.Id == item.DataItemId);
            if (info != null)
            {
                info.Timestamp = item.Timestamp;
                info.Value1 = item.CDATA;
                if (info.Category == MTConnect.Application.Components.DataItemCategory.CONDITION)
                {
                    info.Value2 = item.Value;
                }
            }
        }

        #endregion

    }
}
