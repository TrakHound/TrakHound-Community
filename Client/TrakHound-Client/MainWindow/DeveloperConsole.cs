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

        //double defaultHeight = 400;
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

                //Animate(lastHeight, DeveloperConsoleHeightProperty);
            }
            else
            {
                lastHeight = DeveloperConsoleHeight.Value;
                DeveloperConsoleHeight = new GridLength(0);
                //DeveloperConsoleHeight = developerConsole.ActualHeight;
                //Animate(0, DeveloperConsoleHeightProperty);
            }  
        }

        //void Animate(double to, DependencyProperty dp)
        //{
        //    var animation = new DoubleAnimation();

        //    animation.From = ((GridLength)GetValue(dp)).Value;
        //    if (!double.IsNaN(to)) animation.To = Math.Max(0, to);
        //    else animation.To = 0;
        //    animation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
        //    this.BeginAnimation(dp, animation);
        //}

        void Log_Initialize()
        {
            LogWriter logWriter = new LogWriter();
            logWriter.Updated += Log_Updated;
            Console.SetOut(logWriter);
        }

        void Log_Updated(string newline)
        {
            this.Dispatcher.BeginInvoke(new Action<string>(Log_Updated_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { newline });
        }

        void Log_Updated_GUI(string newline)
        {
            developerConsole.AddLine(newline);
        }

    }
}
