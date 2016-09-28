// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.GeneratedEvents.Controls
{
    /// <summary>
    /// Interaction logic for CaptureItem.xaml
    /// </summary>
    public partial class CaptureItem : UserControl
    {
        public CaptureItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Page.Event ParentEvent;
        public Page.CaptureItem ParentCaptureItem;


        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(CaptureItem), new PropertyMetadata(null));

        


        public object CaptureName
        {
            get { return (object)GetValue(CaptureNameProperty); }
            set { SetValue(CaptureNameProperty, value); }
        }

        public static readonly DependencyProperty CaptureNameProperty =
            DependencyProperty.Register("CaptureName", typeof(object), typeof(CaptureItem), new PropertyMetadata(null));


        public object SelectedLink
        {
            get { return (object)GetValue(SelectedLinkProperty); }
            set { SetValue(SelectedLinkProperty, value); }
        }

        public static readonly DependencyProperty SelectedLinkProperty =
            DependencyProperty.Register("SelectedLink", typeof(object), typeof(CaptureItem), new PropertyMetadata(null));


        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        private void name_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentCaptureItem != null) ParentCaptureItem.name = txt.Text;

            if (txt.IsKeyboardFocused) SettingChanged?.Invoke();
        }

        private void link_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.SelectedItem != null)
            {
                if (ParentCaptureItem != null) ParentCaptureItem.link = combo.SelectedItem.ToString();
            }

            if (combo.IsKeyboardFocused || combo.IsMouseCaptured) SettingChanged?.Invoke();
            
        }

        private void link_COMBO_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.SelectedItem != null)
            {
                if (ParentCaptureItem != null) ParentCaptureItem.link = combo.SelectedItem.ToString();
            }

            if (combo.IsKeyboardFocused || combo.IsMouseCaptured) SettingChanged?.Invoke();
        }

        public delegate void RemoveClicked_Handler(CaptureItem ci);
        public event RemoveClicked_Handler RemoveClicked;

        private void Remove_Clicked(TrakHound_UI.Button bt)
        {
            RemoveClicked?.Invoke(this);
        }
    }
}
