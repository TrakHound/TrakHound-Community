// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.ComponentModel.Composition;
using System.Data;

namespace TrakHound.Plugins.Server
{
    [InheritedExport(typeof(IConfigurationPage))]
    public interface IConfigurationPage
    {
        string Title { get; }

        Uri Image { get; }

        event SettingChanged_Handler SettingChanged;

        bool Loaded { get; set; }

        event SendData_Handler SendData;

        void GetSentData(EventData data);


        void LoadConfiguration(DataTable dt);

        void SaveConfiguration(DataTable dt);

    }

    //public delegate void SaveRequest_Handler();
    public delegate void SettingChanged_Handler(string name, string oldVal, string newVal);

}
