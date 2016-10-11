// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace TrakHound_Server.Plugins.Instances
{
    public class Instance : IDisposable
    {
        private object _lock = new object();

        public DateTime Timestamp { get; set; }
        public long Sequence { get; set; }
        public long AgentInstanceId { get; set; }

        public class DataItemValue
        {
            private object _lock = new object();

            public string Id { get; set; }
            public string Type { get; set; }
            public string SubType { get; set; }
            public string Value { get; set; }
            public long ChangedSequence { get; set; }

            public DataItemValue Copy()
            {
                lock (_lock)
                {
                    var result = new DataItemValue();
                    result.Id = Id;
                    result.Type = Type;
                    result.SubType = SubType;
                    result.Value = Value;
                    result.ChangedSequence = ChangedSequence;
                    return result;
               }
            }
        }

        public Instance() { Values = new List<DataItemValue>(); }

        public Instance Copy()
        {
            lock (_lock)
            {
                var result = new Instance();
                result.Timestamp = Timestamp;
                result.Sequence = Sequence;
                result.AgentInstanceId = AgentInstanceId;

                foreach (var val in Values)
                {
                    if (val != null)
                    {
                        result.Values.Add(val.Copy());
                    }
                }

                return result;
            }
        }

        public List<DataItemValue> Values { get; set; }

        public void Dispose()
        {
            lock (_lock)
            {
                Values.Clear();
                Values = null;
            }
        }

        ~Instance()
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

                if (item.Category == MTConnect.DataItemCategory.CONDITION)
                {
                    data.Value = ((MTConnect.Application.Streams.Condition)item).ConditionValue.ToString();
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

    public class CurrentInstance
    {
        public MTConnect.Application.Streams.ReturnData CurrentData { get; set; }
        public Instance Instance { get; set; }
    }
}
