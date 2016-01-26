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
            DataContext = this;
        }

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

    }
}
