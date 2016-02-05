// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;

namespace TrakHound_Server
{
    /// <summary>
    /// Interaction logic for Console.xaml
    /// </summary>
    public partial class Output_Console : Window
    {
        public Output_Console()
        {
            InitializeComponent();
            DataContext = this;
        }

        public class ConsoleItem : FrameworkElement
        {
            public Int64 Row { get; set; }
            public DateTime Timestamp { get; set; }
            public string Text { get; set; }
        }

        ObservableCollection<ConsoleItem> consoleOutput;
        public ObservableCollection<ConsoleItem> ConsoleOutput
        {
            get
            {
                if (consoleOutput == null) consoleOutput = new ObservableCollection<ConsoleItem>();
                return consoleOutput;
            }
            set { consoleOutput = value; }
        }

        Int64 rowIndex = 0;
        const int MaxLines = 500;


        public void AddLine(string line)
        {
            this.Dispatcher.BeginInvoke(new Action<string>(AddLine_GUI), new object[] { line });
        }

        void AddLine_GUI(string line)
        {
            string[] lines = line.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var l in lines)
            {
                var item = new ConsoleItem();
                item.Row = rowIndex;
                item.Timestamp = DateTime.Now;
                item.Text = l;

                rowIndex++;

                ConsoleOutput.Add(item);

                RemoveOldLines();

                ScrollLastIntoView();
            }
        }

        void RemoveOldLines()
        {
            if (ConsoleOutput.Count > MaxLines)
            {
                int first = ConsoleOutput.Count - MaxLines;

                for (int x = 0; x < first; x++) ConsoleOutput.RemoveAt(0);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void ScrollLastIntoView()
        {
            var scroll = GetScrollViewer(dg);
            if (scroll != null)
            {
                double bottomScrollPort = scroll.ScrollableHeight - 5;

                if (scroll.VerticalOffset >= bottomScrollPort)
                {
                    if (ConsoleOutput.Count > 0)
                    {
                        ConsoleItem lastItem = ConsoleOutput[ConsoleOutput.Count - 1];
                        dg.ScrollIntoView(lastItem);
                    }
                }
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
    }
}
