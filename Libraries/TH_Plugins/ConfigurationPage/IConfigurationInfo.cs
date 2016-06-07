// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel.Composition;

namespace TH_Plugins.ConfigurationPage
{
    [InheritedExport(typeof(IConfigurationInfo))]
    public interface IConfigurationInfo
    {

        Type ConfigurationPageType { get; }

    }
}
