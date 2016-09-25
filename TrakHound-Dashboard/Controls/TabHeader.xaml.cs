// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

using TrakHound.Tools;

namespace TrakHound_Dashboard.Controls
{
    /// <summary>
    /// Interaction logic for TH_TabHeader.xaml
    /// </summary>
    public partial class TabHeader : UserControl
    {
        public TabHeader()
        {
            Id = Guid.NewGuid().ToString();

            InitializeComponent();
            DataContext = this;

            root.Opacity = 0;
            root.MaxWidth = START_WIDTH;
            MinWidth = START_WIDTH;
            MaxWidth = MAX_WIDTH;
        }

        public TabPage Page { get; set; }

        public string Tag { get; set; }
        public string Id { get; set; }

        // Text to be displayed on Tab
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TabHeader), new PropertyMetadata(null));


        // Image to be displayed on Tab
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(TabHeader), new PropertyMetadata(null));


        // Whether tab is selected or not
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TabHeader), new PropertyMetadata(false));



        #region "Events"

        public delegate void Click_Handler(TabHeader header);
        public event Click_Handler Clicked;
        public event Click_Handler CloseClicked;
        public event EventHandler Opened;
        public event EventHandler Closed;

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
            DependencyProperty.Register("Closing", typeof(bool), typeof(TabHeader), new PropertyMetadata(false));

        #endregion

        #region "Opening"

        public const double START_WIDTH = 45; // Just the Icon
        public const double MIN_WIDTH = 70; // Icon + Close Button
        public const double MAX_WIDTH = 320;

        const double TAB_OPENING_OPACITY_ANIMATION_TIME = 200;
        const double TAB_OPENING_HEIGHT_ANIMATION_TIME = 200;
        const double TAB_OPENING_WIDTH_ANIMATION_TIME = 300;

        public void Open(bool fade = false)
        {
            if (fade) AnimateTabOpening_Opacity();
            else root.Opacity = 1;

            AnimateTabOpening_Height();
        }

        private void AnimateTabOpening_Opacity()
        {
            var animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 1;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(TAB_OPENING_OPACITY_ANIMATION_TIME));
            animation.Completed += Animation_Completed;

            var ease = new CubicEase();
            ease.EasingMode = EasingMode.EaseIn;

            animation.EasingFunction = ease;
            root.BeginAnimation(OpacityProperty, animation);
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            AnimateTabOpening_Width();
        }

        private void AnimateTabOpening_Height()
        {
            var animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 32;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(TAB_OPENING_HEIGHT_ANIMATION_TIME));
            animation.Completed += Animation_Completed;

            var ease = new CubicEase();
            ease.EasingMode = EasingMode.EaseIn;

            animation.EasingFunction = ease;
            root.BeginAnimation(MaxHeightProperty, animation);
        }

        private void AnimateTabOpening_Width()
        {
            root.Opacity = 1;

            var animation = new DoubleAnimation();
            animation.From = START_WIDTH;
            animation.To = MaxWidth;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(TAB_OPENING_WIDTH_ANIMATION_TIME));
            animation.Completed += Opening_WIDTH_Completed;

            var ease = new CubicEase();
            ease.EasingMode = EasingMode.EaseIn;

            animation.EasingFunction = ease;
            root.BeginAnimation(MaxWidthProperty, animation);
        }

        private void Opening_WIDTH_Completed(object sender, EventArgs e)
        {
            MinWidth = MIN_WIDTH;
            if (Opened != null) Opened(this, new EventArgs());
        }

        #endregion

        #region "Closing"

        const double TAB_CLOSING_ANIMATION_TIME = 200;
        const double SPACE_CLOSING_ANIMATION_TIME = 200;

        double spaceWidth = 0;

        public void Close()
        {
            AnimateTabClosing();
        }

        private void AnimateTabClosing()
        {
            spaceWidth = root.ActualWidth;
            Width = spaceWidth;

            var animation = new DoubleAnimation();
            animation.Completed += TabClosingAnimation_Completed;
            animation.From = 1;
            animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(TAB_CLOSING_ANIMATION_TIME));

            var ease = new CubicEase();
            ease.EasingMode = EasingMode.EaseOut;

            animation.EasingFunction = ease;
            root.BeginAnimation(OpacityProperty, animation);
        }

        private void TabClosingAnimation_Completed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(AnimateSpaceClosing));
        }

        private void AnimateSpaceClosing()
        {
            MinWidth = 0;

            var animation = new DoubleAnimation();
            animation.Completed += SpaceClosingAnimation_Completed;
            animation.From = spaceWidth;
            animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(SPACE_CLOSING_ANIMATION_TIME));

            var ease = new CubicEase(); 
            ease.EasingMode = EasingMode.EaseOut;

            animation.EasingFunction = ease;
            BeginAnimation(WidthProperty, animation);
        }

        private void SpaceClosingAnimation_Completed(object sender, EventArgs e)
        {
            var args = new EventArgs();
            if (Closed != null) Closed(this, args);
        }

        #endregion

        #region "Mouse Over"

        public bool MouseOver
        {
            get { return (bool)GetValue(MouseOverProperty); }
            set { SetValue(MouseOverProperty, value); }
        }

        public static readonly DependencyProperty MouseOverProperty =
            DependencyProperty.Register("MouseOver", typeof(bool), typeof(TabHeader), new PropertyMetadata(false));


        private void Control_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsSelected) MouseOver = true;
        }

        private void Control_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
        }

        #endregion

        private const double MAX_TEXT_WIDTH = 100;

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                ChangeWidth(e.NewSize.Width, txt);
            }
        }

        private void ChangeWidth(double width, TextBlock txt)
        {
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    double maxWidth = MAX_TEXT_WIDTH;
            //    if (MaxWidth < MAX_WIDTH)
            //    {
            //        maxWidth = MaxWidth - 120;
            //        maxWidth = Math.Min(MAX_TEXT_WIDTH, maxWidth);
            //        maxWidth = Math.Max(10, maxWidth);
            //    }

            //    if (width > maxWidth)
            //    {
            //        string t = Text;

            //        if (t != null)
            //        {
            //            double textWidth = GetFormattedText(t).Width;

            //            if (textWidth > maxWidth)
            //            {
            //                // Keep removing characters from the string until the max width is met
            //                while (textWidth > maxWidth)
            //                {
            //                    t = t.Substring(0, t.Length - 1);
            //                    textWidth = GetFormattedText(t).Width;
            //                }

            //                // Make sure the last character is not a space
            //                if (t[t.Length - 1] == ' ' && txt.Text.Length > t.Length + 2) t = txt.Text.Substring(0, t.Length + 2);

            //                // Add the ...
            //                txt.Text = t + "...";
            //            }
            //            else txt.Text = Text;
            //        }
            //    }
            //    else txt.Text = Text;

            //}), MainWindow.PRIORITY_CONTEXT_IDLE, new object[] { });
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

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                ChangeWidth(e.NewSize.Width, txt);
            }
        }
    }

}
