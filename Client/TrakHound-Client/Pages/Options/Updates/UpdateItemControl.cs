using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrakHound_Client.Pages.Options.Updates
{
    public class UpdateItem : Control
    {
        static UpdateItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UpdateItem), new FrameworkPropertyMetadata(typeof(UpdateItem)));
        }


        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(UpdateItem), new PropertyMetadata(false));



        public string ApplicationName
        {
            get { return (string)GetValue(ApplicationNameProperty); }
            set { SetValue(ApplicationNameProperty, value); }
        }

        public static readonly DependencyProperty ApplicationNameProperty =
            DependencyProperty.Register("ApplicationName", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));




        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));



        public double ProgressValue
        {
            get { return (double)GetValue(ProgressValueProperty); }
            set { SetValue(ProgressValueProperty, value); }
        }

        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(double), typeof(UpdateItem), new PropertyMetadata(0d));




        public string ApplicationTitle
        {
            get { return (string)GetValue(ApplicationTitleProperty); }
            set { SetValue(ApplicationTitleProperty, value); }
        }

        public static readonly DependencyProperty ApplicationTitleProperty =
            DependencyProperty.Register("ApplicationTitle", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));



        public string ApplicationSubtitle
        {
            get { return (string)GetValue(ApplicationSubtitleProperty); }
            set { SetValue(ApplicationSubtitleProperty, value); }
        }

        public static readonly DependencyProperty ApplicationSubtitleProperty =
            DependencyProperty.Register("ApplicationSubtitle", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));



        public string AvailableVersion
        {
            get { return (string)GetValue(AvailableVersionProperty); }
            set { SetValue(AvailableVersionProperty, value); }
        }

        public static readonly DependencyProperty AvailableVersionProperty =
            DependencyProperty.Register("AvailableVersion", typeof(string), typeof(UpdateItem), new PropertyMetadata(null));


    }
}
