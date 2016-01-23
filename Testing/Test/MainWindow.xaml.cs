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

using TH_Configuration;
using TH_Configuration.Converter_Sub_Classes;

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


            //Image = new BitmapImage(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/5/53/Google_%22G%22_Logo.svg/1024px-Google_%22G%22_Logo.svg.png"));

            //Image = new BitmapImage(new Uri("https://www.overkillshop.com/media/wysiwyg/Logos/NIKE_NSW_BRAND_LOGOwhite.png"));

            Image = new BitmapImage(new Uri("http://www.iconsdb.com/icons/preview/color/EEEEEE/warning-33-xxl.png"));

            



            //levels_LI.TotalLevelCount = 4;
            //levels_LI.ActiveLevelCount = 1;

            //test_TIMER = new System.Timers.Timer();
            //test_TIMER.Interval = 100;
            //test_TIMER.Elapsed += test_TIMER_Elapsed;
            //test_TIMER.Enabled = true;
        }



        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(MainWindow), new PropertyMetadata(null));

        

        void test_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(UpdateProgress), System.Windows.Threading.DispatcherPriority.Background);
        }

        void UpdateProgress()
        {
            ProgressValue += 1d;
            if (ProgressValue > ProgressMaximum)
            {
                ProgressMaximum = ProgressMaximum * 10;
            }
        }

        System.Timers.Timer test_TIMER = new System.Timers.Timer();




        public double ProgressValue
        {
            get { return (double)GetValue(ProgressValueProperty); }
            set { SetValue(ProgressValueProperty, value); }
        }

        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(double), typeof(MainWindow), new PropertyMetadata(0d));




        public double ProgressMaximum
        {
            get { return (double)GetValue(ProgressMaximumProperty); }
            set { SetValue(ProgressMaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressMaximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressMaximumProperty =
            DependencyProperty.Register("ProgressMaximum", typeof(double), typeof(MainWindow), new PropertyMetadata(100d));

        


        private void ListButton_Selected(TH_WPF.ListButton LB)
        {
            //TH_WPF.MessageBox.Show("Test Box", "TEST TITLE", TH_WPF.MessageBoxButtons.YesNo);

            Configuration config = Configuration.CreateBlank();

           ObjectToXml.Create(config);
        }
    }
}
