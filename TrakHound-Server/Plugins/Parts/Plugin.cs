// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins.Server;
using TrakHound.Tools;
using TrakHound.Tools.Web;
using TrakHound_Server.Plugins.GeneratedEvents;

namespace TrakHound_Server.Plugins.Parts
{
    public class Plugin : IServerPlugin
    {
        private object _lock = new object();

        private long lastSequence = 0;

        private DeviceConfiguration configuration;


        public string Name { get { return "Parts"; } }

        public void Initialize(DeviceConfiguration config)
        {
            var pc = Configuration.Read(config.Xml);
            if (pc != null)
            {
                config.CustomClasses.Add(pc);

                configuration = config;
                lastSequence = LoadStoredSequence();
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "MTCONNECT_AGENT_RESET")
                {
                    lastSequence = 0;
                    SaveStoredSequence(configuration, lastSequence);
                }

                if (data.Id == "GENERATED_EVENTS") { ProcessGeneratedEvents(data); }
            }
        }

        private void ProcessGeneratedEvents(EventData data)
        {
            var gEventItems = (List<GeneratedEvent>)data.Data02;
            gEventItems = gEventItems.OrderBy(o => o.CurrentValue.Timestamp).ToList();

            var pc = Configuration.Get(configuration);
            if (pc != null)
            {
                var infos = new List<PartInfo>();

                foreach (var partCountEvent in pc.Events)
                {
                    var matchedItems = gEventItems.FindAll(x => x.EventName == partCountEvent.EventName && x.CurrentValue != null && x.PreviousValue != null);
                    foreach (var gEvent in matchedItems)
                    {
                        // Test if current event value matches configured EventName
                        if (gEvent.CurrentValue != null && gEvent.CurrentValue.Value == String_Functions.UppercaseFirst(partCountEvent.EventValue.Replace('_', ' ')))
                        {
                            bool match = true;

                            // Test if previous event value matches configured PreviousEventName
                            if (!string.IsNullOrEmpty(partCountEvent.PreviousEventValue))
                            {
                                match = gEvent.PreviousValue.Value == String_Functions.UppercaseFirst(partCountEvent.PreviousEventValue.Replace('_', ' '));
                            }

                            if (match)
                            {
                                lock (_lock)
                                {
                                    var partInfo = PartInfo.Process(partCountEvent, gEvent, lastSequence);
                                    if (partInfo != null)
                                    {
                                        infos.Add(partInfo);

                                        lastSequence = partInfo.Sequence;
                                        SaveStoredSequence(configuration, lastSequence);
                                    }
                                }
                            }
                        }
                    }

                    if (infos.Count > 0)
                    {
                        SendPartInfos(infos);
                    }
                }
            }
        }


        public class SequenceInfo
        {
            public string UniqueId { get; set; }

            public long Sequence { get; set; }
        }

        private static void SaveStoredSequence(DeviceConfiguration config, long sequence)
        {
            string json = Properties.Settings.Default.PartsStoredSequences;
            if (!string.IsNullOrEmpty(json))
            {
                var sequenceInfos = JSON.ToType<List<SequenceInfo>>(json);
                if (sequenceInfos != null)
                {
                    int i = sequenceInfos.FindIndex(o => o.UniqueId == config.UniqueId);
                    if (i < 0)
                    {
                        var sequenceInfo = new SequenceInfo();
                        sequenceInfo.UniqueId = config.UniqueId;
                        sequenceInfos.Add(sequenceInfo);
                        i = sequenceInfos.FindIndex(o => o.UniqueId == config.UniqueId);
                    }

                    sequenceInfos[i].Sequence = sequence;

                    Properties.Settings.Default.PartsStoredSequences = JSON.FromList<SequenceInfo>(sequenceInfos);
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                var sequenceInfos = new List<SequenceInfo>();

                var sequenceInfo = new SequenceInfo();
                sequenceInfo.UniqueId = config.UniqueId;
                sequenceInfo.Sequence = sequence;
                sequenceInfos.Add(sequenceInfo);

                Properties.Settings.Default.PartsStoredSequences = JSON.FromList<SequenceInfo>(sequenceInfos);
                Properties.Settings.Default.Save();
            }
        }

        private long LoadStoredSequence()
        {
            string json = Properties.Settings.Default.PartsStoredSequences;
            if (!string.IsNullOrEmpty(json))
            {
                var sequenceInfos = JSON.ToType<List<SequenceInfo>>(json);
                if (sequenceInfos != null)
                {
                    var sequenceInfo = sequenceInfos.Find(o => o.UniqueId == configuration.UniqueId);
                    if (sequenceInfo != null) return sequenceInfo.Sequence;
                }
            }

            return 0;
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }

        private void SendPartInfos(List<PartInfo> infos)
        {
            var data = new EventData(this);
            data.Id = "PARTS";
            data.Data01 = configuration;
            data.Data02 = infos;
            SendData?.Invoke(data);
        }

    }

}
