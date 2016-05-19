// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Configuration;

namespace TH_Mobile.Data
{
    public class DescriptionInfo
    {
        public DescriptionInfo(Configuration config)
        {
            Description = config.Description.Description;
            DeviceId = config.Description.Device_ID;
            Manufacturer = config.Description.Manufacturer;
            Model = config.Description.Model;
            Serial = config.Description.Serial;
            Controller = config.Description.Controller;
            ImageUrl = config.FileLocations.Image_Path;
            LogoUrl = config.FileLocations.Manufacturer_Logo_Path;
        }

        [NonSerialized]
        public bool Changed;

        public string Description { get; set; }
        public string DeviceId { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Controller { get; set; }

        public string ImageUrl { get; set; }
        public string LogoUrl { get; set; }

        public bool EqualTo(DescriptionInfo info)
        {
            return 
                info != null &&
                Description == info.Description &&
                DeviceId == info.DeviceId &&
                Manufacturer == info.Manufacturer &&
                Model == info.Model &&
                Serial == info.Serial &&
                Controller == info.Controller &&
                ImageUrl == info.ImageUrl &&
                LogoUrl == info.LogoUrl;
        }
    }
}
