// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows.Media;

namespace TrakHound
{
    /// <summary>
    /// Interface used for any general page use that needs a name and image
    /// </summary>
    public interface IPage
    {
        string Title { get; }

        ImageSource Image { get; }

        void Opened();
        bool Opening();

        void Closed();
        bool Closing();
    }

}
