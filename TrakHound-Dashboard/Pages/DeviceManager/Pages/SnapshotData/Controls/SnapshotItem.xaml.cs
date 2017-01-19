// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using TrakHound.Logging;
using TrakHound.Tools;
using TrakHound_Server.Plugins.SnapshotData;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.SnapshotData.Controls
{
    /// <summary>
    /// Interaction logic for SnapshotItem.xaml
    /// </summary>
    public partial class SnapshotItem : UserControl
    {
        public SnapshotItem(Page parentPage)
        {
            Init();

            ParentPage = parentPage;
            ParentSnapshot = new Snapshot();
        }

        public SnapshotItem(Page parentPage, Snapshot snapshot)
        {
            Init();

            ParentPage = parentPage;
            ParentSnapshot = snapshot;

            NameText = snapshot.Name;
            SelectedType = snapshot.Type.ToString();
            SelectedLink = snapshot.Link;
        }

        private void Init()
        {
            InitializeComponent();
            root.DataContext = this;

            Id = String_Functions.RandomString(20);
        }

        public string Id { get; set; }

        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(SnapshotItem), new PropertyMetadata(null));

        public Snapshot ParentSnapshot;

        public bool Loading { get; set; }


        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        public delegate void Clicked_Handler(SnapshotItem item);


        #region "Name"

        public string NameText
        {
            get { return (string)GetValue(NameTextProperty); }
            set
            {
                SetValue(NameTextProperty, value);
            }
        }

        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof(string), typeof(SnapshotItem), new PropertyMetadata(null));

        private void name_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentSnapshot != null) ParentSnapshot.Name = txt.Text;

            if (txt.IsKeyboardFocusWithin) SettingChanged?.Invoke();
        }

        #endregion

        #region "Type"

        public string SelectedType
        {
            get { return (string)GetValue(SelectedTypeProperty); }
            set
            {
                SetValue(SelectedTypeProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedTypeProperty =
            DependencyProperty.Register("SelectedType", typeof(string), typeof(SnapshotItem), new PropertyMetadata(null));


        ObservableCollection<string> typeitems;
        public ObservableCollection<string> TypeItems
        {
            get
            {
                if (typeitems == null)
                    typeitems = new ObservableCollection<string>();
                return typeitems;
            }

            set
            {
                typeitems = value;
            }
        }


        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;

            if (combo.SelectedItem != null)
            {
                var s = combo.SelectedItem.ToString().ToLower();

                try
                {
                    var type = (SnapshotType)Enum.Parse(typeof(SnapshotType), s, true);
                    if (ParentSnapshot != null) ParentSnapshot.Type = type;
                }
                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message, LogLineType.Warning); }
            }

            if (combo.IsKeyboardFocusWithin) SettingChanged?.Invoke();
        }

        #endregion

        #region "Link"

        public string SelectedVariableLink
        {
            get { return (string)GetValue(SelectedVariableLinkProperty); }
            set { SetValue(SelectedVariableLinkProperty, value); }
        }


        public static readonly DependencyProperty SelectedVariableLinkProperty =
            DependencyProperty.Register("SelectedVariableLink", typeof(string), typeof(SnapshotItem), new PropertyMetadata(null));


        public string SelectedLink
        {
            get { return (string)GetValue(SelectedLinkProperty); }
            set
            {
                SetValue(SelectedLinkProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedLinkProperty =
            DependencyProperty.Register("SelectedLink", typeof(string), typeof(SnapshotItem), new PropertyMetadata(null));


        ObservableCollection<object> linkitems;
        public ObservableCollection<object> LinkItems
        {
            get
            {
                if (linkitems == null)
                    linkitems = new ObservableCollection<object>();
                return linkitems;
            }

            set
            {
                linkitems = value;
            }
        }


        private void Link_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.SelectedItem != null)
            {
                if (ParentSnapshot != null)
                {
                    if (SelectedType == "Collected") ParentSnapshot.Link = ((Page.CollectedItem)combo.SelectedItem).Id;
                    else if (SelectedType == "Generated") ParentSnapshot.Link = ((Page.GeneratedEventItem)combo.SelectedItem).Id;
                    else ParentSnapshot.Link = combo.Text;
                }
            }

            if (combo.IsKeyboardFocusWithin)
            {
               SettingChanged?.Invoke();
            }
        }

        private void variable_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentSnapshot != null) ParentSnapshot.Link = txt.Text;

            if (txt.IsKeyboardFocused || txt.IsMouseCaptured)
            {
                SettingChanged?.Invoke();
            }
        }

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (ParentSnapshot != null) ParentSnapshot.Link = combo.Text;

            if (combo.IsKeyboardFocusWithin || combo.IsMouseCaptured)
            {
                SettingChanged?.Invoke();
            }
        }

        #endregion

        #region "Remove"

        public event Clicked_Handler RemoveClicked;

        private void Removed_Clicked(TrakHound_UI.Button bt)
        {
            RemoveClicked?.Invoke(this);
            SettingChanged?.Invoke();
        }

        #endregion

        public event Clicked_Handler RefreshClicked;

        private void Refresh_Clicked(TrakHound_UI.Button bt)
        {
            RefreshClicked?.Invoke(this);
        }

    }
}
