// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;

namespace TrakHound_Server.Plugins.GeneratedEvents
{
    public class Return
    {
        public Return()
        {
            CaptureItems = new List<CaptureItem>();
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public int NumVal { get; set; }
        public string Value { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime ChangedTimeStamp { get; set; }
        public long Sequence { get; set; }
        public long ChangedSequence { get; set; }
        public double Duration { get; set; }

        public List<CaptureItem> CaptureItems { get; set; }

        public static Return Read(XmlNode node)
        {
            var result = new Return();

            if (node.Attributes != null)
                if (node.Attributes["numval"] != null)
                    result.NumVal = Convert.ToInt16(node.Attributes["numval"].Value);

            result.Value = node.InnerText;

            return result;
        }

        public Return Copy()
        {
            var result = new Return();
            result.NumVal = NumVal;
            result.Value = Value;
            result.TimeStamp = TimeStamp;
            result.ChangedTimeStamp = ChangedTimeStamp;
            result.Sequence = Sequence;
            result.ChangedSequence = ChangedSequence;
            result.Duration = Duration;
            result.CaptureItems.AddRange(CaptureItems);

            return result;
        }


        public new string ToString()
        {
            string format = "{0} : {1} @ {2} : {3} : {4}";
            return string.Format(format, Id, Value, TimeStamp.ToString("o"), Sequence, ChangedSequence);
        }

        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {

            var other = obj as Return;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        private static bool EqualTo(Return r1, Return r2)
        {
            if (!object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return false;
            if (object.ReferenceEquals(r1, null) && !object.ReferenceEquals(r2, null)) return false;
            if (object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return true;

            return r1.Value == r2.Value;
        }

        private static bool NotEqualTo(Return r1, Return r2)
        {
            if (!object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return true;
            if (object.ReferenceEquals(r1, null) && !object.ReferenceEquals(r2, null)) return true;
            if (object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return false;

            return r1.Value != r2.Value;
        }

        public static bool operator ==(Return r1, Return r2)
        {
            return EqualTo(r1, r2);
        }

        public static bool operator !=(Return r1, Return r2)
        {
            return NotEqualTo(r1, r2);
        }
    }
}
