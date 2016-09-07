// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TrakHound.Configurations
{

    public class Description_Settings
    {
        /// <summary>
        /// General Device Description (ex. 3 Axis VMC)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of Device (ex. Lathe, Machining Center, Grinder, etc)
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Custom Device Identifier (ex. VMC-02, A2, etc.)
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Name of the Manufacturer of the Device (ex. Okuma, Mazak, etc.)
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Model name of the Device (ex. MULTUS 3000, LT2000, etc.)
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Serial Number of the Device
        /// </summary>
        public string Serial { get; set; }

        /// <summary>
        /// Type of controller the Device has (ex. Fanuc, OSP, Mazatrol, Fagor, etc.)
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Name of the building or location of the Device
        /// </summary>
        public string Location { get; set; }
        
    }

}
