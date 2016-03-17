// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Media.Animation;
using System.Collections.ObjectModel;

namespace TrakHound_Client.Controls.Developer_Console
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

        //double defaultHeight = 400;

        public delegate void ShownChanged_Handler(bool shown);
        public event ShownChanged_Handler ShownChanged;

        public bool Shown
        {
            get { return (bool)GetValue(ShownProperty); }
            set 
            { 
                SetValue(ShownProperty, value);

                if (ShownChanged != null) ShownChanged(value);

                //if (value) { Animate(defaultHeight, HeightProperty); }
                //else { Animate(0, HeightProperty); }
            }
        }

        public static readonly DependencyProperty ShownProperty =
            DependencyProperty.Register("Shown", typeof(bool), typeof(PopUp), new PropertyMetadata(false));


        public class ConsoleItem
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

        public void AddLine(string line)
        {
            if (line != null) Dispatcher.BeginInvoke(new Action<string>(AddLine_GUI), new object[] { line });
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

                CheckScrollPosition();
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
                ConsoleItem lastItem = ConsoleOutput[ConsoleOutput.Count - 1];
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

        //public void AddLine(string line)
        //{
        //    Console_Item ci = new Console_Item();
        //    ci.Row = rowIndex;
        //    ci.Timestamp = DateTime.Now;
        //    ci.Text = line;

        //    rowIndex++;

        //    this.Dispatcher.BeginInvoke(new Action<Console_Item>(AddLine_GUI), new object[] { ci });
        //}



        //void AddLine_GUI(Console_Item ci)
        //{
        //    Console_Output.Add(ci);

        //    if (Console_Output.Count > MaxLines)
        //    {
        //        int first = Console_Output.Count - MaxLines;

        //        for (int x = 0; x < first; x++) Console_Output.RemoveAt(0);
        //    }

        //    dg.ScrollIntoView(ci);
        //}
        
        private void Minimize_ToolBarItem_Clicked(TH_WPF.Button bt)
        {
            Shown = !Shown;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //if (sender.GetType() == typeof(DataGrid))
            //{
            //    DataGrid dg = (DataGrid)sender;
            //    dg.SelectedItem = e.Row.Item;
            //    dg.ScrollIntoView(e.Row.Item);
            //}
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollLastIntoView();


            //if (sender.GetType() == typeof(DataGrid))
            //{
            //    DataGrid dg = (DataGrid)sender;
            //    dg.SelectedItem = dg.Items[dg.Items.Count - 1];
            //    dg.ScrollIntoView(dg.SelectedItem);
            //}
        }

        private void Copy_Clicked(TH_WPF.Button bt)
        {
            dg.SelectAllCells();
            dg.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, dg);
            dg.UnselectAllCells();

        }
    }
}
