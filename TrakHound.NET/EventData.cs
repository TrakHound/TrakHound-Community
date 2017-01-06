// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TrakHound
{
    public delegate void SendData_Handler(EventData data);

    public class EventData
    {
        public EventData(object sender)
        {
            Sender = sender;
        }

        public object Sender { get; set; }

        public string Id { get; set; }

        public object Data01 { get; set; }
        public object Data02 { get; set; }
        public object Data03 { get; set; }
        public object Data04 { get; set; }
        public object Data05 { get; set; }
    }
}
