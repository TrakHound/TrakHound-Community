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

using TH_WPF;

namespace TrakHound_Client.Pages.About
{
    public partial class Manager : UserControl
    {
        public Manager()
        {
            InitializeComponent();
        }

        public string CurrentPageName
        {
            get { return (string)GetValue(CurrentPageNameProperty); }
            set { SetValue(CurrentPageNameProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageNameProperty =
            DependencyProperty.Register("CurrentPageName", typeof(string), typeof(Manager), new PropertyMetadata(null));
     

        public void AddPage(TH_Global.Page page)
        {
            ListButton lb = new ListButton();
            lb.Image = Image_Functions.SetImageSize(page.Image, 20);
            lb.Text = page.PageName;
            lb.Selected += ListButton_Selected;
            lb.DataObject = page;

            Pages_STACK.Children.Add(lb);
        }

        public void RemovePage(TH_Global.Page page)
        {
            foreach (ListButton lb in Pages_STACK.Children.OfType<ListButton>())
            {
                if (lb.Text.ToUpper() == page.PageName.ToUpper())
                {
                    Pages_STACK.Children.Remove(lb);
                }
            } 
        }

        private void ListButton_Selected(ListButton LB)
        {
            foreach (ListButton oLB in Pages_STACK.Children.OfType<ListButton>())
            {
                if (oLB == LB) oLB.IsSelected = true;
                else oLB.IsSelected = false;
            }

            UserControl optionsControl = LB.DataObject as UserControl;

            CurrentPageName = LB.Text;

            Content_GRID.Children.Clear();
            Content_GRID.Children.Add(optionsControl);
        }

        private void manager_Loaded(object sender, RoutedEventArgs e)
        {
            if (Pages_STACK.Children.Count > 0)
            {
                if (Pages_STACK.Children[0].GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton) Pages_STACK.Children[0];

                    foreach (ListButton oLB in Pages_STACK.Children.OfType<ListButton>())
                    {
                        if (oLB == lb) oLB.IsSelected = true;
                        else oLB.IsSelected = false;
                    }

                    UserControl optionsControl = lb.DataObject as UserControl;

                    CurrentPageName = lb.Text;

                    Content_GRID.Children.Clear();
                    Content_GRID.Children.Add(optionsControl);
                }
            }
        }

    }
}
