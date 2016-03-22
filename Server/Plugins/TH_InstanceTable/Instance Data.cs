using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TH_InstanceTable
{
    public partial class InstanceTable
    {

        public class InstanceData : IDisposable
        {
            public InstanceData() { values = new List<Value>(); }

            public DateTime timestamp { get; set; }
            public Int64 sequence { get; set; }
            public Int64 agentInstanceId { get; set; }

            public class Value
            {
                public string id { get; set; }
                public string value { get; set; }

                public Value Copy()
                {
                    var result = new Value();
                    result.id = id;
                    result.value = value;
                    return result;
                }
            }

            public InstanceData Copy()
            {
                InstanceData result = new InstanceData();
                result.timestamp = timestamp;
                result.sequence = sequence;
                result.agentInstanceId = agentInstanceId;

                foreach (InstanceData.Value val in values)
                {
                    if (val != null)
                    {
                        InstanceData.Value newval = new InstanceData.Value();
                        newval.id = val.id;
                        newval.value = val.value;
                        result.values.Add(newval.Copy());
                    }
                }

                return result;
            }

            public List<Value> values;

            public void Dispose()
            {
                values.Clear();
                values = null;
            }

            ~InstanceData()
            {
                Dispose();
            }
        }

        class InstanceVariableData
        {
            public string id { get; set; }
            public object value { get; set; }
            public DateTime timestamp { get; set; }
            public Int64 sequence { get; set; }
        }

        public class CurrentInstanceData
        {
            public TH_MTConnect.Streams.ReturnData currentData { get; set; }
            public InstanceData data { get; set; }
        }

    }
}
