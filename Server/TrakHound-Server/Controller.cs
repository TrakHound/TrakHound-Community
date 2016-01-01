using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;

using TH_Configuration;
using TH_Global;
using TH_UserManagement.Management;

using TrakHound_Server_Core;

namespace TrakHound_Server
{
    class Controller : ApplicationContext
    {
        private System.ComponentModel.IContainer mComponents;   //List of components
        private NotifyIcon mNotifyIcon;
        private ContextMenuStrip mContextMenu;

        private ToolStripMenuItem mLogin;
        private ToolStripMenuItem mLogout;

        private ToolStripMenuItem mControlPanel;
        private ToolStripMenuItem mStartService;
        private ToolStripMenuItem mStopService;
        private ToolStripMenuItem mExitApplication;

        private Label mUsernameLBL;
        private ToolStripControlHost mLoggedIn;

        public Database_Settings userDatabaseSettings;

        private Server server;
        private Control_Panel controlPanel;

        public Controller(Server s, Control_Panel cp)
        {
            InitializeComponents();

            controlPanel = cp;

            s.CurrentUserChanged += s_CurrentUserChanged;
            s.Started += s_Started;
            s.Stopped += s_Stopped;

            server = s;

        }

        void InitializeComponents()
        {
            mComponents = new System.ComponentModel.Container();

            mNotifyIcon = new NotifyIcon(this.mComponents);
            mNotifyIcon.Icon = Properties.Resources.TrakHound_Logo_Initials_10;
            mNotifyIcon.Text = "TrakHound Server";
            mNotifyIcon.Visible = true;

            mContextMenu = new ContextMenuStrip();
            mNotifyIcon.ContextMenuStrip = mContextMenu;

            // Add Username label
            mContextMenu.Items.Add(CreateLoggedInLabel());

            ToolStripSeparator seperator1 = new System.Windows.Forms.ToolStripSeparator();
            seperator1.Visible = false;
            mContextMenu.Items.Add(seperator1);

            mLogin = new ToolStripMenuItem();
            mLogout = new ToolStripMenuItem();

            mControlPanel = new ToolStripMenuItem();
            mStartService = new ToolStripMenuItem();
            mStopService = new ToolStripMenuItem();
            mExitApplication = new ToolStripMenuItem();

            mLogin.Text = "Login";
            mLogin.Click += mLogin_Click;
            mContextMenu.Items.Add(mLogin);

            mLogout.Text = "Logout";
            mLogout.Click += mSignOut_Click;
            mContextMenu.Items.Add(mLogout);
            mLogout.Enabled = false;

            ToolStripSeparator seperator = new System.Windows.Forms.ToolStripSeparator();
            mContextMenu.Items.Add(seperator);

            mControlPanel.Text = "Control Panel";
            mControlPanel.Click += mControlPanel_Click;
            mContextMenu.Items.Add(mControlPanel);

            seperator = new System.Windows.Forms.ToolStripSeparator();
            mContextMenu.Items.Add(seperator);

            mStartService.Text = "Start Server";
            mStartService.Click += mStartService_Click;
            mContextMenu.Items.Add(mStartService);

            mStopService.Text = "Stop Server";
            mStopService.Click += mStopService_Click;
            mContextMenu.Items.Add(mStopService);
            mStopService.Enabled = false;

            seperator = new System.Windows.Forms.ToolStripSeparator();
            mContextMenu.Items.Add(seperator);

            mExitApplication.Text = "Exit";
            mExitApplication.Image = Properties.Resources.Power_01_Red;
            mExitApplication.Click += mExitApplication_Click;
            mContextMenu.Items.Add(mExitApplication);
        }

