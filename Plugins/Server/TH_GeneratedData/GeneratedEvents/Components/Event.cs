// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TH_InstanceData;

namespace TH_GeneratedData.GeneratedEvents
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
            Return ReturnValue;

            ReturnValue = Default;

            bool conditionsMet = false;

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

                if (conditionsMet)
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
                    ReturnValue = Value.Result;

                    // Break from loop since it shouldn't be able to meet any more Value's conditions
                    break;
                }
            }

            // Get CaptureItems
            foreach (CaptureItem item in CaptureItems)
            {
                var i = ReturnValue.CaptureItems.Find(x => x.id == item.id);
                if (i == null)
                {
                    i = new CaptureItem();
                    i.id = item.id;
                    i.name = item.name;
                    i.link = item.link;
                    ReturnValue.CaptureItems.Add(i);
                }

                i.previous_value = i.value;

                InstanceData.DataItemValue instanceValue = instanceData.Values.ToList().Find(x => Tools.GetValue(x, "Id") == Tools.GetValue(item, "link"));
                if (instanceValue != null) i.value = instanceValue.Value;
                else i.value = "";
            }

            ReturnValue.TimeStamp = instanceData.Timestamp;

            return ReturnValue;
        }

        private static bool FindTrigger(InstanceData.DataItemValue value, Trigger trigger)
        {
            if (trigger.LinkType == TriggerLinkType.Type)
            {
                return FormatDataItemType(value) == trigger.Link;
            }
            else
            {
                return value.Id == trigger.Link;
            }
        }

        private static string FormatDataItemType(InstanceData.DataItemValue value)
        {
            if (!string.IsNullOrEmpty(value.Type) && !string.IsNullOrEmpty(value.SubType))
            {
                return value.Type + "||" + value.SubType;
            }
            else return value.Type;
        }
    }

}
