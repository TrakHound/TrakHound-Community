// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.ComponentModel.Composition;
using System.Data;
using System.Windows.Media;

namespace TH_Plugins.Database
{
    public enum Application_Type
    {
        Client = 0,
        Server = 1
    }

    public delegate void SettingChanged_Handler(string name, string oldVal, string newVal);


    [InheritedExport(typeof(IConfigurationPage))]
    public interface IConfigurationPage
    {
        string PageName { get; }

        ImageSource Image { get; }

        event SettingChanged_Handler SettingChanged;

        string prefix { get; set; }

        void LoadConfiguration(DataTable dt);

        void SaveConfiguration(DataTable dt);

        Application_Type ApplicationType { get; set; }

        IDatabasePlugin Plugin { get; }
    }
}
