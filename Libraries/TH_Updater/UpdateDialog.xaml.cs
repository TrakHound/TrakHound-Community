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

namespace TH_Updater
{
    /// <summary>
    /// Interaction logic for UpdateDialog.xaml
    /// </summary>
    public partial class UpdateDialog : Window
    {
        public UpdateDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void UpdateApplicationName(string name)
        {
            applicationName_TXT.Text = name;
        }

        public void UpdateProgressValue(int value)
        {

        }

        public void UpdateProgressMaximum(int value)
        {

        }

        #region "Properties"

        public string ApplicationName
        {
            get { return (string)GetValue(ApplicationNameProperty); }
            set { SetValue(ApplicationNameProperty, value); }
        }

        public static readonly DependencyProperty ApplicationNameProperty =
            DependencyProperty.Register("ApplicationName", typeof(string), typeof(UpdateDialog), new PropertyMetadata(null));

        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(UpdateDialog), new PropertyMetadata(null));


        public int ProgressValue
        {
            get { return (int)GetValue(ProgressValueProperty); }
            set { SetValue(ProgressValueProperty, value); }
        }

        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(int), typeof(UpdateDialog), new PropertyMetadata(0));


        public int ProgressMaximum
        {
            get { return (int)GetValue(ProgressMaximumProperty); }
            set { SetValue(ProgressMaximumProperty, value); }
        }

        public static readonly DependencyProperty ProgressMaximumProperty =
            DependencyProperty.Register("ProgressMaximum", typeof(int), typeof(UpdateDialog), new PropertyMetadata(100));


        #endregion

    }
}
