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
    /// <summary>
    /// Interaction logic for UpdateItem.xaml
    /// </summary>
    public partial class UpdateItem : UserControl
    {
        public UpdateItem()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public string ApplicationName { get; set; }

        public delegate void Clicked_Handler(UpdateItem item);
        public event Clicked_Handler CheckForUpdatesClicked;
        public event Clicked_Handler ApplyClicked;


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(UpdateItem), new PropertyMetadata(false));


        public bool UpdateAvailable
        {
            get { return (bool)GetValue(UpdateAvailableProperty); }
            set { SetValue(UpdateAvailableProperty, value); }
        }

        public static readonly DependencyProperty UpdateAvailableProperty =
            DependencyProperty.Register("UpdateAvailable", typeof(bool), typeof(UpdateItem), new PropertyMetadata(false));


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



        public string UpdateLastChecked
        {
            get { return (string)GetValue(UpdateLastCheckedProperty); }
            set { SetValue(UpdateLastCheckedProperty, value); }
        }

        public static readonly DependencyProperty UpdateLastCheckedProperty =
            DependencyProperty.Register("UpdateLastChecked", typeof(string), typeof(UpdateItem), new PropertyMetadata("Never"));


        public string UpdateLastInstalled
        {
            get { return (string)GetValue(UpdateLastInstalledProperty); }
            set { SetValue(UpdateLastInstalledProperty, value); }
        }

        public static readonly DependencyProperty UpdateLastInstalledProperty =
            DependencyProperty.Register("UpdateLastInstalled", typeof(string), typeof(UpdateItem), new PropertyMetadata("Never"));


        public bool Error
        {
            get { return (bool)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(bool), typeof(UpdateItem), new PropertyMetadata(false));


        private void Check_Clicked(TH_WPF.Button bt)
        {
            if (CheckForUpdatesClicked != null) CheckForUpdatesClicked(this);
        }

        private void Apply_Clicked(TH_WPF.Button bt)
        {
            if (ApplyClicked != null) ApplyClicked(this);
        }
    }
}
