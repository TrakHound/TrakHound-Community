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

using WinInterop = System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Interop;

using System.Threading;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using System.IO;
using System.Collections.ObjectModel;
using System.Xml;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_DeviceManager;
using TH_Global;
using TH_PlugIns_Server;
using TH_WPF;
using TH_UserManagement;

namespace TrakHound_Server_Control_Panel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            devicemanager = new DeviceManager();

            InitializeComponent();
            DataContext = this;

            this.SourceInitialized += new EventHandler(win_SourceInitialized);

            Application.Current.MainWindow = this;

            // Set border thickness (maybe make this a static resource in XAML?)
            ResizeBorderThickness = 2;

            CurrentPage = devicemanager;

            // Read Users and Login
            ReadUserManagementSettings();

            devicemanager.userDatabaseSettings = userDatabaseSettings;

            LoginMenu.LoadRememberMe(Management.RememberMeType.Server);
        }

        DeviceManager devicemanager;

        #region "Main Window"

        #region "Window Controls"

        public bool Maximized
        {
            get { return (bool)GetValue(MaximizedProperty); }
            set { SetValue(MaximizedProperty, value); }
        }

        public static readonly DependencyProperty MaximizedProperty =
            DependencyProperty.Register("Maximized", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));


        private void Close_BD_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) Application.Current.Shutdown();
        }

        private void Maximize_BD_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) AdjustWindowSize();
        }

        private void Minimize_BD_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.WindowState = WindowState.Minimized;
        }

        #endregion

        private void AdjustWindowSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                Maximized = false;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                Maximized = true;
            }
        }

        #region "Dragging"

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
        }

        #endregion

        #region "Resizing"

        public int ResizeBorderThickness
        {
            get { return (int)GetValue(ResizeBorderThicknessProperty); }
            set { SetValue(ResizeBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty ResizeBorderThicknessProperty =
            DependencyProperty.Register("ResizeBorderThickness", typeof(int), typeof(MainWindow), new PropertyMetadata(2));


        private void Vertical_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNS;
        }

        private void Vertical_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private bool _isResizing = false;
        private const double CURSOR_OFFSET_SMALL = 3;
        private const double CURSOR_OFFSET_LARGE = 5;

        private void Resize_Begin(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle)
            {
                _isResizing = true;
                ((System.Windows.Shapes.Rectangle)sender).CaptureMouse();
            }
        }

        private void Resize_End(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle)
            {
                _isResizing = false;
                ((System.Windows.Shapes.Rectangle)sender).ReleaseMouseCapture();
            }
        }

        private void Resize(object sender, MouseEventArgs e)
        {
            if (_isResizing && (sender is System.Windows.Shapes.Rectangle))
            {
                double x = e.GetPosition(this).X;
                double y = e.GetPosition(this).Y;

                string mode = ((System.Windows.Shapes.Rectangle)sender).Name.ToLower();
                if (mode.Contains("left"))
                {
                    x -= CURSOR_OFFSET_SMALL;
                    if ((Width - x >= MinWidth) && (Width - x <= MaxWidth))
                    {
                        Width -= x;
                        Left += x;
                    }
                }
                if (mode.Contains("right"))
                {
                    Width = Math.Max(MinWidth, Math.Min(MaxWidth, x + CURSOR_OFFSET_LARGE));
                }
                if (mode.Contains("top"))
                {
                    y -= CURSOR_OFFSET_SMALL;
                    if ((Height - y >= MinHeight) && (Height - y <= MaxHeight))
                    {
                        Height -= y;
                        Top += y;
                    }
                }
                if (mode.Contains("bottom"))
                {
                    Height = Math.Max(MinHeight, Math.Min(MaxHeight, y + CURSOR_OFFSET_SMALL));
                }
            }
        }

        private void Resize_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        #region "Top"

        private void Rectangle_TopLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNWSE;
        }

        private void Rectangle_TopRight_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNESW;
        }

        private void Rectangle_TopMiddle_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNS;
        }

        #endregion

        #region "Bottom"

        private void Rectangle_BottomLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNESW;
        }

        private void Rectangle_BottomRight_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNWSE;
        }

        private void Rectangle_BottomMiddle_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNS;
        }

        #endregion

        private void Rectangle_WE_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeWE;
        }

        #endregion

        #region "Maximize and Taskbar Fix"

        void win_SourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        }

        private static System.IntPtr WindowProc(
              System.IntPtr hwnd,
              int msg,
              System.IntPtr wParam,
              System.IntPtr lParam,
              ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {

            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero)
            {

                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }


        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>            
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>            
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>            
            public int dwFlags = 0;
        }


        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;
            /// <summary> Win32 </summary>
            public int top;
            /// <summary> Win32 </summary>
            public int right;
            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }
            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }
            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }


        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// 
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        #endregion

        private void Main_Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            LoginMenu.Hide();
        }

        #endregion

        private void Login_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            LoginMenu.Shown = true;

            //if (sender.GetType() == typeof(Grid))
            //{
            //    Grid grid = (Grid)sender;

            //    Point point = grid.TransformToAncestor(Main_GRID).Transform(new Point(0, 0));
            //    LoginMenu.Margin = new Thickness(0, point.Y + grid.RenderSize.Height, Main_GRID.RenderSize.Width - point.X - grid.RenderSize.Width, 0);

            //    LoginMenu.Shown = true;
            //}
        }

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(MainWindow), new PropertyMetadata(null));



        #region "User Login"

        private void LoginMenu_CurrentUserChanged(TH_UserManagement.UserConfiguration userConfig)
        {
            CurrentUser = userConfig;
            //LoginMenu.Shown = true;
        }

        private void LoginMenu_ShownChanged(bool val)
        {
            //if (val)
            //{
            //    Point point = Login_GRID.TransformToAncestor(Main_GRID).Transform(new Point(0, 0));
            //    LoginMenu.Margin = new Thickness(0, point.Y + 30, Main_GRID.RenderSize.Width - point.X - Login_GRID.RenderSize.Width, 0);
            //}


            //if (val)
            //{
            //    Point point = Login_GRID.TransformToAncestor(Main_GRID).Transform(new Point(0, 0));
            //    LoginMenu.Margin = new Thickness(0, point.Y + Login_GRID.RenderSize.Height, Main_GRID.RenderSize.Width - point.X - Login_GRID.RenderSize.Width, 0);
            //}
        }

        #region "Properties"

        public string CurrentUsername
        {
            get { return (string)GetValue(CurrentUsernameProperty); }
            set { SetValue(CurrentUsernameProperty, value); }
        }

        public static readonly DependencyProperty CurrentUsernameProperty =
            DependencyProperty.Register("CurrentUsername", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public ImageSource ProfileImage
        {
            get { return (ImageSource)GetValue(ProfileImageProperty); }
            set { SetValue(ProfileImageProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageProperty =
            DependencyProperty.Register("ProfileImage", typeof(ImageSource), typeof(MainWindow), new PropertyMetadata(null));


        public bool LoggedIn
        {
            get { return (bool)GetValue(LoggedInProperty); }
            set { SetValue(LoggedInProperty, value); }
        }

        public static readonly DependencyProperty LoggedInProperty =
            DependencyProperty.Register("LoggedIn", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        #endregion

        public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        public event CurrentUserChanged_Handler CurrentUserChanged;

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                devicemanager.CurrentUser = currentuser;

                if (currentuser != null)
                {
                    CurrentUsername = TH_Global.Formatting.UppercaseFirst(currentuser.username);

                    if (devicemanager != null) devicemanager.LoadDevices();

                    //LoadDevices();

                    LoggedIn = true;
                }
                else
                {
                    LoggedIn = false;
                    CurrentUsername = null;
                }

                if (CurrentUserChanged != null) CurrentUserChanged(currentuser);
            }
        }

        public Database_Settings userDatabaseSettings;

        void ReadUserManagementSettings()
        {
            DatabasePluginReader dpr = new DatabasePluginReader();

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

            string configPath;

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            Logger.Log(configPath);

            UserManagementSettings userSettings = UserManagementSettings.ReadConfiguration(configPath);

            if (userSettings != null)
            {
                if (userSettings.Databases.Databases.Count > 0)
                {
                    userDatabaseSettings = userSettings.Databases;
                    Global.Initialize(userDatabaseSettings);
                }
            }
        }

        #endregion

        void LoginMenu_MyAccountClicked()
        {

            TH_UserManagement.MyAccount.Page page = new TH_UserManagement.MyAccount.Page();
            page.ProfileImage = LoginMenu.ProfileImage;
            page.LoadProfile(CurrentUser);

            CurrentPage = page;

        }


    }

}
