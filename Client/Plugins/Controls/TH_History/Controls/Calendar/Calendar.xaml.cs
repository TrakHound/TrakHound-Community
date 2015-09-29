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

using System.Globalization;

using System.Collections.ObjectModel;

namespace TH_History.Controls.Calendar
{
    /// <summary>
    /// Interaction logic for Calendar.xaml
    /// </summary>
    public partial class Calendar : UserControl
    {
        public Calendar()
        {
            InitializeComponent();
            DataContext = this;

            LoadCalender(DateTime.Now);
        }

        void LoadCalender(DateTime date)
        {
            AddDayOfWeekTitles();

            // Set Name of Month for Calendar Title
            MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month);

            // Year
            Year = date.Year.ToString();

            Days_UGRID.Children.Clear();

            DateTime prevMonth = GetPreviousMonth(date);
            DateTime nextMonth = GetNextMonth(date);

            int month = date.Month;
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

            DateTime firstDay = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            DayOfWeek firstDayOfWeek = firstDay.DayOfWeek;

            DateTime lastDay = new DateTime(date.Year, date.Month, daysInMonth, 0, 0, 0);
            DayOfWeek lastDayOfWeek = lastDay.DayOfWeek;

            // Add days from previous month
            if (firstDayOfWeek != DayOfWeek.Sunday) AddPreviousMonthDays(prevMonth);

            for (int x = 1; x <= daysInMonth; x++)
            {
                Day day = new Day(x);
                Days_UGRID.Children.Add(day);
            }

            // Add days from next month
            if (lastDayOfWeek != DayOfWeek.Saturday) AddNextMonthDays(nextMonth);
        }

        static DateTime GetPreviousMonth(DateTime date)
        {
            if (date.Month > 1) return new DateTime(date.Year, date.Month - 1, date.Day);
            else return new DateTime(date.Year - 1, 12, date.Day);
        }

        static DateTime GetNextMonth(DateTime date)
        {
            if (date.Month < 12) return new DateTime(date.Year, date.Month + 1, date.Day);
            else return new DateTime(date.Year + 1, 1, date.Day);
        }

        void AddPreviousMonthDays(DateTime prevMonth)
        {
            int daysInPreviousMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

            DayOfWeek dow = new DateTime(prevMonth.Year, prevMonth.Month, daysInPreviousMonth).DayOfWeek;

            List<int> days = new List<int>();

            int d = daysInPreviousMonth;
            while (dow != DayOfWeek.Sunday)
            {
                dow = new DateTime(prevMonth.Year, prevMonth.Month, d).DayOfWeek;
                days.Add(d);
                d -= 1;
            }

            days.Sort();
            foreach (int i in days)
            {
                Day day = new Day(i);
                day.NotCurrentMonth = true;
                Days_UGRID.Children.Add(day);
            }
        }

        void AddNextMonthDays(DateTime nextMonth)
        {
            DayOfWeek dow = new DateTime(nextMonth.Year, nextMonth.Month, 1).DayOfWeek;

            List<int> days = new List<int>();

            int d = 1;
            while (dow != DayOfWeek.Saturday)
            {
                dow = new DateTime(nextMonth.Year, nextMonth.Month, d).DayOfWeek;
                days.Add(d);
                d += 1;
            }

            days.Sort();
            foreach (int i in days)
            {
                Day day = new Day(i);
                day.NotCurrentMonth = true;
                Days_UGRID.Children.Add(day);
            }
        }

        void AddDayOfWeekTitles()
        {
            DayOfWeek_UGRID.Children.Add(new Controls.Calendar.DayOfWeekTitle("Sunday"));
            DayOfWeek_UGRID.Children.Add(new Controls.Calendar.DayOfWeekTitle("Monday"));
            DayOfWeek_UGRID.Children.Add(new Controls.Calendar.DayOfWeekTitle("Tuesday"));
            DayOfWeek_UGRID.Children.Add(new Controls.Calendar.DayOfWeekTitle("Wednesday"));
            DayOfWeek_UGRID.Children.Add(new Controls.Calendar.DayOfWeekTitle("Thursday"));
            DayOfWeek_UGRID.Children.Add(new Controls.Calendar.DayOfWeekTitle("Friday"));
            DayOfWeek_UGRID.Children.Add(new Controls.Calendar.DayOfWeekTitle("Saturday"));
        }
        

        public string MonthName
        {
            get { return (string)GetValue(MonthNameProperty); }
            set { SetValue(MonthNameProperty, value); }
        }

        public static readonly DependencyProperty MonthNameProperty =
            DependencyProperty.Register("MonthName", typeof(string), typeof(Calendar), new PropertyMetadata(null));


        public string Year
        {
            get { return (string)GetValue(YearProperty); }
            set { SetValue(YearProperty, value); }
        }

        public static readonly DependencyProperty YearProperty =
            DependencyProperty.Register("Year", typeof(string), typeof(Calendar), new PropertyMetadata(null));

    }
}
