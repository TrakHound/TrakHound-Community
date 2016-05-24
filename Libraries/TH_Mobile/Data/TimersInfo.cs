// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TH_Mobile.Data
{
    public class TimersInfo
    {
        [NonSerialized]
        public bool Changed;

        public double Total { get; set; }
        public double Production { get; set; }
        public double Idle { get; set; }
        public double Alert { get; set; }

        public bool EqualTo(TimersInfo info)
        {
            return
                info != null &&
                Total == info.Total &&
                Production == info.Production &&
                Idle == info.Idle &&
                Alert == info.Alert;
        }
    }
}
