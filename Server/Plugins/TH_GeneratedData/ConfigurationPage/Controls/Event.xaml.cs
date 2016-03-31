// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TH_GeneratedData.ConfigurationPage.Controls
{
    /// <summary>
    /// Interaction logic for Event.xaml
    /// </summary>
    public partial class Event : UserControl
    {
        public Event()
        {
            InitializeComponent();
            DataContext = this;
        }


        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(Event), new PropertyMetadata(null));
        

        public Page.Event ParentEvent;

        ObservableCollection<Value> values;
        public ObservableCollection<Value> Values
        {
            get
            {
                if (values == null)
                    values = new ObservableCollection<Value>();
                return values;
            }

            set
            {
                values = value;
            }
        }


        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(Event), new PropertyMetadata(null));

        public delegate void Clicked_Handler(Event e);
        public event Clicked_Handler AddValueClicked;

        private void AddValue_Clicked(TH_WPF.Button bt)
        {
            if (AddValueClicked != null) AddValueClicked(this);
        }

        private void Description_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentEvent != null) ParentEvent.description = txt.Text;

            if (txt.IsKeyboardFocused) if (SettingChanged != null) SettingChanged();
        }

        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        public object DefaultValue
        {
            get { return (object)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(object), typeof(Event), new PropertyMetadata(null));

        object oldFocus = null;

        private void TXT_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotFocus(object sender, RoutedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotMouseCapture(object sender, MouseEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_LostFocus(object sender, RoutedEventArgs e) { oldFocus = null; }




        #region "Capture Items"

        ObservableCollection<CaptureItem> captureitems;
        public ObservableCollection<CaptureItem> CaptureItems
        {
            get
            {
                if (captureitems == null)
                    captureitems = new ObservableCollection<CaptureItem>();
                return captureitems;
            }

            set
            {
                captureitems = value;
            }
        }

        #endregion

        public event Clicked_Handler AddCaptureItemClicked;

        private void AddCaptureItem_Clicked(TH_WPF.Button bt)
        {
            if (AddCaptureItemClicked != null) AddCaptureItemClicked(this);
        }

    }
}
