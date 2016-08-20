// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace TrakHound_Server.Plugins.GeneratedEvents
{
    public enum TriggerModifier
    {
        None = 0,
        NOT = 1,
        GREATER_THAN = 2,
        LESS_THAN = 3,
        CONTAINS = 4,
        CONTAINS_MATCH_CASE = 5,
        CONTAINS_WHOLE_WORD = 6,
        CONTAINS_WHOLE_WORD_MATCH_CASE = 7
    };

    public enum TriggerLinkType
    {
        ID = 0,
        Type = 1,
    }

    public class Trigger
    {
        public TriggerLinkType LinkType { get; set; }
        public string Link { get; set; }
        public string Value { get; set; }
        public TriggerModifier Modifier { get; set; }

        public static Trigger Read(XmlNode Node)
        {
            Trigger result = null;

            if (Node.Attributes != null)
            {
                result = new Trigger();

                if (Node.Attributes["link_type"] != null)
                {
                    switch (Node.Attributes["link_type"].Value.ToString().ToLower())
                    {
                        case "type": result.LinkType = TriggerLinkType.Type; break;
                        default: result.LinkType = TriggerLinkType.ID; break;
                    }
                }

                if (Node.Attributes["link"] != null)
                    result.Link = Node.Attributes["link"].Value;

                if (Node.Attributes["value"] != null)
                    result.Value = Node.Attributes["value"].Value;

                // Added modifier to enable being able to say != to 'value' (ex. Message != "")
                if (Node.Attributes["modifier"] != null)
                {
                    switch (Node.Attributes["modifier"].Value.ToString().ToLower())
                    {
                        case "not": result.Modifier = TriggerModifier.NOT; break;
                        case "greater_than": result.Modifier = TriggerModifier.GREATER_THAN; break;
                        case "less_than": result.Modifier = TriggerModifier.LESS_THAN; break;
                        case "contains": result.Modifier = TriggerModifier.CONTAINS; break;
                        case "contains_match_case": result.Modifier = TriggerModifier.CONTAINS_MATCH_CASE; break;
                        case "contains_whole_word": result.Modifier = TriggerModifier.CONTAINS_WHOLE_WORD; break;
                        case "contains_whole_word_match_case": result.Modifier = TriggerModifier.CONTAINS_WHOLE_WORD_MATCH_CASE; break;
                    }
                }     
            }

            return result;
        }
    }
}
