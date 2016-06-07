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

using TH_Shifts;

namespace TH_Shifts_Config.Controls
{
    /// <summary>
    /// Interaction logic for ShiftListItem.xaml
    /// </summary>
    public partial class ShiftListItem : UserControl
    {
        public ShiftListItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Page ParentPage;
        public Shift ParentShift;

        #region "Properties"

        public string ShiftName
        {
            get { return (string)GetValue(ShiftNameProperty); }
            set { SetValue(ShiftNameProperty, value); }
        }

        public static readonly DependencyProperty ShiftNameProperty =
            DependencyProperty.Register("ShiftName", typeof(string), typeof(ShiftListItem), new PropertyMetadata(null));


        public ShiftTime BeginTime
        {
            get { return (ShiftTime)GetValue(BeginTimeProperty); }
            set { SetValue(BeginTimeProperty, value); }
        }

        public static readonly DependencyProperty BeginTimeProperty =
            DependencyProperty.Register("BeginTime", typeof(ShiftTime), typeof(ShiftListItem), new PropertyMetadata(null));


        public ShiftTime EndTime
        {
            get { return (ShiftTime)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(ShiftTime), typeof(ShiftListItem), new PropertyMetadata(null));

        // Use seperate collections since it was producing weird behavior, one of the comboboxes would be selected sometimes and not other times
        ObservableCollection<ShiftTime> beginhouritems;
        public ObservableCollection<ShiftTime> BeginHourItems
        {
            get
            {
                if (beginhouritems == null)
                    beginhouritems = new ObservableCollection<ShiftTime>();
                return beginhouritems;
            }

            set
            {
                beginhouritems = value;
            }
        }

        ObservableCollection<ShiftTime> endhouritems;
        public ObservableCollection<ShiftTime> EndHourItems
        {
            get
            {
                if (endhouritems == null)
                    endhouritems = new ObservableCollection<ShiftTime>();
                return endhouritems;
            }

            set
            {
                endhouritems = value;
            }
        }

        #endregion

        public delegate void SettingChanged_Handler(string name);
        public event SettingChanged_Handler SettingChanged;

        #region "Shift Name"

        bool namechanged = false;

        private void shiftname_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ParentShift != null) ParentShift.name = ((TextBox)sender).Text;

            if (namechanged) if (SettingChanged != null) SettingChanged("Shift Name");
            namechanged = true;
        }

        private void EditShiftName_Clicked(TH_WPF.Button bt)
        {

        }

        #endregion

        #region "Begin Time"

        bool begintimechanged = false;

        private void BeginTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo.SelectedItem != null)
            {
                ShiftTime selectedTime = (ShiftTime)combo.SelectedItem;
                if (ParentShift != null) ParentShift.begintime = selectedTime;
            }

            if (begintimechanged) if (SettingChanged != null) SettingChanged("Shift Begin Time");
            begintimechanged = true;
        }

        #endregion

        #region "End Time"

        bool endtimechanged = false;

        private void EndTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo.SelectedItem != null)
            {
                ShiftTime selectedTime = (ShiftTime)combo.SelectedItem;
                if (ParentShift != null) ParentShift.endtime = selectedTime;
            }

            if (endtimechanged) if (SettingChanged != null) SettingChanged("Shift End Time");
            endtimechanged = true;
        }

        #endregion

        #region "Remove"

        public delegate void Clicked_Handler(ShiftListItem item);
        public event Clicked_Handler RemoveClicked;

        private void RemoveShift_Clicked(TH_WPF.Button bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
        }

        #endregion

        #region "Break Times"

        ObservableCollection<Controls.BreakListItem> breakitems;
        public ObservableCollection<Controls.BreakListItem> BreakItems
        {
            get
            {
                if (breakitems == null)
                    breakitems = new ObservableCollection<Controls.BreakListItem>();
                return breakitems;
            }

            set
            {
                breakitems = value;
            }
        }

        void AddBreakTime()
        {
            if (ParentShift != null)
            {
                Segment b = new Segment();

                ParentShift.Breaks.Add(b);

                BreakListItem bli = new BreakListItem();
                bli.ParentPage = ParentPage;
                bli.ParentShift = ParentShift;
                bli.ParentSegment = b;

                bli.BeginHourItems = new ObservableCollection<ShiftTime>();
                foreach (ShiftTime time in BeginHourItems) bli.BeginHourItems.Add(time.Copy());

                bli.EndHourItems = new ObservableCollection<ShiftTime>();
                foreach (ShiftTime time in EndHourItems) bli.EndHourItems.Add(time.Copy());

                bli.SettingChanged += bli_SettingChanged;
                bli.RemoveClicked += bli_RemoveClicked;

                BreakItems.Add(bli);

                if (SettingChanged != null) SettingChanged("Break Added");
            }
        }

        void bli_SettingChanged(string name)
        {
            if (SettingChanged != null) SettingChanged(name);
        }

        public delegate void BreakClicked_Handler(BreakListItem item);
        public event BreakClicked_Handler BreakRemoveClicked;

        void bli_RemoveClicked(BreakListItem item)
        {
            if (BreakRemoveClicked != null) BreakRemoveClicked(item);
        }

        private void AddBreak_Clicked(TH_WPF.Button bt)
        {
            AddBreakTime();
        }

        #endregion

    }
}
