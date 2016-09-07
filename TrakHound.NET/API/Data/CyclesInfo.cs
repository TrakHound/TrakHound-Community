// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TrakHound.API
{
    public partial class Data
    {
        public class CyclesInfo
        {
            public CyclesInfo()
            {
                Cycles = new List<CycleInfo>();
            }

            

            [JsonProperty("cycles")]
            public List<CycleInfo> Cycles { get; set; }

        }
    }   
}
