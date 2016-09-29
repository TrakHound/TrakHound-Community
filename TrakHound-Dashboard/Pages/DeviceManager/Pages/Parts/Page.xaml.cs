// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

using TrakHound;
using TrakHound.Plugins.Server;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.Parts
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IConfigurationPage
    {
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
                string eventPrefix = prefix + "/Event||" + i.ToString("00");

                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix, "attributes", "id||" + i.ToString("00") + ";");
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/EventName", "value", item.EventName);
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/EventValue", "value", item.EventValue);
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/CaptureItemLink", "value", item.CaptureItemLink);
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/CalculationType", "value", item.CalculationType);

                i++;
            }
        }

        const string prefix = "/Parts/";


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
                            case "CaptureItemLink": item.CaptureItemLink = DataTable_Functions.GetRowValue("value", eventRow); break;
                            case "CalculationType": item.CalculationType = DataTable_Functions.GetRowValue("value", eventRow); break;
                        }
                    }

                    PartCountItems.Add(item);
                }
            }
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
