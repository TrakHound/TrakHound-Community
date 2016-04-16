using System;
using System.Windows;

using System.Windows.Media.Animation;

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

        double lastHeight = 400;

        public static readonly DependencyProperty DevConsole_ShownProperty =
            DependencyProperty.Register("DevConsole_Shown", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public GridLength DeveloperConsoleHeight
        {
            get { return (GridLength)GetValue(DeveloperConsoleHeightProperty); }
            set
            {
                SetValue(DeveloperConsoleHeightProperty, value);
            }
        }

        public static readonly DependencyProperty DeveloperConsoleHeightProperty =
            DependencyProperty.Register("DeveloperConsoleHeight", typeof(GridLength), typeof(MainWindow), new PropertyMetadata(new GridLength(0)));


        private void DeveloperConsole_ToolBarItem_Clicked(TH_WPF.Button bt)
        {
            developerConsole.Shown = !developerConsole.Shown;
        }

        private void developerConsole_ShownChanged(bool shown)
        {
            DevConsole_Shown = shown;

            if (shown)
            {
                developerConsole.ScrollLastIntoView();

                DeveloperConsoleHeight = new GridLength(lastHeight);
            }
            else
            {
                lastHeight = DeveloperConsoleHeight.Value;
                DeveloperConsoleHeight = new GridLength(0);
            }  
        }

        public const string CLIENT_NAME = "TrakHound-Client";
        public const string SERVER_NAME = "TrakHound-Server";

        void Log_Initialize()
        {
            Logger.AppicationName = CLIENT_NAME;

            var clientReader = new Logger.LogReader(CLIENT_NAME, DateTime.Now);
            clientReader.LineAdded += ClientReader_LineAdded;

            var serverReader = new Logger.LogReader(SERVER_NAME, DateTime.Now);
            serverReader.LineAdded += ServerReader_LineAdded;
        }

        private void ClientReader_LineAdded(Logger.Line line)
        {
            Dispatcher.BeginInvoke(new Action<Logger.Line, string>(Log_Updated_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { line, CLIENT_NAME });
        }

        private void ServerReader_LineAdded(Logger.Line line)
        {
            Dispatcher.BeginInvoke(new Action<Logger.Line, string>(Log_Updated_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { line, SERVER_NAME });
        }

        void Log_Updated_GUI(Logger.Line line, string applicationName)
        {
            if (developerConsole != null) developerConsole.AddLine(line, applicationName);
        }

    }
}
