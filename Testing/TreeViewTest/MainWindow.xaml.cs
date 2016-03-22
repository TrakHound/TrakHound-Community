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
using System.Collections.ObjectModel;



namespace TreeViewTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var events = new List<Event>();

            events.Add(CreateProductionStatus());
          
            trvFamilies.ItemsSource = events;
        }

        Event CreateProductionStatus()
        {
            var e = new Event();
            e.Name = "Production Status";

            e.Children.Add(CreateFullProduction());
            e.Children.Add(CreateIdle());

            var d = new Result();
            d.Numval = 0;
            d.Value = "Alert";

            e.Children.Add(d);

            return e;
        }

        Value CreateFullProduction()
        {
            var avail = new Trigger();
            avail.Link = "avail";
            avail.Modifier = "Equal To";
            avail.Numval = 0;
            avail.Value = "AVAILABLE";

            var mode = new Trigger();
            mode.Link = "mode";
            mode.Modifier = "Equal To";
            mode.Numval = 1;
            mode.Value = "AUTOMATIC";

            var estop = new Trigger();
            estop.Link = "estop";
            estop.Modifier = "Equal To";
            estop.Numval = 2;
            estop.Value = "ARMED";

            var result = new Result();
            result.Value = "Full Production";
            result.Numval = 2;

            var value = new Value();
            value.Triggers.Add(avail);
            value.Triggers.Add(estop);
            value.Triggers.Add(mode);

            value.Result = result;

            return value;
        }

        Value CreateIdle()
        {
            var avail = new Trigger();
            avail.Link = "avail";
            avail.Modifier = "Equal To";
            avail.Numval = 0;
            avail.Value = "AVAILABLE";

            var mode = new Trigger();
            mode.Link = "mode";
            mode.Modifier = "Not Equal To";
            mode.Numval = 1;
            mode.Value = "AUTOMATIC";

            var estop = new Trigger();
            estop.Link = "estop";
            estop.Modifier = "Equal To";
            estop.Numval = 2;
            estop.Value = "ARMED";

            var result = new Result();
            result.Value = "Idle";
            result.Numval = 1;

            var value = new Value();
            value.Triggers.Add(avail);
            value.Triggers.Add(estop);
            value.Triggers.Add(mode);

            value.Result = result;

            return value;
        }

        
    }

    public class Event
    {
        public Event()
        {
            Children = new ObservableCollection<object>();
        }

        public ObservableCollection<object> Children { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }

        public Result Default { get; set; }

        public string Description { get; set; }



        //public override string ToString()
        //{
        //    return String_Functions.UppercaseFirst(name.Replace('_', ' '));
        //}
    }

    public class Value
    {
        public Value()
        {
            Triggers = new ObservableCollection<Trigger>();
        }

        public ObservableCollection<Trigger> Triggers { get; set; }

        //public Group Group { get; set; }

        public int Id { get; set; }

        public Result Result { get; set; }
    }

    //public class Group
    //{
    //    public Group()
    //    {
    //        Children = new ObservableCollection<object>();
    //    }

    //    public ObservableCollection<object> Children { get; set; }
    //}

    public class Trigger
    {
        public int Id { get; set; }
        public int Numval { get; set; }

        public string Value { get; set; }
        public string Link { get; set; }
        public string Modifier { get; set; }
    }

    public class Result
    {
        public int Numval { get; set; }
        public string Value { get; set; }
    }

    public class CaptureItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
    }

}
