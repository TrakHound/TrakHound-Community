// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;

namespace TrakHound_Server.Plugins.OEE
{
    public class OEEData : TrakHound.OEE
    {
        public DateTime Timestamp { get; set; }

        public long Sequence { get; set; }
    }
}
