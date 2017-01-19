// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins.Server;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.Parts
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IConfigurationPage
    {
        const string prefix = "/Parts/";


        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Title { get { return "Parts"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Block_01.png"); } }

        public bool Loaded { get; set; }

        public event SettingChanged_Handler SettingChanged;


        public event SendData_Handler SendData;

        public bool ZoomEnabled { get { return false; } }

        public void SetZoom(double zoomPercentage) { }

        public void GetSentData(EventData data) { }


        public void LoadConfiguration(DataTable dt)
        {
            LoadGeneratedEventItems(dt);
            LoadPartCountEventItems(dt);
        }

        public void SaveConfiguration(DataTable dt)
        {
            DataTable_Functions.TrakHound.DeleteRows(prefix + "*", "address", dt);

            int i = 0;

            foreach (var item in PartCountItems)
            {
                if (!string.IsNullOrEmpty(item.EventName) && !string.IsNullOrEmpty(item.EventValue))
                {
                    string eventPrefix = prefix + "Event||" + i.ToString("00");

                    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix, "attributes", "id||" + i.ToString("00") + ";");
                    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/EventName", "value", item.EventName);
                    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/EventValue", "value", item.EventValue);
                    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/PreviousEventValue", "value", item.PreviousEventValue);
                    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/ValueType", "value", FormatValue(item.ValueType));
                    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/CaptureItemLink", "value", item.CaptureItemLink);
                    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/CalculationType", "value", item.CalculationType);
                    DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/StaticIncrementValue", "value", item.StaticIncrementValue.ToString());

                    i++;
                }
            }
        }

        private ObservableCollection<GeneratedEventItem> _generatedEventItems;
        public ObservableCollection<GeneratedEventItem> GeneratedEventItems
        {
            get
            {
                if (_generatedEventItems == null)
                    _generatedEventItems = new ObservableCollection<GeneratedEventItem>();
                return _generatedEventItems;
            }

            set
            {
                _generatedEventItems = value;
            }
        }

        public class GeneratedEventItem
        {
            public GeneratedEventItem(GeneratedEvents.Page.Event e)
            {
                Id = e.name;
                Name = String_Functions.UppercaseFirst(e.name.Replace('_', ' ').ToLower());
                Event = e;
            }

            public string Id { get; set; }
            public string Name { get; set; }

            public GeneratedEvents.Page.Event Event { get; set; }
        }

        private void LoadGeneratedEventItems(DataTable dt)
        {
            GeneratedEventItems.Clear();

            var events = GeneratedEvents.Page.GetGeneratedEvents(dt);
            foreach (var e in events)
            {
                GeneratedEventItems.Add(new GeneratedEventItem(e));
            }
        }
        

        private ObservableCollection<Controls.PartCountEventItem> _partCountItems;
        public ObservableCollection<Controls.PartCountEventItem> PartCountItems
        {
            get
            {
                if (_partCountItems == null)
                    _partCountItems = new ObservableCollection<Controls.PartCountEventItem>();
                return _partCountItems;
            }

            set
            {
                _partCountItems = value;
            }
        }

        private void LoadPartCountEventItems(DataTable dt)
        {
            PartCountItems.Clear();

            string address = "/Parts/";
            string filter = "address LIKE '" + address + "*'";

            var rows = DataTable_Functions.GetRows(dt, filter);

            foreach (DataRow row in rows)
            {
                string lastNode = DataTable_Functions.TrakHound.GetLastNode(row);
                if (lastNode == "Event")
                {
                    var item = new Controls.PartCountEventItem();
                    item.ParentPage = this;
                    item.RemoveClicked += Item_RemoveClicked;
                    item.SettingChanged += Item_SettingChanged;

                    filter = "address LIKE '" + row["address"].ToString() + "/*'";
                    var eventRows = DataTable_Functions.GetRows(dt, filter);
                    foreach (DataRow eventRow in eventRows)
                    {
                        lastNode = DataTable_Functions.TrakHound.GetLastNode(eventRow);

                        switch (lastNode)
                        {
                            case "EventName": item.EventName = DataTable_Functions.GetRowValue("value", eventRow); break;
                            case "EventValue": item.EventValue = DataTable_Functions.GetRowValue("value", eventRow); break;
                            case "PreviousEventValue": item.PreviousEventValue = DataTable_Functions.GetRowValue("value", eventRow); break;
                            case "ValueType": item.ValueType = GetFormattedValue(DataTable_Functions.GetRowValue("value", eventRow)); break;
                            case "CaptureItemLink": item.CaptureItemLink = DataTable_Functions.GetRowValue("value", eventRow); break;
                            case "CalculationType": item.CalculationType = DataTable_Functions.GetRowValue("value", eventRow); break;
                            case "StaticIncrementValue": item.StaticIncrementValue = DataTable_Functions.GetIntegerFromRow("value", eventRow); break;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.EventName)) PartCountItems.Add(item);
                }
            }

            // Try loading deprecated configuration
            if (PartCountItems.Count == 0 && rows.Length > 0)
            {
                var item = new Controls.PartCountEventItem();
                item.ParentPage = this;
                item.RemoveClicked += Item_RemoveClicked;
                item.SettingChanged += Item_SettingChanged;

                foreach (DataRow row in rows)
                {
                    string lastNode = DataTable_Functions.TrakHound.GetLastNode(row);

                    if (lastNode == "PartsEventValue") item.EventValue = GetFormattedValue(DataTable_Functions.GetRowValue("value", row));
                    if (lastNode == "PartsEventName") item.EventName = DataTable_Functions.GetRowValue("value", row);
                    else if (lastNode == "PartsCaptureItemLink") item.CaptureItemLink = DataTable_Functions.GetRowValue("value", row);
                    else if (lastNode == "CalculationType") item.CalculationType = DataTable_Functions.GetRowValue("value", row);   
                }

                if (!string.IsNullOrEmpty(item.EventName)) PartCountItems.Add(item);
            }
        }

        private string GetFormattedValue(string s)
        {
            if (!string.IsNullOrEmpty(s)) return String_Functions.UppercaseFirst(s.Replace('_', ' '));
            else return null;
        }

        private string FormatValue(string s)
        {
            if (!string.IsNullOrEmpty(s)) return s.Replace(' ', '_').ToLower();
            else return null;
        }

        private void Add_Clicked(TrakHound_UI.Button bt)
        {
            var item = new Controls.PartCountEventItem();
            item.ParentPage = this;
            item.RemoveClicked += Item_RemoveClicked;
            item.SettingChanged += Item_SettingChanged;
            PartCountItems.Add(item);
        }

        private void Item_SettingChanged()
        {
            SettingChanged?.Invoke(null, null, null);
        }

        private void Item_RemoveClicked(Controls.PartCountEventItem item)
        {
            int i = PartCountItems.ToList().FindIndex(o => o.Id == item.Id);
            if (i >= 0)
            {
                PartCountItems.RemoveAt(i);
                SettingChanged?.Invoke(null, null, null);
            }
        }

    }
}
