// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TH_Plugins
{
    public delegate void SendData_Handler(EventData data);

    public class EventData
    {
        public string id { get; set; }

        public object data01 { get; set; }
        public object data02 { get; set; }
        public object data03 { get; set; }
        public object data04 { get; set; }
        public object data05 { get; set; }
    }
}
