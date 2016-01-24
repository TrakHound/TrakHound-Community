using System;
using System.Windows;

using TH_Global;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        public bool DevConsole_Shown
        {
            get { return (bool)GetValue(DevConsole_ShownProperty); }
            set { SetValue(DevConsole_ShownProperty, value); }
        }

        public static readonly DependencyProperty DevConsole_ShownProperty =
            DependencyProperty.Register("DevConsole_Shown", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        private void DeveloperConsole_ToolBarItem_Clicked(TH_WPF.Button bt)
        {
            developerConsole.Shown = !developerConsole.Shown;
        }

        private void developerConsole_ShownChanged(bool shown)
        {
            DevConsole_Shown = shown;
        }

        void Log_Initialize()
        {
            LogWriter logWriter = new LogWriter();
            logWriter.Updated += Log_Updated;
            Console.SetOut(logWriter);
        }

        void Log_Updated(string newline)
        {
            this.Dispatcher.BeginInvoke(new Action<string>(Log_Updated_GUI), Priority, new object[] { newline });
        }

        void Log_Updated_GUI(string newline)
        {
            developerConsole.AddLine(newline);
        }

    }
}
