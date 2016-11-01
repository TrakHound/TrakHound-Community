using System.Windows;

namespace TrakHound_UI.Windows
{
    /// <summary>
    /// Interaction logic for BugReportSent.xaml
    /// </summary>
    public partial class BugReportSent : Window
    {
        public BugReportSent()
        {
            InitializeComponent();
        }

        private void Close_Clicked(Button bt)
        {
            DialogResult = false;
        }

        private void Restart_Clicked(Button bt)
        {
            DialogResult = true;
        }
    }
}
