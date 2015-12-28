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
        }
        
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
            //if (sender.GetType() == typeof(DataGrid))
            //{
            //    DataGrid dg = (DataGrid)sender;
            //    dg.SelectedItem = dg.Items[dg.Items.Count - 1];
            //    dg.ScrollIntoView(dg.SelectedItem);
            //}
        }
    }
}
