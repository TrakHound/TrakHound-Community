using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;

using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Security.Principal;
using System.Security.Permissions;

using TH_Global;

namespace TrakHound_Server_Controller
{
    class Controller : ApplicationContext
    {
        const string ServiceName = ApplicationNames.TrakHoundServer_ServiceName;

        private System.ComponentModel.IContainer mComponents;   //List of components
        private NotifyIcon mNotifyIcon;
        private ContextMenuStrip mContextMenu;
        private ToolStripMenuItem mControlPanel;
        private ToolStripMenuItem mStartService;
        private ToolStripMenuItem mStopService;
        private ToolStripMenuItem mExitApplication;

        public Controller() 
        {
            InitializeComponents(); 
        }

        #region "UAC Icon"

        [DllImport("user32")]
        public static extern UInt32 SendMessage
            (IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        internal const int BCM_FIRST = 0x1600; //Normal button
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

        static internal bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static internal void AddShieldToButton(Button b)
        {
            b.FlatStyle = FlatStyle.System;
            SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
        }

        #endregion

        void InitializeComponents()
        {
            System.AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            mComponents = new System.ComponentModel.Container();

            mNotifyIcon = new NotifyIcon(this.mComponents);
            mNotifyIcon.Icon = Properties.Resources.TrakHound_Logo_Initials_10;
            mNotifyIcon.Text = "TrakHound Server Controller";
            mNotifyIcon.Visible = true;

            mContextMenu = new ContextMenuStrip();
            mControlPanel = new ToolStripMenuItem();
            mStartService = new ToolStripMenuItem();
            mStopService = new ToolStripMenuItem();
            mExitApplication = new ToolStripMenuItem();

            mNotifyIcon.ContextMenuStrip = mContextMenu;

            mControlPanel.Text = "Control Panel";
            mControlPanel.Click += mControlPanel_Click;
            mContextMenu.Items.Add(mControlPanel);

            ToolStripSeparator seperator = new System.Windows.Forms.ToolStripSeparator();
            mContextMenu.Items.Add(seperator);

            mStartService.Text = "Start Server";
            mStartService.Image = Properties.Resources.UAC_01;
            mStartService.Click += mStartService_Click;
            mContextMenu.Items.Add(mStartService);

            mStopService.Text = "Stop Server";
            mStopService.Image = Properties.Resources.UAC_01;
            mStopService.Click += mStopService_Click;
            mContextMenu.Items.Add(mStopService);

            seperator = new System.Windows.Forms.ToolStripSeparator();
            mContextMenu.Items.Add(seperator);

            mExitApplication.Text = "Exit";
            mExitApplication.Image = Properties.Resources.Power_01_Red;
            mExitApplication.Click += mExitApplication_Click;
            mContextMenu.Items.Add(mExitApplication);
        }


        void StartAdminProcess(string fileName)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = fileName;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        System.Timers.Timer StatusDelay_TIMER;

        void OpenControlPanel()
        {
            try
            {
                string filename = AppDomain.CurrentDomain.BaseDirectory + @"TrakHound-Server-Control-Panel.exe";

                Process.Start(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void StartService()
        {
            // Get filename for TH_StartService.exe
            string dir = AppDomain.CurrentDomain.BaseDirectory + @"Admin Functions\";
            //string dir = @"C:\TrakHound\Server\TrakHound-Server\Admin Functions\";
            string filename = dir + "TH_StartService.exe";

            if (File.Exists(filename))
            {
                StartAdminProcess(filename);
            }
            else Console.WriteLine(filename);

            StatusDelay_TIMER = new System.Timers.Timer();
            StatusDelay_TIMER.Interval = 3000;
            StatusDelay_TIMER.Elapsed += StartStatusDelay_TIMER_Elapsed;
            StatusDelay_TIMER.Enabled = true;
        }

        void StopService()
        {
            // Get filename for TH_StopService.exe
            string dir = AppDomain.CurrentDomain.BaseDirectory + @"Admin Functions\";
            //string dir = @"C:\TrakHound\Server\TrakHound-Server\Admin Functions\";
            string filename = dir + "TH_StopService.exe";

            if (File.Exists(filename))
            {
                StartAdminProcess(filename);
            }
            else Console.WriteLine(filename);

            StatusDelay_TIMER = new System.Timers.Timer();
            StatusDelay_TIMER.Interval = 3000;
            StatusDelay_TIMER.Elapsed += StopStatusDelay_TIMER_Elapsed;
            StatusDelay_TIMER.Enabled = true;
        }

        void StartStatusDelay_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sender.GetType() == typeof(System.Timers.Timer))
            {
                System.Timers.Timer timer = (System.Timers.Timer)sender;
                timer.Enabled = false;
            }

            // Show Status as BalloonTip
            ServiceController service = new ServiceController(ServiceName, Environment.MachineName);
            if (service != null)
            {
                if (service.Status == ServiceControllerStatus.Running)
                {
                    mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    mNotifyIcon.BalloonTipTitle = "TrakHound Server";
                    mNotifyIcon.BalloonTipText = "Successfully started";
                    mNotifyIcon.ShowBalloonTip(3000);
                }
                else
                {
                    mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    mNotifyIcon.BalloonTipTitle = "TrakHound Server";
                    mNotifyIcon.BalloonTipText = "Server did not start correctly";
                    mNotifyIcon.ShowBalloonTip(3000);
                }

                service.Close();
            }
        }

        void StopStatusDelay_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sender.GetType() == typeof(System.Timers.Timer))
            {
                System.Timers.Timer timer = (System.Timers.Timer)sender;
                timer.Enabled = false;
            }

            // Show Status as BalloonTip
            ServiceController service = new ServiceController(ServiceName, Environment.MachineName);
            if (service != null)
            {
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    mNotifyIcon.BalloonTipTitle = "TrakHound Server";
                    mNotifyIcon.BalloonTipText = "Stopped";
                    mNotifyIcon.ShowBalloonTip(3000);
                }
                else
                {
                    mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    mNotifyIcon.BalloonTipTitle = "TrakHound Server";
                    mNotifyIcon.BalloonTipText = "Server did not stop";
                    mNotifyIcon.ShowBalloonTip(3000);
                }

                service.Close();
            }
        }

        #region "Menu Options"

        void mControlPanel_Click(object sender, EventArgs e) { OpenControlPanel(); }

        void mStartService_Click(object sender, EventArgs e) { StartService(); }

        void mStopService_Click(object sender, EventArgs e) { StopService(); }


        void mExitApplication_Click(object sender, EventArgs e)
        {
            ExitThreadCore();
        }

        protected override void ExitThreadCore()
        {
            base.ExitThreadCore();
        }

        #endregion

    }

}
