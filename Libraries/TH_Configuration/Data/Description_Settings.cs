// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace TH_Configuration
{

    public class Description_Settings
    {
        public string Machine_Type { get; set; }

        public string Description { get; set; }

        public string Customer_Name { get; set; }
        public string Control_Type { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Machine_ID { get; set; }

        public List<Tuple<string, string>> Custom;

    }

}
