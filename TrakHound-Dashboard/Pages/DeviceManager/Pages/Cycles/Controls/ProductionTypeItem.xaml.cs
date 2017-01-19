// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.Cycles.Controls
{
    /// <summary>
    /// Interaction logic for ProductionTypeItem.xaml
    /// </summary>
    public partial class ProductionTypeItem : UserControl
    {
        public ProductionTypeItem()
        {
            InitializeComponent();
            bd.DataContext = this;
        }

        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(ProductionTypeItem), new PropertyMetadata(null));


        public string ValueName
        {
            get { return (string)GetValue(ValueNameProperty); }
            set { SetValue(ValueNameProperty, value); }
        }

        public static readonly DependencyProperty ValueNameProperty =
            DependencyProperty.Register("ValueName", typeof(string), typeof(ProductionTypeItem), new PropertyMetadata(null));


        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        
        private void productionType_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.IsKeyboardFocusWithin || combo.IsMouseCaptured)
            {
                SettingChanged?.Invoke();
            }
        }

        private void productionType_COMBO_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.IsKeyboardFocusWithin || combo.IsMouseCaptured)
            {
                SettingChanged?.Invoke();
            }
        }
    }
}
