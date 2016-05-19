// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TH_Mobile.Data
{
    public class StatusInfo
    {
        [NonSerialized]
        public bool Changed;

        public int Connected { get; set; }
        public int Status { get; set; }
        public string ProductionStatus { get; set; }
        public int ProductionStatusTimer { get; set; }

        public bool EqualTo(StatusInfo info)
        {
            return
                info != null &&
                Connected == info.Connected &&
                Status == info.Status &&
                ProductionStatus == info.ProductionStatus &&
                ProductionStatusTimer == info.ProductionStatusTimer;
        }
    }
}
