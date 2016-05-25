// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TH_Mobile.Data
{
    public class OeeInfo
    {
        [NonSerialized]
        public bool Changed;

        public double Oee { get; set; }
        public double Availability { get; set; }
        public double Performance { get; set; }
        public double Quality { get; set; }

        public bool EqualTo(OeeInfo info)
        {
            return
                info != null &&
                Oee == info.Oee &&
                Availability == info.Availability &&
                Performance == info.Performance &&
                Quality == info.Quality;
        }
    }
}
