// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TrakHound_Device_Manager.Pages.GeneratedEvents.Controls
{
    /// <summary>
    /// Interaction logic for Default.xaml
    /// </summary>
    public partial class Default : UserControl
    {
        public Default()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Page.Result ParentResult;

        public string ValueName
        {
            get { return (string)GetValue(ValueNameProperty); }
            set { SetValue(ValueNameProperty, value); }
        }

        public static readonly DependencyProperty ValueNameProperty =
            DependencyProperty.Register("ValueName", typeof(string), typeof(Default), new PropertyMetadata(null));

        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentResult != null) ParentResult.value = txt.Text;

            if (txt.IsKeyboardFocused) if (SettingChanged != null) SettingChanged();
        }

        object oldFocus = null;

        private void TXT_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotFocus(object sender, RoutedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotMouseCapture(object sender, MouseEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_LostFocus(object sender, RoutedEventArgs e) { oldFocus = null; }

    }
}
