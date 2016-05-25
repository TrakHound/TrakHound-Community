// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using TH_Global;
using TH_Global.Functions;
using TH_Global.TrakHound.Users;

namespace TrakHound_Server_Controller
{
    public class Controller : ApplicationContext
    {
        private SynchronizationContext uiThreadContext;

        public Controller()
        {
            uiThreadContext = new WindowsFormsSynchronizationContext();

            CreateMenu();

            uiThreadContext.Post(new SendOrPostCallback(ReadUser), null);

            //ReadUser();

            StartLoginFileMonitor();
        }


        #region "Menu"

        private System.ComponentModel.IContainer mComponents;   //List of components
        private NotifyIcon mNotifyIcon;
        private ContextMenuStrip mContextMenu;

        private ToolStripMenuItem mLogin;
        private ToolStripMenuItem mLogout;

        private ToolStripMenuItem mMobileId;

        private ToolStripMenuItem mStartService;
        private ToolStripMenuItem mStopService;

        private Label mUsernameLBL;
        private ToolStripControlHost mLoggedIn;
        ToolStripSeparator seperator1;


        void mLogin_Click(object sender, EventArgs e) { Login(); }

        void mLogout_Click(object sender, EventArgs e) { Logout(); }


        private void mMobileId_Click(object sender, EventArgs e) { ShowMobileId(); }


        void mStartService_Click(object sender, EventArgs e) { }

        void mStopService_Click(object sender, EventArgs e) { }


        void CreateMenu()
        {
            mComponents = new System.ComponentModel.Container();

            mNotifyIcon = new NotifyIcon(this.mComponents);
            mNotifyIcon.Icon = Properties.Resources.TrakHound;
            mNotifyIcon.Text = "TrakHound Server";
            mNotifyIcon.Visible = true;

            mContextMenu = new ContextMenuStrip();
            mNotifyIcon.ContextMenuStrip = mContextMenu;

            // Add Username label
            mContextMenu.Items.Add(CreateLoggedInLabel());

            seperator1 = new ToolStripSeparator();
            seperator1.Visible = false;
            mContextMenu.Items.Add(seperator1);

            mLogin = new ToolStripMenuItem();
            mLogout = new ToolStripMenuItem();
            mMobileId = new ToolStripMenuItem();
            mStartService = new ToolStripMenuItem();
            mStopService = new ToolStripMenuItem();

            mLogin.Text = "Login";
            mLogin.Click += mLogin_Click;
            mContextMenu.Items.Add(mLogin);

            mLogout.Text = "Logout";
            mLogout.Click += mLogout_Click;
            mContextMenu.Items.Add(mLogout);
            mLogout.Enabled = false;

            mMobileId.Text = "Show Mobile ID";
            mMobileId.Click += mMobileId_Click;
            mContextMenu.Items.Add(mMobileId);

            //var seperator = new ToolStripSeparator();
            //mContextMenu.Items.Add(seperator);

            //mStartService.Text = "Start Server";
            //mStartService.Click += mStartService_Click;
            //mContextMenu.Items.Add(mStartService);

            //mStopService.Text = "Stop Server";
            //mStopService.Click += mStopService_Click;
            //mContextMenu.Items.Add(mStopService);
            //mStopService.Enabled = false;
        }

        ToolStripControlHost CreateLoggedInLabel()
        {
            var mPanel = new FlowLayoutPanel();
            mPanel.FlowDirection = FlowDirection.TopDown;
            mPanel.BackColor = System.Drawing.Color.Transparent;

            var mLabel = new Label();
            mLabel.BackColor = System.Drawing.Color.Transparent;
            mLabel.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            System.Drawing.Font font1 = new System.Drawing.Font("segoe", 8);
            mLabel.Font = font1;
            mLabel.Margin = new Padding(0, 10, 0, 2);
            mLabel.Height = 15;
            mLabel.Text = "Logged in as";
            mLabel.Parent = mPanel;

            mUsernameLBL = new Label();
            mUsernameLBL.BackColor = System.Drawing.Color.Transparent;
            mUsernameLBL.ForeColor = System.Drawing.Color.FromArgb(0, 128, 255);
            System.Drawing.Font font2 = new System.Drawing.Font("segoe", 10, System.Drawing.FontStyle.Bold);
            mUsernameLBL.Font = font2;
            mUsernameLBL.Margin = new Padding(0, 0, 0, 0);
            mUsernameLBL.Text = "";
            mUsernameLBL.Parent = mPanel;

            mLoggedIn = new ToolStripControlHost(mPanel);
            mLoggedIn.Visible = false;
            return mLoggedIn;
        }

