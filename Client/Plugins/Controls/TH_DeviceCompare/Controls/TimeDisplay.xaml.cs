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

namespace TH_DeviceCompare.Controls
{
    /// <summary>
    /// Interaction logic for TimeDisplay.xaml
    /// </summary>
    public partial class TimeDisplay : UserControl
    {
        public TimeDisplay()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TimeDisplay), new PropertyMetadata("Time Display"));




        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TimeDisplay), new PropertyMetadata(false));


        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(string), typeof(TimeDisplay), new PropertyMetadata("00:00:00"));


        public string Percentage
        {
            get { return (string)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(string), typeof(TimeDisplay), new PropertyMetadata(null));


        #region "Bar Properties"

        public int BarValue
        {
            get { return (int)GetValue(BarValueProperty); }
            set { SetValue(BarValueProperty, value); }
        }

        public static readonly DependencyProperty BarValueProperty =
            DependencyProperty.Register("BarValue", typeof(int), typeof(TimeDisplay), new PropertyMetadata(10));


        public int BarMaximum
        {
            get { return (int)GetValue(BarMaximumProperty); }
            set { SetValue(BarMaximumProperty, value); }
        }

        public static readonly DependencyProperty BarMaximumProperty =
            DependencyProperty.Register("BarMaximum", typeof(int), typeof(TimeDisplay), new PropertyMetadata(60));

        #endregion

        public SolidColorBrush BarBrush
        {
            get { return (SolidColorBrush)GetValue(BarBrushProperty); }
            set { SetValue(BarBrushProperty, value); }
        }

        public static readonly DependencyProperty BarBrushProperty =
            DependencyProperty.Register("BarBrush", typeof(SolidColorBrush), typeof(TimeDisplay), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(170,255,255,255))));

        
    }
}
