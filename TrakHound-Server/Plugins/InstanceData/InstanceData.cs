// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace TrakHound_Server.Plugins.Instances
{
    public class InstanceData : IDisposable
    {
        public InstanceData() { Values = new List<DataItemValue>(); }

        public DateTime Timestamp { get; set; }
        public long Sequence { get; set; }
        public long AgentInstanceId { get; set; }

        public class DataItemValue
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string SubType { get; set; }
            public string Value { get; set; }

            public DataItemValue Copy()
            {
                var result = new DataItemValue();
                result.Id = Id;
                result.Type = Type;
                result.SubType = SubType;
                result.Value = Value;
                return result;
            }
        }

        public InstanceData Copy()
        {
            var result = new InstanceData();
            result.Timestamp = Timestamp;
            result.Sequence = Sequence;
            result.AgentInstanceId = AgentInstanceId;

            foreach (var val in Values)
            {
                if (val != null)
                {
                    var newval = new DataItemValue();
                    newval.Id = val.Id;
                    newval.Type = val.Type;
                    newval.SubType = val.SubType;
                    newval.Value = val.Value;
                    result.Values.Add(newval.Copy());
                }
            }

            return result;
        }

        public List<DataItemValue> Values { get; set; }

        public void Dispose()
        {
            Values.Clear();
            Values = null;
        }

        ~InstanceData()
        {
            Dispose();
        }
    }

    class VariableData
    {
        public string Id { get; set; }

        public string Type { get; set; }
        public string SubType { get; set; }

        public object Value { get; set; }
        public DateTime Timestamp { get; set; }
        public long Sequence { get; set; }

        public static List<VariableData> Get(List<MTConnect.Application.Streams.DataItem> dataItems)
        {
            var result = new List<VariableData>();

            foreach (var item in dataItems)
            {
                var data = new VariableData();
                data.Id = item.DataItemId;

                data.Type = item.Type;
                data.SubType = item.SubType;

                if (item.Category == MTConnect.Application.Streams.DataItemCategory.CONDITION)
                {
                    data.Value = item.Value;
                }
                else data.Value = item.CDATA;

                data.Timestamp = item.Timestamp;
                data.Sequence = item.Sequence;

                result.Add(data);
            }

            // Sort List by timestamp ASC
            result.Sort((x, y) => x.Timestamp.Second.CompareTo(y.Timestamp.Second));

            return result;
        }
    }

    public class CurrentInstanceData
    {
        public MTConnect.Application.Streams.ReturnData CurrentData { get; set; }
        public InstanceData Data { get; set; }
    }
}
