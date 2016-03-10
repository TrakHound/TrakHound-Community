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
using System.Globalization;
using System.Windows.Shapes;

namespace TrakHound_Client.Controls
{
    /// <summary>
    /// Interaction logic for TH_TabHeader.xaml
    /// </summary>
    public partial class TH_TabHeader_Top : UserControl
    {
        public TH_TabHeader_Top()
        {
            InitializeComponent();
            DataContext = this;
        }

        // Parent TH_TabItem to use for getting TH_TabItem when TH_TabHeader is clicked/closed/etc.
        public TH_TabItem TabParent
        {
            get { return (TH_TabItem)GetValue(TabParentProperty); }
            set { SetValue(TabParentProperty, value); }
        }

        public static readonly DependencyProperty TabParentProperty =
            DependencyProperty.Register("TabParent", typeof(TH_TabItem), typeof(TH_TabHeader_Top), new PropertyMetadata(null));


        // Text to be displayed on Tab
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TH_TabHeader_Top), new PropertyMetadata(null));


        // Image to be displayed on Tab
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(TH_TabHeader_Top), new PropertyMetadata(null));

        // Whether tab is selected or not
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TH_TabHeader_Top), new PropertyMetadata(false));


        #region "Events"

        public delegate void Click_Handler(TH_TabHeader_Top header);
        public event Click_Handler Clicked;
        public event Click_Handler CloseClicked;

        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Clicked != null) Clicked(this);
            }

        }

        private void TabItemClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CloseClicked != null) CloseClicked(this);
        }

        public bool Closing
        {
            get { return (bool)GetValue(ClosingProperty); }
            set { SetValue(ClosingProperty, value); }
        }

        public static readonly DependencyProperty ClosingProperty =
            DependencyProperty.Register("Closing", typeof(bool), typeof(TH_TabHeader_Top), new PropertyMetadata(false));

        #endregion

        #region "Mouse Over"

        public bool MouseOver
        {
            get { return (bool)GetValue(MouseOverProperty); }
            set { SetValue(MouseOverProperty, value); }
        }

        public static readonly DependencyProperty MouseOverProperty =
            DependencyProperty.Register("MouseOver", typeof(bool), typeof(TH_TabHeader_Top), new PropertyMetadata(false));


        private void Control_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsSelected) MouseOver = true;
        }

        private void Control_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
        }

        #endregion

        const double MAX_TEXT_WIDTH = 200;

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                if (e.NewSize.Width > MAX_TEXT_WIDTH)
                {
                    var txt = (TextBlock)sender;

                    string t = txt.Text;

                    if (t != null)
                    {
                        // Keep removing characters from the string until the max width is met
                        while (GetFormattedText(t).Width > MAX_TEXT_WIDTH)
                        {
                            t = t.Substring(0, t.Length - 1);
                        }

                        // Make sure the last character is not a space
                        if (t[t.Length - 1] == ' ' && txt.Text.Length > t.Length + 2) t = txt.Text.Substring(0, t.Length + 2);

                        // Add the ...
                        txt.Text = t + "...";
                    }
                }
            }
        }

        private static FormattedText GetFormattedText(string s)
        {
            return new FormattedText(
                        s,
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        13,
                        Brushes.Black);
        }

    }

    public class DesignTime_TH_TabHeader_Top : TH_TabHeader_Top
    {
        public DesignTime_TH_TabHeader_Top()
        {
            Text = "This is From a Design Time Class";
            MouseOver = true;
            IsSelected = true;
        }
    }
}
