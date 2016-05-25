// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;

namespace TH_GeneratedData.GeneratedEvents
{
    public class Value
    {
        public Value() { Triggers = new List<Trigger>(); MultiTriggers = new List<MultiTrigger>(); }

        public List<Trigger> Triggers { get; set; }
        public List<MultiTrigger> MultiTriggers { get; set; }
        public Return Result { get; set; }

        public static Value Read(XmlNode node)
        {
            var result = new Value();
            result.Triggers = new List<Trigger>();
            result.MultiTriggers = new List<MultiTrigger>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                {
                    switch (childNode.Name.ToLower())
                    {
                        case "triggers":

                            foreach (XmlNode triggerNode in childNode)
                            {
                                if (triggerNode.NodeType == XmlNodeType.Element)
                                {

                                    switch (triggerNode.Name.ToLower())
                                    {

                                        case "trigger":

                                            var trigger = Trigger.Read(triggerNode);
                                            if (trigger != null) result.Triggers.Add(trigger);

                                            break;


                                        case "multitrigger":

                                            var multiTrigger = MultiTrigger.Read(triggerNode);
                                            if (multiTrigger != null) result.MultiTriggers.Add(multiTrigger);

                                            break;
                                    }
                                }
                            }

                            break;

                        case "result":

                            var returnResult = new Return();

                            if (childNode.Attributes != null)
                                if (childNode.Attributes["numval"] != null)
                                    returnResult.NumVal = Convert.ToInt16(childNode.Attributes["numval"].Value);

                            returnResult.Value = childNode.InnerText;

                            result.Result = returnResult;

                            break;
                    }
                }
            }

            return result;
        }
    }
}
