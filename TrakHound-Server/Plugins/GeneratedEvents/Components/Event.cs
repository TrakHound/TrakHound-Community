// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TrakHound_Server.Plugins.Instances;

namespace TrakHound_Server.Plugins.GeneratedEvents
{
    public class Event
    {
        public Event()
        {
            Values = new List<Value>();
            CaptureItems = new List<CaptureItem>();
        }

        public string Name { get; set; }

        public List<Value> Values { get; set; }

        public Return Default { get; set; }

        public List<CaptureItem> CaptureItems { get; set; }


        public Return CurrentValue { get; set; }

        public Return PreviousValue { get; set; }


        public Return Process(InstanceData instanceData)
        {
            Return result;

            // Initially set to return the default value for the Event
            result = new Return();
            result.Value = Default.Value;
            result.NumVal = Default.NumVal;

            bool conditionsMet = false;

            if (instanceData != null)
            {
                foreach (Value Value in Values)
                {
                    conditionsMet = false;

                    foreach (var trigger in Value.Triggers)
                    {
                        InstanceData.DataItemValue instanceValue = instanceData.Values.Find(x => FindTrigger(x, trigger));
                        if (instanceValue != null)
                        {
                            if (trigger.Modifier == TriggerModifier.NOT)
                            {
                                if (Tools.GetValue(instanceValue, "Value") != Tools.GetValue(trigger, "Value")) conditionsMet = true;
                                else conditionsMet = false;
                            }
                            else if (trigger.Modifier == TriggerModifier.GREATER_THAN)
                            {
                                double trigger_val = double.MinValue;
                                double val = double.MinValue;
                                if (double.TryParse(instanceValue.Value, out val) && double.TryParse(trigger.Value, out trigger_val))
                                {
                                    if (val > trigger_val) conditionsMet = true;
                                    else conditionsMet = false;
                                }
                                else conditionsMet = false;
                            }
                            else if (trigger.Modifier == TriggerModifier.LESS_THAN)
                            {
                                double trigger_val = double.MinValue;
                                double val = double.MinValue;
                                if (double.TryParse(instanceValue.Value, out val) && double.TryParse(trigger.Value, out trigger_val))
                                {
                                    if (val < trigger_val) conditionsMet = true;
                                    else conditionsMet = false;
                                }
                                else conditionsMet = false;
                            }
                            else if (trigger.Modifier == TriggerModifier.CONTAINS)
                            {
                                if (Regex.IsMatch(instanceValue.Value, "^(?=.*" + trigger.Value + ").+$", RegexOptions.IgnoreCase)) conditionsMet = true;
                                else conditionsMet = false;
                            }
                            else if (trigger.Modifier == TriggerModifier.CONTAINS_MATCH_CASE)
                            {
                                if (Regex.IsMatch(instanceValue.Value, "^(?=.*" + trigger.Value + ").+$")) conditionsMet = true;
                                else conditionsMet = false;
                            }
                            else if (trigger.Modifier == TriggerModifier.CONTAINS_WHOLE_WORD)
                            {
                                if (Regex.IsMatch(instanceValue.Value, trigger.Value + "\\b", RegexOptions.IgnoreCase)) conditionsMet = true;
                                else conditionsMet = false;
                            }
                            else if (trigger.Modifier == TriggerModifier.CONTAINS_WHOLE_WORD_MATCH_CASE)
                            {
                                if (Regex.IsMatch(instanceValue.Value, trigger.Value + "\\b")) conditionsMet = true;
                                else conditionsMet = false;
                            }
                            else
                            {
                                if (Tools.GetValue(instanceValue, "Value") == Tools.GetValue(trigger, "Value")) conditionsMet = true;
                                else conditionsMet = false;
                            }
                        }

                        // if any triggers are not met then break
                        if (!conditionsMet) break;
                    }
                    
                    if (conditionsMet || Value.Triggers.Count == 0)
                    {
                        foreach (MultiTrigger multiTrigger in Value.MultiTriggers)
                        {
                            foreach (Trigger trigger in multiTrigger.Triggers)
                            {
                                InstanceData.DataItemValue instanceValue = instanceData.Values.Find(x => FindTrigger(x, trigger));
                                if (instanceValue != null)
                                {
                                    if (trigger.Modifier == TriggerModifier.NOT)
                                    {
                                        if (Tools.GetValue(instanceValue, "Value") != Tools.GetValue(trigger, "Value")) conditionsMet = true;
                                        else conditionsMet = false;
                                    }
                                    else if (trigger.Modifier == TriggerModifier.GREATER_THAN)
                                    {
                                        double trigger_val = double.MinValue;
                                        double val = double.MinValue;
                                        if (double.TryParse(instanceValue.Value, out val) && double.TryParse(trigger.Value, out trigger_val))
                                        {
                                            if (val > trigger_val) conditionsMet = true;
                                            else conditionsMet = false;
                                        }
                                        else conditionsMet = false;
                                    }
                                    else if (trigger.Modifier == TriggerModifier.LESS_THAN)
                                    {
                                        double trigger_val = double.MinValue;
                                        double val = double.MinValue;
                                        if (double.TryParse(instanceValue.Value, out val) && double.TryParse(trigger.Value, out trigger_val))
                                        {
                                            if (val < trigger_val) conditionsMet = true;
                                            else conditionsMet = false;
                                        }
                                        else conditionsMet = false;
                                    }
                                    else if (trigger.Modifier == TriggerModifier.CONTAINS)
                                    {
                                        if (Regex.IsMatch(instanceValue.Value, "^(?=.*" + trigger.Value + ").+$", RegexOptions.IgnoreCase)) conditionsMet = true;
                                        else conditionsMet = false;
                                    }
                                    else if (trigger.Modifier == TriggerModifier.CONTAINS_MATCH_CASE)
                                    {
                                        if (Regex.IsMatch(instanceValue.Value, "^(?=.*" + trigger.Value + ").+$")) conditionsMet = true;
                                        else conditionsMet = false;
                                    }
                                    else if (trigger.Modifier == TriggerModifier.CONTAINS_WHOLE_WORD)
                                    {
                                        if (Regex.IsMatch(instanceValue.Value, trigger.Value + "\\b", RegexOptions.IgnoreCase)) conditionsMet = true;
                                        else conditionsMet = false;
                                    }
                                    else if (trigger.Modifier == TriggerModifier.CONTAINS_WHOLE_WORD_MATCH_CASE)
                                    {
                                        if (Regex.IsMatch(instanceValue.Value, trigger.Value + "\\b")) conditionsMet = true;
                                        else conditionsMet = false;
                                    }
                                    else
                                    {
                                        if (Tools.GetValue(instanceValue, "Value") == Tools.GetValue(trigger, "Value")) conditionsMet = true;
                                        else conditionsMet = false;
                                    }
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
                        result = new Return();
                        result.Value = Value.Result.Value;
                        result.NumVal = Value.Result.NumVal;

                        // Break from loop since it shouldn't be able to meet any more Value's conditions
                        break;
                    }
                }

                // Get CaptureItems
                foreach (var captureItem in CaptureItems)
                {
                    captureItem.PreviousValue = captureItem.Value;

                    var instanceValue = instanceData.Values.ToList().Find(x => Tools.GetValue(x, "Id") == Tools.GetValue(captureItem, "Link"));
                    if (instanceValue != null)
                    {
                        if (instanceValue.Value != captureItem.Value)
                        {
                            captureItem.Value = instanceValue.Value;
                            captureItem.Sequence = instanceData.Sequence;
                        }
                    }
                    else captureItem.Value = "";

                    result.CaptureItems = CaptureItems.ToList();
                }

                result.TimeStamp = instanceData.Timestamp;
            }
            else
            {
                foreach (CaptureItem captureItem in CaptureItems)
                {
                    var match = result.CaptureItems.Find(x => x.Id == captureItem.Id);
                    if (match == null)
                    {
                        match = new CaptureItem();
                        match.Id = captureItem.Id;
                        match.Name = captureItem.Name;
                        match.Link = captureItem.Link;
                        result.CaptureItems.Add(match);
                    }

                    match.PreviousValue = match.Value;

                    match.Value = "UNAVAILABLE";
                }

                result.TimeStamp = DateTime.UtcNow;
            }

            return result;
        }
        
        private static bool FindTrigger(InstanceData.DataItemValue value, Trigger trigger)
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

        private static string FormatDataItemType(InstanceData.DataItemValue value)
        {
            if (!string.IsNullOrEmpty(value.Type))
            {
                return value.Type.ToLower();
            }

            return null;
        }
    }

}
