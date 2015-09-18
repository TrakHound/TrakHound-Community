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

using System.ComponentModel;

namespace TH_WPF
{
    /// <summary>
    /// Interaction logic for ProductionIndicator.xaml
    /// </summary>
    public partial class ProductionIndicator : UserControl
    {

        Color[] BackColors;

        private List<Color> IndicatorColors = new List<Color>();


        public ProductionIndicator()
        {

        InitializeComponent();


        Color Alert_COLOR = (Color)FindResource("Alert_True_Color");
        Color Production_COLOR = (Color)FindResource("Alert_False_Color");

        BackColors = new Color[]
                {                                             
                Alert_COLOR,                                                //0

                (Color)ColorConverter.ConvertFromString("#FF4000"),         //1
                (Color)ColorConverter.ConvertFromString("#FF7F00"),         //2
                (Color)ColorConverter.ConvertFromString("#FFBF00"),         //3

                Colors.Yellow,                                              //4

                (Color)ColorConverter.ConvertFromString("#BFFF00"),         //5
                (Color)ColorConverter.ConvertFromString("#7FFF00"),         //6
                (Color)ColorConverter.ConvertFromString("#40FF00"),         //7

                Production_COLOR                                            //8                                             
                };


        Main_STACK.DataContext = this;

        }

        public List<Border> Indicators = new List<Border>();

        public void AddIndicator()
        {

            Border Indicator_BD = new Border();
            Indicator_BD.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            Indicator_BD.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Indicator_BD.CornerRadius = new CornerRadius(2);
            Indicator_BD.BorderThickness = new Thickness(1);
            Indicator_BD.Margin = new Thickness(2, 0, 2, 0);

            Color Border_Color = (Color)ColorConverter.ConvertFromString("#FFC8C8C8");
            Indicator_BD.BorderBrush = new SolidColorBrush(Border_Color);

            Indicator_BD.Width = this.MinHeight * 0.25;

            Indicator_BD.Height = this.MinHeight * 0.25;

            Indicator_BD.Height += Indicators.Count * (this.MinHeight * 0.25);

            Indicators.Add(Indicator_BD);
            Main_STACK.Children.Add(Indicator_BD);

        }

        private int[] GetIndicatorColorSequence(int IndicatorCount)
        {

            int[] Result = null;

            switch (IndicatorCount)
            {
                case 1: Result = new int[] { 8 }; break;
                case 2: Result = new int[] { 0, 8 }; break;
                case 3: Result = new int[] { 0, 4, 8 }; break;
                case 4: Result = new int[] { 0, 2, 4, 8 }; break;
                case 5: Result = new int[] { 0, 2, 4, 6, 8 }; break;
                case 6: Result = new int[] { 0, 1, 2, 4, 6, 8 }; break;
                case 7: Result = new int[] { 0, 1, 2, 4, 6, 7, 8 }; break;
                case 8: Result = new int[] { 0, 1, 2, 3, 4, 5, 6, 8 }; break;
                case 9: Result = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }; break;
            }

            return Result;

        }

        #region "Production Level Count"

        public int ProductionLevelCount
            {
            get { return (int)GetValue(ProductionLevelCountProperty); }
            set { SetValue(ProductionLevelCountProperty, value); }
            }

        public static readonly DependencyProperty ProductionLevelCountProperty =
            DependencyProperty.Register("ProductionLevelCount", typeof(int), typeof(ProductionIndicator), new PropertyMetadata(0, new PropertyChangedCallback(ProductionLevelCountPropertyChanged)));

        private static void ProductionLevelCountPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
            {
            ProductionIndicator PI = (ProductionIndicator)dependencyObject;
            PI.InitializeIndicators();
            }

        void InitializeIndicators()
            {

            Indicators.Clear();
            Main_STACK.Children.Clear();
            IndicatorColors.Clear();

            for (int x = 1; x <= ProductionLevelCount - 1; x++)
                AddIndicator();

            int[] ColorArray = GetIndicatorColorSequence(Indicators.Count);
            for (int x = 0; x <= Indicators.Count - 1; x++)
                {
                IndicatorColors.Add(BackColors[ColorArray[x]]);
                }

            }

        #endregion

        #region "Production Level"

        public int ProductionLevel
            {
            get { return (int)GetValue(ProductionLevelProperty); }
            set { SetValue(ProductionLevelProperty, value); }
            }

        public static readonly DependencyProperty ProductionLevelProperty =
            DependencyProperty.Register("ProductionLevel", typeof(int), typeof(ProductionIndicator), new PropertyMetadata(0, new PropertyChangedCallback(ProductionLevelPropertyChanged)));

        private static void ProductionLevelPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
            {
            ProductionIndicator PI = (ProductionIndicator)dependencyObject;
            PI.UpdateIndicators();
            }


        void UpdateIndicators()
            {

            //Set Backgrounds up to Current Production Level
            for (int x = 0; x <= ProductionLevel - 1; x++)
                {
                Color BackColor = new Color();
                BackColor = IndicatorColors[ProductionLevel - 1];

                Color LightColor = BackColor;
                LightColor.A -= 63;

                LinearGradientBrush Grad = new LinearGradientBrush();
                Grad.StartPoint = new Point(0.5, 0);
                Grad.EndPoint = new Point(0.5, 1);
                Grad.GradientStops.Add(new GradientStop(LightColor, 0.0));
                Grad.GradientStops.Add(new GradientStop(BackColor, 0.5));
                Grad.GradientStops.Add(new GradientStop(LightColor, 1.0));

                Indicators[x].Background = Grad;

                }

            // Set the rest to Null (if any are left)
            for (int x = ProductionLevel; x <= Indicators.Count - 1; x++)
                {
                Indicators[x].Background = null;
                }

            }

        #endregion
  
    }
}
