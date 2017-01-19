// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.GeneratedEvents.Controls
{
    /// <summary>
    /// Interaction logic for Default.xaml
    /// </summary>
    public partial class Default : UserControl
    {
        public Default()
        {
            InitializeComponent();
            root.DataContext = this;
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

            if (txt.IsKeyboardFocused) SettingChanged?.Invoke();
        }
    }
}
