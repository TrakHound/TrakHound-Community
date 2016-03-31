// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace TH_GeneratedData.GeneratedEvents
{
    public class MultiTrigger
    {
        public List<Trigger> Triggers { get; set; }

        public static MultiTrigger Read(XmlNode Node)
        {
            var result = new MultiTrigger();
            result.Triggers = new List<Trigger>();

            foreach (XmlNode triggerNode in Node.ChildNodes)
            {
                if (triggerNode.NodeType == XmlNodeType.Element)
                {
                    switch (triggerNode.Name.ToLower())
                    {
                        case "trigger":

                            var trigger = Trigger.Read(triggerNode);
                            if (trigger != null) result.Triggers.Add(trigger);

                            break;
                    }
                }
            }

            return result;
        }
    }
}
