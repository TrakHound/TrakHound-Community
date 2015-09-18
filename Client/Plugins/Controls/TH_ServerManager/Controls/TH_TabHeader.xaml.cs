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

namespace TH_ServerManager.Controls
{
    /// <summary>
    /// Interaction logic for TH_TabHeader.xaml
    /// </summary>
    public partial class TH_TabHeader : UserControl
    {
        public TH_TabHeader()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public TH_TabItem TabParent
        {
            get { return (TH_TabItem)GetValue(TabParentProperty); }
            set { SetValue(TabParentProperty, value); }
        }

        public static readonly DependencyProperty TabParentProperty =
            DependencyProperty.Register("TabParent", typeof(TH_TabItem), typeof(TH_TabHeader), new PropertyMetadata(null));

        

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TH_TabHeader), new PropertyMetadata(null));


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(TH_TabHeader), new PropertyMetadata(null));


        public delegate void Click_Handler(TH_TabHeader header);
        public event Click_Handler Clicked;
        public event Click_Handler CloseClicked;

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }

        private void TabItemClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CloseClicked != null) CloseClicked(this);
        }



        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TH_TabHeader), new PropertyMetadata(false));


        public bool MouseOver
        {
            get { return (bool)GetValue(MouseOverProperty); }
            set { SetValue(MouseOverProperty, value); }
        }

        public static readonly DependencyProperty MouseOverProperty =
            DependencyProperty.Register("MouseOver", typeof(bool), typeof(TH_TabHeader), new PropertyMetadata(false));

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseOver = true;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
        }










        // Create a custom routed event by first registering a RoutedEventID
        // This event uses the bubbling routing strategy
        public static readonly RoutedEvent TapEvent = EventManager.RegisterRoutedEvent(
            "Tap", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TH_TabHeader));

        // Provide CLR accessors for the event
        public event RoutedEventHandler Tap
        {
            add { AddHandler(TapEvent, value); }
            remove { RemoveHandler(TapEvent, value); }
        }

        // This method raises the Tap event
        void RaiseTapEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(TH_TabHeader.TapEvent);
            RaiseEvent(newEventArgs);
        }








        public bool Closing
        {
            get { return (bool)GetValue(ClosingProperty); }
            set { SetValue(ClosingProperty, value); }
        }

        public static readonly DependencyProperty ClosingProperty =
            DependencyProperty.Register("Closing", typeof(bool), typeof(TH_TabHeader), new PropertyMetadata(false));



        
    }
}
