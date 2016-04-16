// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TH_InstanceTable;

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

            bool ConditionsMet = false;

            foreach (Value Value in Values)
            {
                ConditionsMet = false;

                foreach (Trigger Trigger in Value.Triggers)
                {
                    InstanceData.DataItemValue instanceValue = instanceData.Values.Find(x => Tools.GetValue(x, "Id") == Tools.GetValue(Trigger, "Link"));
                    if (instanceValue != null)
                    {
                        if (Trigger.Modifier == TriggerModifier.NOT)
                        {
                            if (Tools.GetValue(instanceValue, "Value") != Tools.GetValue(Trigger, "Value")) ConditionsMet = true;
                            else ConditionsMet = false;
                        }
                        else if (Trigger.Modifier == TriggerModifier.GREATER_THAN)
                        {
                            double trigger_val = double.MinValue;
                            double val = double.MinValue;
                            if (double.TryParse(instanceValue.Value, out val) && double.TryParse(Trigger.Value, out trigger_val))
                            {
                                if (val > trigger_val) ConditionsMet = true;
                                else ConditionsMet = false;
                            }
                            else ConditionsMet = false;
                        }
                        else if (Trigger.Modifier == TriggerModifier.LESS_THAN)
                        {
                            double trigger_val = double.MinValue;
                            double val = double.MinValue;
                            if (double.TryParse(instanceValue.Value, out val) && double.TryParse(Trigger.Value, out trigger_val))
                            {
                                if (val < trigger_val) ConditionsMet = true;
                                else ConditionsMet = false;
                            }
                            else ConditionsMet = false;
                        }
                        else if (Trigger.Modifier == TriggerModifier.CONTAINS)
                        {
                            if (Regex.IsMatch(instanceValue.Value, "^(?=.*" + Trigger.Value + ").+$", RegexOptions.IgnoreCase)) ConditionsMet = true;
                            else ConditionsMet = false;
                        }
                        else if (Trigger.Modifier == TriggerModifier.CONTAINS_MATCH_CASE)
                        {
                            if (Regex.IsMatch(instanceValue.Value, "^(?=.*" + Trigger.Value + ").+$")) ConditionsMet = true;
                            else ConditionsMet = false;
                        }
                        else if (Trigger.Modifier == TriggerModifier.CONTAINS_WHOLE_WORD)
                        {
                            if (Regex.IsMatch(instanceValue.Value, Trigger.Value + "\\b", RegexOptions.IgnoreCase)) ConditionsMet = true;
                            else ConditionsMet = false;
                        }
                        else if (Trigger.Modifier == TriggerModifier.CONTAINS_WHOLE_WORD_MATCH_CASE)
                        {
                            if (Regex.IsMatch(instanceValue.Value, Trigger.Value + "\\b")) ConditionsMet = true;
                            else ConditionsMet = false;
                        }
                        else
                        {
                            if (Tools.GetValue(instanceValue, "Value") == Tools.GetValue(Trigger, "Value")) ConditionsMet = true;
                            else ConditionsMet = false;
                        }
                    }

                    // if any triggers are not met then break
                    if (!ConditionsMet) break;
                }

                if (ConditionsMet)
                {
                    foreach (MultiTrigger MultiTrigger in Value.MultiTriggers)
                    {
                        foreach (Trigger MultiTrigger_Trigger in MultiTrigger.Triggers)
                        {
                            InstanceData.DataItemValue instanceValue = instanceData.Values.Find(x => x.Id.ToLower() == MultiTrigger_Trigger.Link.ToLower());
                            if (instanceValue != null)
                            {
                                if (MultiTrigger_Trigger.Modifier == TriggerModifier.NOT)
                                {
                                    if (Tools.GetValue(instanceValue, "Value") != Tools.GetValue(MultiTrigger_Trigger, "Value")) ConditionsMet = true;
                                    else ConditionsMet = false;
                                }
                                else if (MultiTrigger_Trigger.Modifier == TriggerModifier.GREATER_THAN)
                                {
                                    double trigger_val = double.MinValue;
                                    double val = double.MinValue;
                                    if (double.TryParse(instanceValue.Value, out val) && double.TryParse(MultiTrigger_Trigger.Value, out trigger_val))
                                    {
                                        if (val > trigger_val) ConditionsMet = true;
                                        else ConditionsMet = false;
                                    }
                                    else ConditionsMet = false;
                                }
                                else if (MultiTrigger_Trigger.Modifier == TriggerModifier.LESS_THAN)
                                {
                                    double trigger_val = double.MinValue;
                                    double val = double.MinValue;
                                    if (double.TryParse(instanceValue.Value, out val) && double.TryParse(MultiTrigger_Trigger.Value, out trigger_val))
                                    {
                                        if (val < trigger_val) ConditionsMet = true;
                                        else ConditionsMet = false;
                                    }
                                    else ConditionsMet = false;
                                }
                                else if (MultiTrigger_Trigger.Modifier == TriggerModifier.CONTAINS)
                                {
                                    if (Regex.IsMatch(instanceValue.Value, "^(?=.*" + MultiTrigger_Trigger.Value + ").+$", RegexOptions.IgnoreCase)) ConditionsMet = true;
                                    else ConditionsMet = false;
                                }
                                else if (MultiTrigger_Trigger.Modifier == TriggerModifier.CONTAINS_MATCH_CASE)
                                {
                                    if (Regex.IsMatch(instanceValue.Value, "^(?=.*" + MultiTrigger_Trigger.Value + ").+$")) ConditionsMet = true;
                                    else ConditionsMet = false;
                                }
                                else if (MultiTrigger_Trigger.Modifier == TriggerModifier.CONTAINS_WHOLE_WORD)
                                {
                                    if (Regex.IsMatch(instanceValue.Value, MultiTrigger_Trigger.Value + "\\b", RegexOptions.IgnoreCase)) ConditionsMet = true;
                                    else ConditionsMet = false;
                                }
                                else if (MultiTrigger_Trigger.Modifier == TriggerModifier.CONTAINS_WHOLE_WORD_MATCH_CASE)
                                {
                                    if (Regex.IsMatch(instanceValue.Value, MultiTrigger_Trigger.Value + "\\b")) ConditionsMet = true;
                                    else ConditionsMet = false;
                                }
                                else
                                {
                                    if (Tools.GetValue(instanceValue, "Value") == Tools.GetValue(MultiTrigger_Trigger, "Value")) ConditionsMet = true;
                                    else ConditionsMet = false;
                                }
                            }

                            // if any trigger is met then break (only one has to be met)
                            if (ConditionsMet) break;
                        }

                        // if none of the triggers were met then break (at least one has to be met)
                        if (!ConditionsMet) break;
                    }
                }

                if (ConditionsMet)
                {
                    ReturnValue = Value.Result;

                    // Break from loop since it shouldn't be able to meet any more Value's conditions
                    break;
                }
            }

            // Get CaptureItems
            //ReturnValue.CaptureItems.Clear();
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

                InstanceData.DataItemValue instanceValue = instanceData.Values.ToList().Find(x => Tools.GetValue(x, "Id") == Tools.GetValue(item, "Link"));
                if (instanceValue != null) i.value = instanceValue.Value;
                else i.value = "";
            }

            ReturnValue.TimeStamp = instanceData.Timestamp;

            return ReturnValue;
        }
    }

}
