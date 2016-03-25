﻿using System;
using System.Collections.ObjectModel;
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
using System.Reflection;

using TH_Global;
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
            DataContext = this;


            //TH_Global.Functions.Plugin_Functions.FindPlugins(@"C:\TrakHound\Plugins\Pages");

            //var files = System.IO.Directory.GetFiles(@"C:\Trakhound\plugins\pages");

            //if (files != null)
            //{
            //    foreach (var filename in files)
            //    {
            //        var plugins = TH_Plugins_Client.PluginTools.FindPlugins(filename);
            //        foreach (var plugin in plugins)
            //        {
            //            Console.WriteLine(plugin.Title);
            //        }
            //    }
            //}

            //pswd.PasswordText = "%PASSWORD$";

            //ReadPlugins();

            //CreateControls(5000);

        }

        //void CreateControls(int instances)
        //{
        //    object obj = new UC();

        //    for (var x = 0; x < instances; x++) Cells.Add(CreateInstance<UC>(obj));
        //}

        //void ReadPlugins()
        //{
        //    var plugins = PluginTools.FindPlugins(FileLocations.Plugins + "\\Pages");
        //    Console.WriteLine(plugins.Count);

        //    foreach (var plugin in plugins)
        //    {
        //        for (var x = 0; x < 40; x++) Cells.Add(CreateInstance<IClientPlugin>(plugin));
        //    }
        //}

        //private ObservableCollection<object> _cells;
        ///// <summary>
        ///// Collection of Cell controls that represent each IClientPlugin
        ///// </summary>
        //public ObservableCollection<object> Cells
        //{
        //    get
        //    {
        //        if (_cells == null) _cells = new ObservableCollection<object>();
        //        return _cells;
        //    }
        //    set
        //    {
        //        _cells = value;
        //    }
        //}

        //private T CreateInstance<T>(object plugin)
        //{
        //    ConstructorInfo ctor = plugin.GetType().GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[] { }, null);

        //    Object_Functions.ObjectActivator<T> createdActivator = Object_Functions.GetActivator<T>(ctor);

        //    T result = createdActivator();

        //    return result;
        //}



        public string PasswordText
        {
            get { return (string)GetValue(PasswordTextProperty); }
            set { SetValue(PasswordTextProperty, value); }
        }

        public static readonly DependencyProperty PasswordTextProperty =
            DependencyProperty.Register("PasswordText", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        private void pswd_PasswordChanged(object sender, RoutedEventArgs e)
        {

            var txt = (PasswordBox)sender;
            PasswordText = txt.Password;
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
