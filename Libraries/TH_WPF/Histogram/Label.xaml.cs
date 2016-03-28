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

namespace TH_WPF.Histogram
{
    /// <summary>
    /// Interaction logic for Label.xaml
    /// </summary>
    public partial class Label : UserControl
    {
        public Label()
        {
            InitializeComponent();
            root.DataContext = this;
        }



        public double TopPadding
        {
            get { return (double)GetValue(TopPaddingProperty); }
            set { SetValue(TopPaddingProperty, value); }
        }

        public static readonly DependencyProperty TopPaddingProperty =
            DependencyProperty.Register("TopPadding", typeof(double), typeof(Label), new PropertyMetadata(0d));



        public double BottomPadding
        {
            get { return (double)GetValue(BottomPaddingProperty); }
            set { SetValue(BottomPaddingProperty, value); }
        }

        public static readonly DependencyProperty BottomPaddingProperty =
            DependencyProperty.Register("BottomPadding", typeof(double), typeof(Label), new PropertyMetadata(0d));



        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Label), new PropertyMetadata(0d));



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Label), new PropertyMetadata(null));


    }
}
