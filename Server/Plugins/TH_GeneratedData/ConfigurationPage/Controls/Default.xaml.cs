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

namespace TH_GeneratedData.ConfigurationPage.Controls
{
    /// <summary>
    /// Interaction logic for Default.xaml
    /// </summary>
    public partial class Default : UserControl
    {
        public Default()
        {
            InitializeComponent();
            DataContext = this;
        }

        public TH_GeneratedData.ConfigurationPage.Page.Result ParentResult;

        public string ValueName
        {
            get { return (string)GetValue(ValueNameProperty); }
            set { SetValue(ValueNameProperty, value); }
        }

        public static readonly DependencyProperty ValueNameProperty =
            DependencyProperty.Register("ValueName", typeof(string), typeof(Default), new PropertyMetadata(null));

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ParentResult != null) ParentResult.value = ((TextBox)sender).Text;
        }

        private void Edit_Clicked(TH_WPF.Button_02 bt)
        {

        }

    }
}
