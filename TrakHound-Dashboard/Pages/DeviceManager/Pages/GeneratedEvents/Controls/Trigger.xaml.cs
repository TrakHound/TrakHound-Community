// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.GeneratedEvents.Controls
{
    /// <summary>
    /// Interaction logic for Trigger.xaml
    /// </summary>
    public partial class Trigger : UserControl
    {
        public Trigger()
        {
            InitializeComponent();
            DataContext = this;

            Modifiers.Add("Equal To");
            Modifiers.Add("Not Equal To");
            Modifiers.Add("Greater Than");
            Modifiers.Add("Less Than");
            Modifiers.Add("Contains");
            Modifiers.Add("Contains Match Case");
            Modifiers.Add("Contains Whole Word");
            Modifiers.Add("Contains Whole Word Match Case");          
        }

       
        public Page.Event ParentEvent;
        public Page.Value ParentValue;
        public Page.Trigger ParentTrigger;


        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(Trigger), new PropertyMetadata(null));


        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;


        ObservableCollection<object> dataitems;
        public ObservableCollection<object> DataItems
        {
            get
            {
                if (dataitems == null)
                    dataitems = new ObservableCollection<object>();
                return dataitems;
            }

            set
            {
                dataitems = value;
            }
        }

        ObservableCollection<object> modifiers;
        public ObservableCollection<object> Modifiers
        {
            get
            {
                if (modifiers == null)
                    modifiers = new ObservableCollection<object>();
                return modifiers;
            }

            set
            {
                modifiers = value;
            }
        }

        #region "Link Type"

        public string LinkType
        {
            get { return (string)GetValue(LinkTypeProperty); }
            set { SetValue(LinkTypeProperty, value); }
        }

        public static readonly DependencyProperty LinkTypeProperty =
            DependencyProperty.Register("LinkType", typeof(string), typeof(Trigger), new PropertyMetadata(null));

        private void LinkType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParentTrigger != null) ParentTrigger.linkType = LinkType;
        }

        #endregion

        #region "Value"

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(Trigger), new PropertyMetadata(null));

        private void value_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;

            if (ParentTrigger != null) ParentTrigger.value = txt.Text;

            if (txt.IsKeyboardFocused) if (SettingChanged != null) SettingChanged();
        }

        void LoadEventValues()
        {
            value_COMBO.Items.Clear();
            
            if (SelectedLink != null)
            {
                if (LinkType == "Type")
                {
                    int index = ParentPage.DataItemTypes.ToList().FindIndex(x => x == SelectedLink.ToString());
                    if (index >= 0)
                    {
                        var item = ParentPage.DataItemTypes[index];
                        if (item != null)
                        {
                            if (ParentPage != null)
                            {
                                var match = ParentPage.EventValues.ToList().Find(x => x.Type == item);
                                if (match != null)
                                {
                                    foreach (var value in match.Values)
                                    {
                                        value_COMBO.Items.Add(value);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    int index = ParentPage.CollectedItems.ToList().FindIndex(x => x.Id == SelectedLink.ToString());
                    if (index >= 0)
                    {
                        var item = ParentPage.CollectedItems[index];
                        if (item != null)
                        {
                            if (item.Category == "EVENT")
                            {
                                if (ParentPage != null)
                                {
                                    var match = ParentPage.EventValues.ToList().Find(x => x.Type == item.Type);
                                    if (match != null)
                                    {
                                        foreach (var value in match.Values)
                                        {
                                            value_COMBO.Items.Add(value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Value_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;

            if (ParentTrigger != null)
            {
                if (combo.SelectedItem != null)
                {
                    ParentTrigger.value = combo.SelectedItem.ToString();
                }
            }

            if (combo.IsKeyboardFocusWithin) SettingChanged?.Invoke();
        }

        private void Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            var combo = (ComboBox)sender;

            if (ParentTrigger != null) ParentTrigger.value = combo.Text;

            if (combo.IsKeyboardFocusWithin) SettingChanged?.Invoke();
        }

        #endregion

        #region "Modifier"

        private void Modifier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;

            if (ParentTrigger != null)
            {
                if (combo.SelectedItem != null)
                {
                    ParentTrigger.modifier = combo.SelectedItem.ToString();
                }
            }

            if (combo.IsKeyboardFocusWithin) SettingChanged?.Invoke();
        }

        #endregion

        #region "Link"

        public object SelectedLink
        {
            get { return (object)GetValue(SelectedLinkProperty); }
            set { SetValue(SelectedLinkProperty, value); }
        }

        public static readonly DependencyProperty SelectedLinkProperty =
            DependencyProperty.Register("SelectedLink", typeof(object), typeof(Trigger), new PropertyMetadata(null));


        private void Link_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;

            if (ParentTrigger != null)
            {
                if (combo.SelectedItem != null)
                {
                    ParentTrigger.link = combo.SelectedItem.ToString();

                    LoadEventValues();
                }
            }

            if (combo.IsKeyboardFocusWithin) SettingChanged?.Invoke();
        }

        private void Link_TextChanged(object sender, TextChangedEventArgs e)
        {
            var combo = (ComboBox)sender;

            if (ParentTrigger != null) ParentTrigger.link = combo.Text;

            if (combo.IsKeyboardFocusWithin) SettingChanged?.Invoke();
        }

        #endregion

        #region "Remove"

        public delegate void RemoveClicked_Handler(Trigger t);
        public event RemoveClicked_Handler RemoveClicked;

        private void Remove_Clicked(TrakHound_UI.Button bt)
        {
            RemoveClicked?.Invoke(this);

            SettingChanged?.Invoke();
        }

        #endregion

        object oldFocus = null;

        private void TXT_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotFocus(object sender, RoutedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotMouseCapture(object sender, MouseEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { oldFocus = null; }

    }
}
