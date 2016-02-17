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

using TH_Global.Functions;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            dummy.DataContext = this;


            //TH_Global.Functions.Plugin_Functions.FindPlugins(@"C:\TrakHound\Plugins\Pages");

            var files = System.IO.Directory.GetFiles(@"C:\Trakhound\plugins\pages");

            if (files != null)
            {
                foreach (var filename in files)
                {
                    var plugins = TH_Plugins_Client.PluginTools.FindPlugins(filename);
                    foreach (var plugin in plugins)
                    {
                        Console.WriteLine(plugin.Title);
                    }
                }
            }

        }




        //public double Value
        //{
        //    get { return (double)GetValue(ValueProperty); }
        //    set { SetValue(ValueProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ValueProperty =
        //    DependencyProperty.Register("Value", typeof(double), typeof(MainWindow), new PropertyMetadata(0d));

        

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(this);

            VisualTreeHelper.HitTest(
                this,
                obj =>
                {
                    return HitTestFilterBehavior.Continue;
                },
                result => HitTestResultBehavior.Continue,
                new PointHitTestParameters(pt));
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(this);

            VisualTreeHelper.HitTest(
                this,
                obj =>
                {
                    return HitTestFilterBehavior.Continue;
                },
                new HitTestResultCallback(MyHitTestResult),
                new PointHitTestParameters(pt));
        }

        private HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {


            Console.WriteLine(result.VisualHit.GetType().ToString());


            //Set the behavior to return visuals at all z-order levels. 
            return HitTestResultBehavior.Continue;
        }



        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(MainWindow), new PropertyMetadata(0d));



        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(MainWindow), new PropertyMetadata(1d));


        const double incr = 0.05;

        private void Button_Clicked(TH_WPF.Button bt)
        {
            Value += incr;
        }

        private void Button_Clicked_1(TH_WPF.Button bt)
        {
            Value -= incr;
        }

        private void Button_Clicked_2(TH_WPF.Button bt)
        {
            Maximum += incr;
        }

        private void Button_Clicked_3(TH_WPF.Button bt)
        {
            Maximum -= incr;
        }

    }
}
