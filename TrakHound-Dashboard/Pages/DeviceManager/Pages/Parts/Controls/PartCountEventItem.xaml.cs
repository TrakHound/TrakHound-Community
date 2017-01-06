// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.Parts.Controls
{
    /// <summary>
    /// Interaction logic for ProductionTypeItem.xaml
    /// </summary>
    public partial class PartCountEventItem : UserControl
    {
        public PartCountEventItem()
        {
            InitializeComponent();
            root.DataContext = this;

            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        public delegate void RemoveClicked_Handler(PartCountEventItem item);
        public event RemoveClicked_Handler RemoveClicked;

        #region "Dependency Properties"

        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(PartCountEventItem), new PropertyMetadata(null));


        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(PartCountEventItem), new PropertyMetadata(null));


        public string EventValue
        {
            get { return (string)GetValue(EventValueProperty); }
            set { SetValue(EventValueProperty, value); }
        }

        public static readonly DependencyProperty EventValueProperty =
            DependencyProperty.Register("EventValue", typeof(string), typeof(PartCountEventItem), new PropertyMetadata(null));


        public string PreviousEventValue
        {
            get { return (string)GetValue(PreviousEventValueProperty); }
            set { SetValue(PreviousEventValueProperty, value); }
        }

        public static readonly DependencyProperty PreviousEventValueProperty =
            DependencyProperty.Register("PreviousEventValue", typeof(string), typeof(PartCountEventItem), new PropertyMetadata(null));


        public string ValueType
        {
            get { return (string)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.Register("ValueType", typeof(string), typeof(PartCountEventItem), new PropertyMetadata("Capture Item"));


        public string CaptureItemLink
        {
            get { return (string)GetValue(CaptureItemLinkProperty); }
            set { SetValue(CaptureItemLinkProperty, value); }
        }

        public static readonly DependencyProperty CaptureItemLinkProperty =
            DependencyProperty.Register("CaptureItemLink", typeof(string), typeof(PartCountEventItem), new PropertyMetadata(null));


        public string CalculationType
        {
            get { return (string)GetValue(CalculationTypeProperty); }
            set { SetValue(CalculationTypeProperty, value); }
        }

        public static readonly DependencyProperty CalculationTypeProperty =
            DependencyProperty.Register("CalculationType", typeof(string), typeof(PartCountEventItem), new PropertyMetadata("Total"));


        public int StaticIncrementValue
        {
            get { return (int)GetValue(StaticIncrementValueProperty); }
            set { SetValue(StaticIncrementValueProperty, value); }
        }

        public static readonly DependencyProperty StaticIncrementValueProperty =
            DependencyProperty.Register("StaticIncrementValue", typeof(int), typeof(PartCountEventItem), new PropertyMetadata(1));

        #endregion


        private void COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.IsKeyboardFocusWithin || combo.IsMouseCaptured)
            {
                SettingChanged?.Invoke();
            }
        }

        private void EventName_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            string val = (string)combo.SelectedValue;

            GetGeneratedEventValues(val);
            GetCaptureItems(val);

            if (combo.IsKeyboardFocusWithin || combo.IsMouseCaptured)
            {
                SettingChanged?.Invoke();
            }
        }

        private void EventValue_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            string val = (string)combo.SelectedValue;

            GetPreviousGeneratedEventValues(val);

            if (combo.IsKeyboardFocusWithin || combo.IsMouseCaptured)
            {
                SettingChanged?.Invoke();
            }
        }

        private void Remove_Clicked(TrakHound_UI.Button bt)
        {
            RemoveClicked?.Invoke(this);
        }

        #region "Generated Event Values"

        private ObservableCollection<object> _generatedEventValues;
        public ObservableCollection<object> GeneratedEventValues
        {
            get
            {
                if (_generatedEventValues == null)
                    _generatedEventValues = new ObservableCollection<object>();
                return _generatedEventValues;
            }

            set
            {
                _generatedEventValues = value;
            }
        }

        private ObservableCollection<object> _previousGeneratedEventValues;
        public ObservableCollection<object> PreviousGeneratedEventValues
        {
            get
            {
                if (_previousGeneratedEventValues == null)
                    _previousGeneratedEventValues = new ObservableCollection<object>();
                return _previousGeneratedEventValues;
            }

            set
            {
                _previousGeneratedEventValues = value;
            }
        }

        private void GetGeneratedEventValues(string Id)
        {
            GeneratedEventValues.Clear();

            if (ParentPage != null && ParentPage.GeneratedEventItems != null)
            {
                var e = ParentPage.GeneratedEventItems.ToList().Find(x => x.Id == Id);
                if (e != null)
                {
                    // Add each Value
                    foreach (var value in e.Event.values)
                    {
                        if (value.result != null) GeneratedEventValues.Add(value.result.value);
                    }

                    // Add Default Value
                    if (e.Event.Default != null)
                    {
                        if (e.Event.Default != null) GeneratedEventValues.Add(e.Event.Default.value);
                    }
                }
            }
        }

        private void GetPreviousGeneratedEventValues(string Id)
        {
            PreviousGeneratedEventValues.Clear();

            if (GeneratedEventValues != null)
            {
                PreviousGeneratedEventValues.Add(string.Empty);

                foreach (var gEventValue in GeneratedEventValues)
                {
                    if ((string)gEventValue != EventValue) PreviousGeneratedEventValues.Add(gEventValue);
                }
            }
        }

        #endregion

        #region "Capture Items"

        public class CaptureItem
        {
            public CaptureItem(GeneratedEvents.Page.CaptureItem item)
            {
                Id = item.name;
                Name = String_Functions.UppercaseFirst(item.name.Replace('_', ' '));
            }

            public string Id { get; set; }
            public string Name { get; set; }
        }

        private ObservableCollection<CaptureItem> _generatedEventCaptureItems;
        public ObservableCollection<CaptureItem> GeneratedEventCaptureItems
        {
            get
            {
                if (_generatedEventCaptureItems == null)
                    _generatedEventCaptureItems = new ObservableCollection<CaptureItem>();
                return _generatedEventCaptureItems;
            }

            set
            {
                _generatedEventCaptureItems = value;
            }
        }

        private void GetCaptureItems(string Id)
        {
            GeneratedEventCaptureItems.Clear();

            if (ParentPage != null && ParentPage.GeneratedEventItems != null)
            {
                var e = ParentPage.GeneratedEventItems.ToList().Find(x => x.Id == Id);
                if (e != null)
                {
                    // Add each Value
                    foreach (var item in e.Event.captureItems)
                    {
                        var i = new CaptureItem(item);
                        GeneratedEventCaptureItems.Add(i);
                    }
                }
            }
        }

        #endregion

    }
}
