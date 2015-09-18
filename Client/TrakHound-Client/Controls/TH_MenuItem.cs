// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Controls;

using TH_PlugIns_Client_Control;

namespace TrakHound_Client.Controls
{
    public class TH_MenuItem : MenuItem
    {

        public object Data { get; set; }

        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(Data);
        }

        public delegate void Clicked_Handler(object data);

        public event Clicked_Handler Clicked;
       
    }
}
