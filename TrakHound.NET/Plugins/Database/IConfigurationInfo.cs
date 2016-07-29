// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel.Composition;
using System.Data;

namespace TrakHound.Plugins.Database
{
    [InheritedExport(typeof(IConfigurationInfo))]
    public interface IConfigurationInfo
    {

        string Type { get; }

        Type ConfigurationPageType { get; }

        object CreateConfigurationButton(DataTable dt);

    }
}
