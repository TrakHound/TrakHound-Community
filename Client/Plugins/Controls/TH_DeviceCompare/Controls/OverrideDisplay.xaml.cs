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

namespace TH_DeviceCompare.Controls
{
    /// <summary>
    /// Interaction logic for OverrideDisplay.xaml
    /// </summary>
    public partial class OverrideDisplay : UserControl
    {
        public OverrideDisplay()
        {
            InitializeComponent();
            DataContext = this;

            override_INDICATOR.TotalLevelCount = 5;
            override_INDICATOR.ActiveLevelCount = 0;
        }


        /// <summary>
        /// Override Value between 0 and 1.25 (0% and 125%)
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set 
            { 
                SetValue(ValueProperty, value);

                if (value <= 0) override_INDICATOR.ActiveLevelCount = 0;
                else if (value < 25) override_INDICATOR.ActiveLevelCount = 1;
                else if (value < 50) override_INDICATOR.ActiveLevelCount = 2;
                else if (value < 75) override_INDICATOR.ActiveLevelCount = 3;
                else if (value < 100) override_INDICATOR.ActiveLevelCount = 4;
                else override_INDICATOR.ActiveLevelCount = 5;

                ValueText = (value * 0.01).ToString("P1");
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(OverrideDisplay), new PropertyMetadata(0d));

        
        

        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(OverrideDisplay), new PropertyMetadata("0.0%"));

    }
}
