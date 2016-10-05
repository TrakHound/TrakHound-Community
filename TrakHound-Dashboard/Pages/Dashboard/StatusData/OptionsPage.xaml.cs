// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

using TrakHound;

namespace TrakHound_Dashboard.Pages.Dashboard.StatusData
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class OptionsPage : UserControl, IPage
    {

        public OptionsPage()
        {
            InitializeComponent();
            DataContext = this;

            Load();
        }

        public string Title { get { return "Status Data"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Generate_01.png"); } }

        public bool ZoomEnabled { get { return false; } }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        public void SetZoom(double zoomPercentage) { }

        public event SendData_Handler SendData;

        public void GetSentData(EventData data) { }

        public void Load()
        {
            DatabaseReadInterval = Properties.Settings.Default.DatabaseReadInterval;
        }

        public void Save()
        {
            Properties.Settings.Default.DatabaseReadInterval = DatabaseReadInterval;
            Properties.Settings.Default.Save();
        }
       

        #region "Dependency Properties"

        private static void Value_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var o = obj as OptionsPage;
            if (o != null) o.Save();
        }

        public int DatabaseReadInterval
        {
            get { return (int)GetValue(DatabaseReadIntervalProperty); }
            set { SetValue(DatabaseReadIntervalProperty, value); }
        }

        public static readonly DependencyProperty DatabaseReadIntervalProperty =
            DependencyProperty.Register("DatabaseReadInterval", typeof(int), typeof(OptionsPage), new PropertyMetadata(5000, new PropertyChangedCallback(Value_PropertyChanged)));
        
        #endregion


        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }


        private void RestoreDefaults_Clicked(TrakHound_UI.Button bt)
        {
            DatabaseReadInterval = 5000;

            Save();
        }
    }
}
