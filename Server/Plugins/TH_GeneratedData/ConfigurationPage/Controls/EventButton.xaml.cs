// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TH_GeneratedData.ConfigurationPage.Controls
{
    /// <summary>
    /// Interaction logic for EventButton.xaml
    /// </summary>
    public partial class EventButton : UserControl
    {
        public EventButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Page.Event ParentEvent;

        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        #region "Properties"

        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set
            {
                SetValue(EventNameProperty, value);
            }
        }

        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(EventButton), new PropertyMetadata(null));


        public string EventValues
        {
            get { return (string)GetValue(EventValuesProperty); }
            set { SetValue(EventValuesProperty, value); }
        }

        public static readonly DependencyProperty EventValuesProperty =
            DependencyProperty.Register("EventValues", typeof(string), typeof(EventButton), new PropertyMetadata(null));

        #endregion

        #region "Event Name"

        private void EventName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentEvent != null) ParentEvent.name = txt.Text;

            if (txt.IsKeyboardFocused) if (SettingChanged != null) SettingChanged();
        }

        private void TXT_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { ((TextBox)sender).SelectAll(); }

        private void TXT_GotFocus(object sender, RoutedEventArgs e) { ((TextBox)sender).SelectAll(); }

        private void TXT_GotMouseCapture(object sender, MouseEventArgs e) { ((TextBox)sender).SelectAll(); }

        #endregion

        #region "Remove"

        public delegate void Clicked_Handler(EventButton bt);
        public event Clicked_Handler RemoveClicked;

        private void Remove_Clicked(TH_WPF.Button bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
        }

        #endregion

    }
}
