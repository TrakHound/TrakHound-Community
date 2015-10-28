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

namespace TrakHound_Server_Control_Panel.Controls
{
    /// <summary>
    /// Interaction logic for TH_Button.xaml
    /// </summary>
    public partial class TH_Button : UserControl
    {
        public TH_Button()
        {
            InitializeComponent();
            DataContext = this;

            //LoadStyle(AlternateStyle);
        }


        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TH_Button), new PropertyMetadata(false));


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(TH_Button), new PropertyMetadata(null));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TH_Button), new PropertyMetadata(null));


        public bool AlternateStyle
        {
            get { return (bool)GetValue(AlternateStyleProperty); }
            set 
            { 
                SetValue(AlternateStyleProperty, value);

                //LoadStyle(value);          
            }
        }

        public static readonly DependencyProperty AlternateStyleProperty =
            DependencyProperty.Register("AlternateStyle", typeof(bool), typeof(TH_Button), new PropertyMetadata(false));


        //void LoadStyle(bool alternate)
        //{
        //    if (alternate)
        //    {
        //        MainBorderStyle = (Style)TryFindResource("MainBorder_Alternate");
        //        ImageRectangleStyle = (Style)TryFindResource("ImageRectangle_Alternate");
        //        TextLabelStyle = (Style)TryFindResource("TextLabel_Alternate");
        //    }
        //    else
        //    {
        //        MainBorderStyle = (Style)TryFindResource("MainBorder_Normal");
        //        ImageRectangleStyle = (Style)TryFindResource("ImageRectangle_Normal");
        //        TextLabelStyle = (Style)TryFindResource("TextLabel_Normal");
        //    }
        //}


        public Style MainBorderStyle
        {
            get { return (Style)GetValue(MainBorderStyleProperty); }
            set { SetValue(MainBorderStyleProperty, value); }
        }

        public static readonly DependencyProperty MainBorderStyleProperty =
            DependencyProperty.Register("MainBorderStyle", typeof(Style), typeof(TH_Button), new PropertyMetadata(null));


        public Style ImageRectangleStyle
        {
            get { return (Style)GetValue(ImageRectangleStyleProperty); }
            set { SetValue(ImageRectangleStyleProperty, value); }
        }

        public static readonly DependencyProperty ImageRectangleStyleProperty =
            DependencyProperty.Register("ImageRectangleStyle", typeof(Style), typeof(TH_Button), new PropertyMetadata(null));


        public Style TextLabelStyle
        {
            get { return (Style)GetValue(TextLabelStyleProperty); }
            set { SetValue(TextLabelStyleProperty, value); }
        }

        public static readonly DependencyProperty TextLabelStyleProperty =
            DependencyProperty.Register("TextLabelStyle", typeof(Style), typeof(TH_Button), new PropertyMetadata(null));

        

        public delegate void Clicked_Handler(TH_Button bt);
        public event Clicked_Handler Clicked;

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }
    }

    //class StyleConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        FrameworkElement targetElement = values[0] as FrameworkElement;
    //        string styleName = values[1] as string;

    //        if (styleName == null)
    //            return null;

    //        Style newStyle = (Style)targetElement.TryFindResource(styleName);

    //        if (newStyle == null)
    //            newStyle = (Style)targetElement.TryFindResource("Normal");

    //        return newStyle;
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
