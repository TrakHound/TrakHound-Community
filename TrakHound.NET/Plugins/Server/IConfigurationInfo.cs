// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.ComponentModel.Composition;

namespace TrakHound.Plugins.Server
{
    [InheritedExport(typeof(IConfigurationInfo))]
    public interface IConfigurationInfo
    {

        string Title { get; }

        Uri Image { get; }

        Type ConfigurationPageType { get; }

    }
}