        #endregion

        #region "Login"

        private void StartLoginFileMonitor()
        {
            string dir = FileLocations.AppData;
            string filename = "nigolresu";

            var watcher = new FileSystemWatcher(dir, filename);
            watcher.Changed += UserLoginFileMonitor_Changed;
            watcher.Created += UserLoginFileMonitor_Changed;
            watcher.Deleted += UserLoginFileMonitor_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private void UserLoginFileMonitor_Changed(object sender, FileSystemEventArgs e)
        {
            uiThreadContext.Post(new SendOrPostCallback(ReadUser), null);

            //ReadUser();
        }

        private void Login()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\trakhound-server-login.exe";
            //string path = @"F:\feenux\TrakHound\TrakHound\Server\TrakHound-Server-Login\bin\Debug\trakhound-server-login.exe";
            if (File.Exists(path))
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void Logout()
        {
            UserLoginFile.Remove();

            uiThreadContext.Post(new SendOrPostCallback(ReadUser), null);

            //ReadUser();
        }

        bool firstLoad = true;

        private void ReadUser(object o)
        {
                var loginData = UserLoginFile.Read();

                //this.BeginInvoke(new MethodInvoker(delegate
                //{
                //    this.lblElapsedTime.Text = elapsedTime.ToString();

                //    if (ElapsedCounter % 2 == 0)
                //        this.lblValue.Text = "hello world";
                //    else
                //        this.lblValue.Text = "hello";
                //});

                if (loginData != null)
                {
                    mUsernameLBL.Text = String_Functions.UppercaseFirst(loginData.Username);
                    mLoggedIn.Visible = true;
                    seperator1.Visible = true;

                    mLogin.Enabled = false;
                    mLogout.Enabled = true;

                    mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    mNotifyIcon.BalloonTipTitle = "TrakHound Server";
                    mNotifyIcon.BalloonTipText = String_Functions.UppercaseFirst(loginData.Username) + " Logged in Successfully!";
                    mNotifyIcon.ShowBalloonTip(3000);
                }
                else if (!firstLoad)
                {
                    mLoggedIn.Visible = false;
                    seperator1.Visible = false;

                    mLogin.Enabled = true;
                    mLogout.Enabled = false;

                    mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    mNotifyIcon.BalloonTipTitle = "TrakHound Server";
                    mNotifyIcon.BalloonTipText = "User Logged Out";
                    mNotifyIcon.ShowBalloonTip(3000);
                }

            firstLoad = false;

        }

        //private delegate void SetControlPropertyThreadSafeDelegate(Control control, string propertyName, object propertyValue);

        //public static void SetControlPropertyThreadSafe(Control control, string propertyName, object propertyValue)
        //{
        //    if (control.InvokeRequired)
        //    {
        //        control.Invoke(new SetControlPropertyThreadSafeDelegate
        //        (SetControlPropertyThreadSafe),
        //        new object[] { control, propertyName, propertyValue });
        //    }
        //    else
        //    {
        //        control.GetType().InvokeMember(
        //            propertyName,
        //            BindingFlags.SetProperty,
        //            null,
        //            control,
        //            new object[] { propertyValue });
        //    }
        //}


        private const string LOCAL_USER_ID = "local_user_id";

        private void ShowMobileId()
        {
            string localUserId = Registry_Functions.GetValue(LOCAL_USER_ID);
            if (localUserId == null)
            {
                // Generate new random Local User ID if not already set in Registry
                // (Should only need to be set once)
                localUserId = String_Functions.RandomString(10);

                Registry_Functions.SetKey(LOCAL_USER_ID, localUserId);
            }

            MessageBox.Show("Your Mobile ID is: " + localUserId);
        }

        #endregion

        #region "Server Events"

        void s_Started()
        {
            mStartService.Enabled = false;
            mStopService.Enabled = true;

            mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            mNotifyIcon.BalloonTipTitle = "TrakHound Server";
            mNotifyIcon.BalloonTipText = "Server Started Sucessfully!";
            mNotifyIcon.ShowBalloonTip(3000);
        }

        void s_Stopped()
        {
            mStartService.Enabled = true;
            mStopService.Enabled = false;

            mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            mNotifyIcon.BalloonTipTitle = "TrakHound Server";
            mNotifyIcon.BalloonTipText = "Server Stopped";
            mNotifyIcon.ShowBalloonTip(3000);
        }

        #endregion

    }
}
