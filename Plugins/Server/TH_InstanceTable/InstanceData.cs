// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace TH_InstanceData
{
    public class InstanceData : IDisposable
    {
        public InstanceData() { Values = new List<DataItemValue>(); }

        public DateTime Timestamp { get; set; }
        public Int64 Sequence { get; set; }
        public Int64 AgentInstanceId { get; set; }

        public class DataItemValue
        {
            public string Id { get; set; }
            public string Value { get; set; }

            public DataItemValue Copy()
            {
                var result = new DataItemValue();
                result.Id = Id;
                result.Value = Value;
                return result;
            }
        }

        public InstanceData Copy()
        {
            InstanceData result = new InstanceData();
            result.Timestamp = Timestamp;
            result.Sequence = Sequence;
            result.AgentInstanceId = AgentInstanceId;

            foreach (InstanceData.DataItemValue val in Values)
            {
                if (val != null)
                {
                    InstanceData.DataItemValue newval = new InstanceData.DataItemValue();
                    newval.Id = val.Id;
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

    class InstanceVariableData
    {
        public string Id { get; set; }
        public object Value { get; set; }
        public DateTime Timestamp { get; set; }
        public Int64 Sequence { get; set; }
    }

    public class CurrentInstanceData
    {
        public MTConnect.Application.Streams.ReturnData CurrentData { get; set; }
        public InstanceData Data { get; set; }
    }
}
