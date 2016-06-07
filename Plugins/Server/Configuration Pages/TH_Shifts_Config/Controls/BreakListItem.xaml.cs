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
    /// Interaction logic for BreakListItem.xaml
    /// </summary>
    public partial class BreakListItem : UserControl
    {
        public BreakListItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Page ParentPage;
        public Shift ParentShift;
        public Segment ParentSegment;

        #region "Properties"

        // Use seperate collections since it was producing weird behavior, one of the comboboxes would be selected sometimes and other not other times
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


        public ShiftTime BeginTime
        {
            get { return (ShiftTime)GetValue(BeginTimeProperty); }
            set { SetValue(BeginTimeProperty, value); }
        }

        public static readonly DependencyProperty BeginTimeProperty =
            DependencyProperty.Register("BeginTime", typeof(ShiftTime), typeof(BreakListItem), new PropertyMetadata(null));


        public ShiftTime EndTime
        {
            get { return (ShiftTime)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(ShiftTime), typeof(BreakListItem), new PropertyMetadata(null));

        #endregion

        public delegate void SettingChanged_Handler(string name);
        public event SettingChanged_Handler SettingChanged;

        #region "Begin Time"

        bool begintimechanged = false;

        private void BeginTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo.SelectedItem != null)
            {
                ShiftTime selectedTime = (ShiftTime)combo.SelectedItem;
                if (ParentSegment != null) ParentSegment.begintime = selectedTime.Copy();
            }

            if (begintimechanged) if (SettingChanged != null) SettingChanged("Break Begin Time");
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
                if (ParentSegment != null) ParentSegment.endtime = selectedTime.Copy();
            }

            if (endtimechanged) if (SettingChanged != null) SettingChanged("Break End Time");
            endtimechanged = true;
        }

        #endregion

        #region "Remove"

        public delegate void Clicked_Handler(BreakListItem item);
        public event Clicked_Handler RemoveClicked;

        private void RemoveBreak_Clicked(TH_WPF.Button bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
        }

        #endregion

    }
}
