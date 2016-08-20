// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using TrakHound;
using TrakHound.Logging;

namespace TrakHound_Dashboard.Controls.Developer_Console
{
    /// <summary>
    /// Interaction logic for PopUp.xaml
    /// </summary>
    public partial class PopUp : UserControl
    {
        public PopUp()
        {
            InitializeComponent();
            DataContext = this;
        }

        const int MaxLines = 500;

        public delegate void ShownChanged_Handler(bool shown);
        public event ShownChanged_Handler ShownChanged;

        public bool Shown
        {
            get { return (bool)GetValue(ShownProperty); }
            set 
            { 
                SetValue(ShownProperty, value);

                if (ShownChanged != null) ShownChanged(value);
            }
        }

        public static readonly DependencyProperty ShownProperty =
            DependencyProperty.Register("Shown", typeof(bool), typeof(PopUp), new PropertyMetadata(false));



        public Line SelectedLine
        {
            get { return (Line)GetValue(SelectedLineProperty); }
            set { SetValue(SelectedLineProperty, value); }
        }

        public static readonly DependencyProperty SelectedLineProperty =
            DependencyProperty.Register("SelectedLine", typeof(Line), typeof(PopUp), new PropertyMetadata(null));


        private class OutputGroup
        {
            public OutputGroup()
            {
                Lines = new List<Line>();
            }

            public string ApplicationName { get; set; }

            public Int64 RowIndex { get; set; }

            public List<Line> Lines { get; set; }
        }

        List<OutputGroup> outputGroups = new List<OutputGroup>();

        private string _currentOutput;
        public string CurrentOutput
        {
            get { return _currentOutput; }
            set
            {
                if (value != null && _currentOutput != value)
                {
                    Dispatcher.BeginInvoke(new Action<string>(SetCurrentOutput), MainWindow.PRIORITY_BACKGROUND, new object[] { value });
                }
                    
                _currentOutput = value;
            }
        }

        private void SetCurrentOutput(string currentOutput)
        {
            ConsoleOutput.Clear();

            var outputGroup = outputGroups.Find(x => x.ApplicationName.ToLower() == currentOutput.ToLower());
            if (outputGroup != null)
            {
                foreach (var line in outputGroup.Lines)
                {
                    ConsoleOutput.Add(line);
                }
            }
        }

        ObservableCollection<Line> consoleOutput;
        public ObservableCollection<Line> ConsoleOutput
        {
            get
            {
                if (consoleOutput == null) consoleOutput = new ObservableCollection<Line>();
                return consoleOutput;
            }
            set { consoleOutput = value; }
        }

        Int64 rowIndex = 0;

        public void AddLine(Line line, string applicationName)
        {
            if (line != null) Dispatcher.BeginInvoke(new Action<Line, string>(AddLine_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { line, applicationName });
        }

        void AddLine_GUI(Line line, string applicationName)
        {
            var outputGroup = outputGroups.Find(x => x.ApplicationName.ToLower() == applicationName.ToLower());
            if (outputGroup == null)
            {
                outputGroup = new OutputGroup();
                outputGroup.ApplicationName = applicationName;
                outputGroups.Add(outputGroup);
            }
            line.Row = outputGroup.RowIndex++;
            outputGroup.Lines.Add(line);

            if (applicationName == CurrentOutput) AddConsoleLine(line);
        }

        void AddConsoleLine(Line line)
        {
            ConsoleOutput.Add(line);

            rowIndex++;
            RemoveOldLines();
            CheckScrollPosition();
        }

        void RemoveOldLines()
        {
            foreach (var outputGroup in outputGroups)
            {
                if (outputGroup.Lines.Count > MaxLines)
                {
                    int first = outputGroup.Lines.Count - MaxLines;

                    for (int x = 0; x < first; x++) outputGroup.Lines.RemoveAt(0);
                }
            }

            if (ConsoleOutput.Count > MaxLines)
            {
                int first = ConsoleOutput.Count - MaxLines;

                for (int x = 0; x < first; x++) ConsoleOutput.RemoveAt(0);
            }
        }

        private void CheckScrollPosition()
        {
            var scroll = GetScrollViewer(dg);
            if (scroll != null)
            {
                double bottomScrollPort = scroll.ScrollableHeight - 5;

                if (scroll.VerticalOffset >= bottomScrollPort)
                {
                    ScrollLastIntoView();
                }
            }
        }

        public void ScrollLastIntoView()
        {
            if (ConsoleOutput.Count > 0)
            {
                var lastItem = ConsoleOutput[ConsoleOutput.Count - 1];
                dg.ScrollIntoView(lastItem);
            }
        }

        ScrollViewer GetScrollViewer(DependencyObject obj)
        {
            ScrollViewer result = null;

            int childCount = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is ScrollViewer)
                {
                    result = (ScrollViewer)(VisualTreeHelper.GetChild(obj, i));
                    break;
                }
                else
                {
                    result = GetScrollViewer(child);
                    if (result != null) break;
                }
            }
            return result;
        }

        private void Minimize_ToolBarItem_Clicked(TrakHound_UI.Button bt)
        {
            Shown = !Shown;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollLastIntoView();
        }

        private void dg_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (dg.SelectedItem != null && dg.SelectedItems != null && dg.SelectedItems.Count == 1)
            {
                SelectedLine = (Line)dg.SelectedItem;
            }
        }


        private void Copy_Clicked(TrakHound_UI.Button bt)
        {
            dg.SelectAllCells();
            dg.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, dg);
            dg.UnselectAllCells();

        }

        private void Clear_Clicked(TrakHound_UI.Button bt)
        {
            ConsoleOutput.Clear();
        }


        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            CurrentOutput = ApplicationNames.TRAKHOUND_DASHBOARD;
            server_BT.IsChecked = false;
        }

        private void Server_Click(object sender, RoutedEventArgs e)
        {
            CurrentOutput = ApplicationNames.TRAKHOUND_SERVER;
            dashboard_BT.IsChecked = false;
        }


        #region "Details"

        public bool DetailsShown
        {
            get { return (bool)GetValue(DetailsShownProperty); }
            set { SetValue(DetailsShownProperty, value); }
        }

        public static readonly DependencyProperty DetailsShownProperty =
            DependencyProperty.Register("DetailsShown", typeof(bool), typeof(PopUp), new PropertyMetadata(false));


        private void Details_Clicked(TrakHound_UI.Button bt)
        {
            DetailsShown = !DetailsShown;
        }

        #endregion

    }
}