       ToolStripControlHost CreateLoggedInLabel()
        {
            var mPanel = new FlowLayoutPanel();
            mPanel.FlowDirection = FlowDirection.TopDown;
            mPanel.BackColor = System.Drawing.Color.Transparent;

            var mLabel = new Label();
            mLabel.BackColor = System.Drawing.Color.Transparent;
            mLabel.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            System.Drawing.Font font1 = new System.Drawing.Font("segoe", 6);
            mLabel.Font = font1;
            mLabel.Margin = new Padding(0, 10, 0, 2);
            mLabel.Height = 10;
            mLabel.Text = "Logged in as";
            mLabel.Parent = mPanel;

            mUsernameLBL = new Label();
            mUsernameLBL.BackColor = System.Drawing.Color.Transparent;
            mUsernameLBL.ForeColor = System.Drawing.Color.FromArgb(0, 128, 255);
            System.Drawing.Font font2 = new System.Drawing.Font("segoe", 10, System.Drawing.FontStyle.Bold);
            mUsernameLBL.Font = font2;
            mUsernameLBL.Margin = new Padding(0, 0, 0, 0);
            mUsernameLBL.Text = "Trakhound";
            mUsernameLBL.Parent = mPanel;

            mLoggedIn = new ToolStripControlHost(mPanel);
            mLoggedIn.Visible = false;
            return mLoggedIn;
        }

       //ToolStripControlHost CreateLoggedOutLabel()
       //{
       //    var mLabel = new Label();
       //    mLabel.BackColor = System.Drawing.Color.Transparent;
       //    mLabel.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
       //    System.Drawing.Font font1 = new System.Drawing.Font("segoe", 8);
       //    mLabel.Font = font1;
       //    mLabel.Margin = new Padding(0, 10, 0, 0);
       //    mLabel.Height = 25;
       //    mLabel.Text = "Logged Out";

       //    mLoggedOut = new ToolStripControlHost(mLabel);
       //    return mLoggedOut;
       //}

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

        void s_CurrentUserChanged(TH_UserManagement.Management.UserConfiguration userConfig)
        {
            if (userConfig != null)
            {
                mLoggedIn.Visible = true;

                mLogin.Enabled = false;
                mLogout.Enabled = true;

                mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                mNotifyIcon.BalloonTipTitle = "TrakHound Server";
                mNotifyIcon.BalloonTipText = Formatting.UppercaseFirst(userConfig.username) + " Logged in Successfully!";
                mNotifyIcon.ShowBalloonTip(3000);
            }
            else
            {
                mLoggedIn.Visible = false;

                mLogin.Enabled = true;
                mLogout.Enabled = false;

                mNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                mNotifyIcon.BalloonTipTitle = "TrakHound Server";
                mNotifyIcon.BalloonTipText = "User Logged Out";
                mNotifyIcon.ShowBalloonTip(3000);
            }

            if (controlPanel != null) controlPanel.CurrentUser = userConfig;
        }

        #endregion

        void Login()
        {

            Login login = new Login();
            login.CurrentUserChanged += login_CurrentUserChanged;
            login.ShowDialog();

        }

        void login_CurrentUserChanged(TH_UserManagement.Management.UserConfiguration userConfig)
        {
            if (server != null) server.Login(userConfig);
        }

        void Logout()
        {
            RememberMe.Clear(RememberMeType.Server, userDatabaseSettings);

            if (server != null) server.Logout();
        }

        void OpenControlPanel()
        {
            if (controlPanel != null) { controlPanel.Show(); }
            if (controlPanel != null) controlPanel.CurrentUser = null;
        }

        void StartServer()
        {
            if (server != null) server.Start();
        }

        void StopServer()
        {
            if (server != null) server.Stop();
        }
        
        #region "Menu Options"

        void mLogin_Click(object sender, EventArgs e) { Login(); }

        void mSignOut_Click(object sender, EventArgs e) { Logout(); }

        void mControlPanel_Click(object sender, EventArgs e) { OpenControlPanel(); }

        void mStartService_Click(object sender, EventArgs e) { StartServer(); }

        void mStopService_Click(object sender, EventArgs e) { StopServer(); }


        void mExitApplication_Click(object sender, EventArgs e)
        {
            if (server != null) server.Stop();

            ExitThreadCore();
        }

        protected override void ExitThreadCore()
        {
            base.ExitThreadCore();
        }

        #endregion

    }
}
