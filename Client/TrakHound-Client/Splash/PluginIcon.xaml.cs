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

namespace TrakHound_Client.Splash
{
    /// <summary>
    /// Interaction logic for PluginIcon.xaml
    /// </summary>
    public partial class PluginIcon : UserControl
    {
        public PluginIcon()
        {
            InitializeComponent();
        }

        public StackPanel STACK;

        public bool IsShown
        {
            get { return (bool)GetValue(IsShownProperty); }
            set
            {
                SetValue(IsShownProperty, value);

                if (value)
                {
                    STACK.Children.Add(this);

                    Main_GRID.Visibility = System.Windows.Visibility.Visible;
                    SlideIn();
                }
            }
        }


        public static readonly DependencyProperty IsShownProperty =
            DependencyProperty.Register("IsShown", typeof(bool), typeof(PluginIcon), new PropertyMetadata(false));

        public void SlideIn()
        {

            Storyboard sb = new Storyboard();

            DoubleAnimation slide = new DoubleAnimation();
            slide.To = 0;
            slide.From = 20;
            slide.Duration = new Duration(new TimeSpan(0, 0, 1));

            // Set the target of the animation
            Storyboard.SetTarget(slide, Main_GRID);
            Storyboard.SetTargetProperty(slide, new PropertyPath("RenderTransform.(TranslateTransform.X)"));

            // Kick the animation off
            sb.Children.Add(slide);
            sb.Begin();
            

        }

    }
}
