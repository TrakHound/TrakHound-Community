using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using TrakHound.Server.Plugins.Shifts;

namespace TrakHound_Device_Manager.Pages.Shifts.Controls
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

                if (EndTime == null || selectedTime > EndTime)
                {
                    int i = combo.SelectedIndex;
                    if (i < combo.Items.Count - 1) EndTime = (ShiftTime)combo.Items[i + 1];
                    else if (i < combo.Items.Count) EndTime = (ShiftTime)combo.Items[i];
                }
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

        private void RemoveBreak_Clicked(TrakHound_UI.Button bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
        }

        #endregion

    }
}
