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

namespace TH_DeviceCompare.Components
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Column column;

        public int Index;

        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Header), new PropertyMetadata(false));


        public string LastUpdatedTimestamp
        {
            get { return (string)GetValue(LastUpdatedTimestampProperty); }
            set { SetValue(LastUpdatedTimestampProperty, value); }
        }

        public static readonly DependencyProperty LastUpdatedTimestampProperty =
            DependencyProperty.Register("LastUpdatedTimestamp", typeof(string), typeof(Header), new PropertyMetadata("Never"));

        


        public int ProductionLevelCount
        {
            get { return (int)GetValue(ProductionLevelCountProperty); }
            set { SetValue(ProductionLevelCountProperty, value); }
        }

        public static readonly DependencyProperty ProductionLevelCountProperty =
            DependencyProperty.Register("ProductionLevelCount", typeof(int), typeof(Header), new PropertyMetadata(0));


        public int ProductionLevel
        {
            get { return (int)GetValue(ProductionLevelProperty); }
            set { SetValue(ProductionLevelProperty, value); }
        }

        public static readonly DependencyProperty ProductionLevelProperty =
            DependencyProperty.Register("ProductionLevel", typeof(int), typeof(Header), new PropertyMetadata(0));




        public string Device_Description
        {
            get { return (string)GetValue(Device_DescriptionProperty); }
            set { SetValue(Device_DescriptionProperty, value); }
        }

        public static readonly DependencyProperty Device_DescriptionProperty =
            DependencyProperty.Register("Device_Description", typeof(string), typeof(Header), new PropertyMetadata(""));


        public string Device_Manufacturer
        {
            get { return (string)GetValue(Device_ManufacturerProperty); }
            set { SetValue(Device_ManufacturerProperty, value); }
        }

        public static readonly DependencyProperty Device_ManufacturerProperty =
            DependencyProperty.Register("Device_Manufacturer", typeof(string), typeof(Header), new PropertyMetadata(""));


        public string Device_Model
        {
            get { return (string)GetValue(Device_ModelProperty); }
            set { SetValue(Device_ModelProperty, value); }
        }

        public static readonly DependencyProperty Device_ModelProperty =
            DependencyProperty.Register("Device_Model", typeof(string), typeof(Header), new PropertyMetadata(""));


        public string Device_Serial
        {
            get { return (string)GetValue(Device_SerialProperty); }
            set { SetValue(Device_SerialProperty, value); }
        }

        public static readonly DependencyProperty Device_SerialProperty =
            DependencyProperty.Register("Device_Serial", typeof(string), typeof(Header), new PropertyMetadata(""));







        public ImageSource Device_Image
        {
            get { return (ImageSource)GetValue(Device_ImageProperty); }
            set { SetValue(Device_ImageProperty, value); }
        }

        public static readonly DependencyProperty Device_ImageProperty =
            DependencyProperty.Register("Device_Image", typeof(ImageSource), typeof(Header), new PropertyMetadata(null));



        public ImageSource Device_Logo
        {
            get { return (ImageSource)GetValue(Device_LogoProperty); }
            set { SetValue(Device_LogoProperty, value); }
        }

        public static readonly DependencyProperty Device_LogoProperty =
            DependencyProperty.Register("Device_Logo", typeof(ImageSource), typeof(Header), new PropertyMetadata(null));




        public string Device_ID
        {
            get { return (string)GetValue(Device_IDProperty); }
            set { SetValue(Device_IDProperty, value); }
        }

        public static readonly DependencyProperty Device_IDProperty =
            DependencyProperty.Register("Device_ID", typeof(string), typeof(Header), new PropertyMetadata(""));

        



        public bool Alert
        {
            get { return (bool)GetValue(AlertProperty); }
            set { SetValue(AlertProperty, value); }
        }

        public static readonly DependencyProperty AlertProperty =
            DependencyProperty.Register("Alert", typeof(bool), typeof(Header), new PropertyMetadata(true));


        public bool Break
        {
            get { return (bool)GetValue(BreakProperty); }
            set { SetValue(BreakProperty, value); }
        }

        public static readonly DependencyProperty BreakProperty =
            DependencyProperty.Register("Break", typeof(bool), typeof(Header), new PropertyMetadata(false));

        



        public bool Minimized
        {
            get { return (bool)GetValue(MinimizedProperty); }
            set { SetValue(MinimizedProperty, value); }
        }

        public static readonly DependencyProperty MinimizedProperty =
            DependencyProperty.Register("Minimized", typeof(bool), typeof(Header), new PropertyMetadata(true));

        

        
        



        
        public SolidColorBrush AccentBrush
        {
            get { return (SolidColorBrush)GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register("AccentBrush", typeof(SolidColorBrush), typeof(Header), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(180, 180, 180))));


        #region "Mouse Over"

        public bool MouseOver
        {
            get { return (bool)GetValue(MouseOverProperty); }
            set { SetValue(MouseOverProperty, value); }
        }

        public static readonly DependencyProperty MouseOverProperty =
            DependencyProperty.Register("MouseOver", typeof(bool), typeof(Header), new PropertyMetadata(false));

        private void Control_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseOver = true;
            if (column != null) column.MouseOver = true;
        }

        private void Control_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
            if (column != null) column.MouseOver = false;
        }

        #endregion

        #region "IsSelected"

        public delegate void Clicked_Handler(int Index);
        public event Clicked_Handler Clicked;

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Header), new PropertyMetadata(false));

        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(Index);
        }

        #endregion


        

        


    }
}
