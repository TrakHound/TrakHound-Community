// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using TrakHound_Server.Plugins.Instances;

namespace TrakHound_Server.Plugins.GeneratedEvents
{
    public class Event
    {
        public Event()
        {
            Values = new List<Value>();
            CaptureItems = new List<CaptureItem>();
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public List<Value> Values { get; set; }

        public Return Default { get; set; }

        public List<CaptureItem> CaptureItems { get; set; }


        public Return CurrentValue { get; set; }

        public Return PreviousValue { get; set; }


        public Return Process(Instance instance)
        {
            bool conditionsMet = false;

            // Set Result to Default Value (in case nothing changes)
            Return result = Default;

            foreach (Value value in Values)
            {
                conditionsMet = false;

                foreach (var trigger in value.Triggers)
                {
                    var instanceValue = instance.Values.Find(x => FindTrigger(x, trigger));
                    if (instanceValue != null)
                    {
                        conditionsMet = trigger.Process(instanceValue);
                    }

                    // if any triggers are not met then break
                    if (!conditionsMet) break;
                }

                if (conditionsMet || value.Triggers.Count == 0)
                {
                    foreach (MultiTrigger multiTrigger in value.MultiTriggers)
                    {
                        foreach (Trigger trigger in multiTrigger.Triggers)
                        {
                            var instanceValue = instance.Values.Find(x => FindTrigger(x, trigger));
                            if (instanceValue != null)
                            {
                                conditionsMet = trigger.Process(instanceValue);
                            }

                            // if any trigger is met then break (only one has to be met)
                            if (conditionsMet) break;
                        }

                        // if none of the triggers were met then break (at least one has to be met)
                        if (!conditionsMet) break;
                    }
                }

                if (conditionsMet)
                {
                    result = value.Result;

                    // Break from loop since it shouldn't be able to meet any more Value's conditions
                    break;
                }
            }

            // Get CaptureItems
            foreach (var captureItem in CaptureItems)
            {
                captureItem.PreviousValue = captureItem.Value;

                var instanceValue = instance.Values.ToList().Find(x => Tools.GetValue(x, "Id") == Tools.GetValue(captureItem, "Link"));
                if (instanceValue != null)
                {
                    if (instanceValue.Value != captureItem.Value)
                    {
                        captureItem.Value = instanceValue.Value;
                    }

                    captureItem.Sequence = instanceValue.ChangedSequence;
                }
                else captureItem.Value = "";

                result.CaptureItems = CaptureItems.ToList();
            }

            return result;
        }

        private static bool FindTrigger(Instance.DataItemValue value, Trigger trigger)
        {
            if (trigger.Link != null)
            {
                if (trigger.LinkType == TriggerLinkType.Type)
                {
                    return FormatDataItemType(value) == trigger.Link.ToLower();
                }
                else
                {
                    return value.Id == trigger.Link;
                }
            }

            return false;
        }

        private static string FormatDataItemType(Instance.DataItemValue value)
        {
            if (!string.IsNullOrEmpty(value.Type))
            {
                return value.Type.ToLower();
            }

            return null;
        }

    }

}
