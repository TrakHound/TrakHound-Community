// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins.Server;
using TrakHound.Tools.Web;
using TrakHound_Server.Plugins.GeneratedEvents;

namespace TrakHound_Server.Plugins.Parts
{
    public class Plugin : IServerPlugin
    {

        public string Name { get { return "Parts"; } }

        private DateTime lastTimestamp = DateTime.MinValue;

        private long lastSequence = 0;


        private DeviceConfiguration configuration;

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
                if (data.Id == "GENERATED_EVENTS")
                {
                    var genEventItems = (List<GeneratedEvent>)data.Data02;

                    var pc = Configuration.Get(configuration);
                    if (pc != null)
                    {
                        genEventItems = genEventItems.FindAll(x => x.EventName == pc.PartsEventName);

                        var infos = new List<PartInfo>();

                        foreach (var item in genEventItems)
                        {
                            PartInfo info = PartInfo.Get(configuration, item);
                            if (info != null && info.Count > 0 && info.Sequence > lastSequence)
                            {
                                lastTimestamp = info.Timestamp;
                                infos.Add(info);

                                lastSequence = info.Sequence;
                                SaveStoredSequence(info);
                            }
                        }

                        if (infos.Count > 0)
                        {
                            SendPartInfos(infos);
                        }
                    }
                }
            }
        }

        private void SaveStoredSequence(PartInfo info)
        {
            string json = Properties.Settings.Default.PartsStoredSequences;
            if (!string.IsNullOrEmpty(json))
            {
                var sequenceInfos = JSON.ToType<List<PartInfo.SequenceInfo>>(json);
                if (sequenceInfos != null)
                {
                    int i = sequenceInfos.FindIndex(o => o.UniqueId == configuration.UniqueId);
                    if (i < 0)
                    {
                        var sequenceInfo = new PartInfo.SequenceInfo();
                        sequenceInfo.UniqueId = configuration.UniqueId;
                        sequenceInfos.Add(sequenceInfo);
                        i = sequenceInfos.FindIndex(o => o.UniqueId == configuration.UniqueId);
                    }

                    sequenceInfos[i].Sequence = info.Sequence;

                    Properties.Settings.Default.PartsStoredSequences = JSON.FromList<PartInfo.SequenceInfo>(sequenceInfos);
                    Properties.Settings.Default.Save();
                }
            }
        }

        private long LoadStoredSequence()
        {
            string json = Properties.Settings.Default.PartsStoredSequences;
            if (!string.IsNullOrEmpty(json))
            {
                var sequenceInfos = JSON.ToType<List<PartInfo.SequenceInfo>>(json);
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
