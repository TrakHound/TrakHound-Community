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

        public class Console_Item
        {
            public Int64 Row { get; set; }
            public DateTime Timestamp { get; set; }
            public string Text { get; set; }
        }

        ObservableCollection<Console_Item> console_output;
        public ObservableCollection<Console_Item> Console_Output
        {
            get
            {
                if (console_output == null) console_output = new ObservableCollection<Console_Item>();
                return console_output;
            }
            set { console_output = value; }
        }

        Int64 rowIndex = 0;
        const int MaxLines = 500;


        public void AddLine(string line)
        {
            Console_Item ci = new Console_Item();
            ci.Row = rowIndex;
            ci.Timestamp = DateTime.Now;
            ci.Text = line;

            rowIndex++;

            this.Dispatcher.BeginInvoke(new Action<Console_Item>(AddLine_GUI), new object[] { ci });
        }

        void AddLine_GUI(Console_Item ci)
        {
            Console_Output.Add(ci);

            if (Console_Output.Count > MaxLines)
            {
                int first = Console_Output.Count - MaxLines;

                for (int x = 0; x < first; x++) Console_Output.RemoveAt(0);
            }

            dg.ScrollIntoView(ci);

            //scrollviewer.ScrollToEnd();

            //var scrollerViewer = GetScrollViewer();
            //if (scrollerViewer != null) scrollerViewer.ScrollToEnd();
        }

        //ScrollViewer GetScrollViewer()
        //{
        //    if (VisualTreeHelper.GetChildrenCount(this) == 0) return null;
        //    var x = VisualTreeHelper.GetChild(this, 0);
        //    if (x == null) return null;
        //    if (VisualTreeHelper.GetChildrenCount(x) == 0) return null;
        //    return VisualTreeHelper.GetChild(x, 0) as ScrollViewer;
        //}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
