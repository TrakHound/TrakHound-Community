// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Windows.Forms;

using TH_Global;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TrakHound_Server_Controller
{
    public class Controller : ApplicationContext
    {
        public Controller()
        {
            CreateMenu();

            ReadUser();

            StartLoginFileMonitor();
        }


        #region "Menu"

        private System.ComponentModel.IContainer mComponents;   //List of components
        private NotifyIcon mNotifyIcon;
        private ContextMenuStrip mContextMenu;

        private ToolStripMenuItem mLogin;
        private ToolStripMenuItem mLogout;

        private ToolStripMenuItem mStartService;
        private ToolStripMenuItem mStopService;

        private Label mUsernameLBL;
        private ToolStripControlHost mLoggedIn;
        ToolStripSeparator seperator1;


        void mLogin_Click(object sender, EventArgs e) { Login(); }

        void mLogout_Click(object sender, EventArgs e) { Logout(); }

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
            mStartService = new ToolStripMenuItem();
            mStopService = new ToolStripMenuItem();

            mLogin.Text = "Login";
            mLogin.Click += mLogin_Click;
            mContextMenu.Items.Add(mLogin);

            mLogout.Text = "Logout";
            mLogout.Click += mLogout_Click;
            mContextMenu.Items.Add(mLogout);
            mLogout.Enabled = false;

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
            string path = FileLocations.AppData + @"\nigolresu";
            if (File.Exists(path))
            {
                string dir = Path.GetDirectoryName(path);

                var watcher = new FileSystemWatcher(dir);
                watcher.Changed += FileSystemWatcher_UserLogin_Changed;
                watcher.Created += FileSystemWatcher_UserLogin_Changed;
                watcher.Deleted += FileSystemWatcher_UserLogin_Changed;
                watcher.EnableRaisingEvents = true;
            }
        }

        private void FileSystemWatcher_UserLogin_Changed(object sender, FileSystemEventArgs e)
        {
            ReadUser();
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

            ReadUser();
        }

        bool firstLoad = true;

        private void ReadUser()
        {
            UserLoginFile.LoginData loginData = UserLoginFile.Read();

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
