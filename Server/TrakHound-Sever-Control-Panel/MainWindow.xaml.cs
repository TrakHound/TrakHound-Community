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

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using System.IO;
using System.Collections.ObjectModel;
using System.Xml;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_PlugIns_Server;
using TH_WPF;

namespace TrakHound_Server_Control_Panel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            this.SourceInitialized += new EventHandler(win_SourceInitialized);

            Application.Current.MainWindow = this;

            // Set border thickness (maybe make this a static resource in XAML?)
            ResizeBorderThickness = 2;

            // Read Users and Login
            ReadUserManagementSettings();

            InitializePages();

            LoadPlugins();

            LoadDevices();
        }

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

        #region "Configuration Management"

        //public delegate void SelectedDeviceChanged_Handler(Configuration config);
        //public event SelectedDeviceChanged_Handler SelectedDeviceChanged;

        //public delegate void ConfigurationTableChanged_Handler(DataTable dt);
        //public event ConfigurationTableChanged_Handler ConfigurationTableChanged;

        Configuration selecteddevice;
        public Configuration SelectedDevice
        {
            get { return selecteddevice; }
            set
            {
                selecteddevice = value;
                //if (SelectedDeviceChanged != null) SelectedDeviceChanged(selecteddevice);
            }
        }

        DataTable configurationtable;
        public DataTable ConfigurationTable
        {
            get { return configurationtable; }
            set 
            {
                configurationtable = value;

                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        page.LoadConfiguration(configurationtable);
                    }
                }
            }
        }

        public void LoadDevices()
        {
            DeviceListShown = false;
            DeviceList.Clear();

            if (currentuser != null)
            {
                if (userDatabaseSettings == null)
                {
                    Configurations = TH_Configuration.User.Management.GetConfigurationsForUser(currentuser);
                }
                else
                {
                    Configurations = TH_Database.Tables.Users.GetConfigurationsForUser(currentuser, userDatabaseSettings);
                }
            }
            // If not logged in Read from File in 'C:\TrakHound\'
            else
            {
                Configurations = ReadConfigurationFile();
            }

            if (Configurations != null)
            {
                // Create DevicesList based on Configurations
                foreach (Configuration config in Configurations)
                {
                    CreateDeviceButton(config);
                }

                DeviceListShown = true;
            }
        }

        #region "Configuration Files"

        List<Configuration> Configurations;

        static List<Configuration> ReadConfigurationFile()
        {
            List<Configuration> Result = new List<Configuration>();

            string configPath;

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "Configuration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "Configuration.Xml";

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            Logger.Log(configPath);

            if (System.IO.File.Exists(configPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);

                foreach (XmlNode Node in doc.DocumentElement.ChildNodes)
                {
                    if (Node.NodeType == XmlNodeType.Element)
                    {
                        switch (Node.Name.ToLower())
                        {
                            case "devices":
                                foreach (XmlNode ChildNode in Node.ChildNodes)
                                {
                                    if (ChildNode.NodeType == XmlNodeType.Element)
                                    {
                                        switch (ChildNode.Name.ToLower())
                                        {
                                            case "device":

                                                Configuration device = ProcessDevice(ChildNode);
                                                if (device != null)
                                                {
                                                    Result.Add(device);
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                Logger.Log("Configuration File Successfully Read From : " + configPath);
            }
            else Logger.Log("Configuration File Not Found : " + configPath);

            return Result;
        }

        static Configuration ProcessDevice(XmlNode node)
        {
            Configuration Result = null;

            string configPath = null;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                {
                    if (childNode.Name.ToLower() == "configuration_path")
                    {
                        configPath = childNode.InnerText;
                    }
                }
            }

            if (configPath != null)
            {
                configPath = GetConfigurationPath(configPath);

                Logger.Log("Reading Device Configuration File @ '" + configPath + "'");

                if (File.Exists(configPath))
                {
                    Configuration config = new Configuration();
                    config = Configuration.ReadConfigFile(configPath);

                    if (config != null)
                    {
                        Console.WriteLine("Device Congifuration Read Successfully!");

                        // Initialize Database Configurations
                        Global.Initialize(config.Databases);

                        Result = config;
                    }
                    else Logger.Log("Error Occurred While Reading : " + configPath);
                }
                else Logger.Log("Can't find Device Configuration file @ " + configPath);
            }
            else Logger.Log("No Device Congifuration found");

            return Result;

        }

        static string GetConfigurationPath(string path)
        {
            // If not full path, try System Dir ('C:\TrakHound\') and then local App Dir
            if (!System.IO.Path.IsPathRooted(path))
            {
                // Remove initial Backslash if contained in "configuration_path"
                if (path[0] == '\\' && path.Length > 1) path.Substring(1);

                string original = path;

                // Check System Path
                path = TH_Global.FileLocations.TrakHound + "\\Configuration Files\\" + original;
                if (File.Exists(path)) return path;
                else Logger.Log(path + " Not Found");


                // Check local app Path
                path = AppDomain.CurrentDomain.BaseDirectory + "Configuration Files\\" + original;
                if (File.Exists(path)) return path;
                else Logger.Log(path + " Not Found");

                // if no files exist return null
                return null;
            }
            else return path;
        }

        #endregion

        void LoadConfiguration()
        {
            if (ConfigurationPages != null)
            {
                foreach (ConfigurationPage page in ConfigurationPages)
                {
                    page.LoadConfiguration(configurationtable);
                }
            }
        }

        #region "Save"

        public void SaveConfiguration()
        {
            DataTable dt = ConfigurationTable;

            if (dt != null)
            {
                if (currentuser != null)
                {
                    if (SelectedDevice != null)
                    {
                        string tablename = TH_Configuration.User.Management.GetConfigurationTableName(currentuser, SelectedDevice);

                        if (userDatabaseSettings == null)
                        {
                            TH_Configuration.User.Management.UpdateConfigurationTable(currentuser, tablename, dt);
                        }
                        else
                        {
                            //TH_Database.Tables.Users.Configuration_UpdateRows(currentuser, userDatabaseSettings, SelectedDevice);
                        }
                    } 
                }
                // If not logged in Save to File in 'C:\TrakHound\'
                else
                {

                }
            }
        }

        #endregion

        #endregion

        #region "Pages"

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(MainWindow), new PropertyMetadata(null));
 

        ObservableCollection<PageItem> pagelist;
        public ObservableCollection<PageItem> PageList
        {
            get
            {
                if (pagelist == null)
                    pagelist = new ObservableCollection<PageItem>();
                return pagelist;
            }

            set
            {
                pagelist = value;
            }
        }

        List<ConfigurationPage> ConfigurationPages;

        void InitializePages()
        {
            PageListShown = false;

            PageList.Clear();

            ConfigurationPages = new List<ConfigurationPage>();

            ConfigurationPages.Add(new Pages.AgentConfiguration());


            // Create PageItem and add to PageList
            foreach (ConfigurationPage page in ConfigurationPages)
            {
                page.SettingChanged += page_SettingChanged;

                PageItem item = new PageItem();
                item.Text = page.PageName;
                item.Image = page.Image;
                item.Data = page;
                item.Clicked += PageItem_Clicked;
                PageList.Add(item);
            }

            //PageItem item = new PageItem();
            //item.Text = "Description";
            //item.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Server-Control-Panel;component/Resources/About_01.png"));
            //item.Data = new Pages.DescriptionConfiguration();
            //item.Clicked += PageItem_Clicked;
            //PageList.Add(item);

            //item = new PageItem();
            //item.Text = "Agent";
            //item.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Server-Control-Panel;component/Resources/Agent_02.png"));
            //item.Data = new Pages.AgentConfiguration();
            //item.Clicked += PageItem_Clicked;
            //PageList.Add(item);

            //item = new PageItem();
            //item.Text = "Databases";
            //item.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Server-Control-Panel;component/Resources/DatabaseConfig_01.png"));
            //PageList.Add(item);

        }



        public bool SaveNeeded
        {
            get { return (bool)GetValue(SaveNeededProperty); }
            set { SetValue(SaveNeededProperty, value); }
        }

        public static readonly DependencyProperty SaveNeededProperty =
            DependencyProperty.Register("SaveNeeded", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        

        void page_SettingChanged(string name, string oldVal, string newVal)
        {
            SaveNeeded = true;
        }

        private void Restore_Clicked(Controls.Button bt)
        {
            LoadConfiguration();
        }

        private void Save_Clicked(Controls.Button bt)
        {
            if (ConfigurationTable != null)
            {
                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        page.SaveConfiguration(configurationtable);
                    }
                }

                SaveConfiguration();
            }

            SaveNeeded = false;
        }




        void PageItem_Clicked(object data)
        {
            if (data != null)
            {
                if (CurrentPage != null)
                {
                    if (CurrentPage.GetType() != data.GetType())
                    {
                        CurrentPage = data;
                    }
                }
                else CurrentPage = data;
            }
        }

        void Description_Clicked()
        {
            if (CurrentPage != null)
            {
                if (CurrentPage.GetType() != typeof(Pages.DescriptionConfiguration))
                {
                    CurrentPage = new Pages.DescriptionConfiguration();
                }
            }
            else CurrentPage = new Pages.DescriptionConfiguration();
        }

        void Agent_Clicked()
        {
            if (CurrentPage != null)
            {
                if (CurrentPage.GetType() != typeof(Pages.AgentConfiguration))
                {
                    CurrentPage = new Pages.AgentConfiguration();
                }
            }
            else CurrentPage = new Pages.AgentConfiguration();
        }

        void AddConfigurationPage(Table_PlugIn tp)
        {
            ConfigurationPage configPage = tp.ConfigPage;

            if (configPage != null)
            {
                PageItem item = new PageItem();
                item.Text = configPage.PageName;
                PageList.Add(item);
            }
        }

        #endregion

        #region "User Login"

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

        private void Login_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Grid))
            {
                Grid grid = (Grid)sender;

                Point point = grid.TransformToAncestor(Main_GRID).Transform(new Point(0, 0));
                LoginMenu.Margin = new Thickness(0, point.Y + grid.RenderSize.Height, Main_GRID.RenderSize.Width - point.X - grid.RenderSize.Width, 0);

                LoginMenu.Shown = true;
            }
        }

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                if (currentuser != null)
                {
                    CurrentUsername = TH_Global.Formatting.UppercaseFirst(currentuser.username);

                    LoadDevices();

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

        #region "PlugIns"

        public IEnumerable<Lazy<Table_PlugIn>> TablePlugIns { get; set; }

        public List<Lazy<Table_PlugIn>> Table_Plugins { get; set; }

        TablePlugs TPLUGS;

        class TablePlugs
        {
            [ImportMany(typeof(Table_PlugIn))]
            public IEnumerable<Lazy<Table_PlugIn>> PlugIns { get; set; }
        }

        void LoadPlugins()
        {
            string plugin_rootpath = FileLocations.Plugins + @"\Server";

            if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

            Table_Plugins = new List<Lazy<Table_PlugIn>>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            pluginsPath = TH_Global.FileLocations.Plugins + @"\Server\";
            if (Directory.Exists(pluginsPath)) LoadTablePlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            if (Directory.Exists(pluginsPath)) LoadTablePlugins(pluginsPath);

            TablePlugIns = Table_Plugins;
        }

        void LoadTablePlugins(string Path)
        {
            Logger.Log("Searching for Table Plugins in '" + Path + "'");
            if (Directory.Exists(Path))
            {
                try
                {
                    TPLUGS = new TablePlugs();

                    var PageCatalog = new DirectoryCatalog(Path);
                    var PageContainer = new CompositionContainer(PageCatalog);
                    PageContainer.SatisfyImportsOnce(TPLUGS);

                    TablePlugIns = TPLUGS.PlugIns;

                    foreach (Lazy<Table_PlugIn> ltp in TablePlugIns)
                    {
                        Table_PlugIn tp = ltp.Value;

                        if (Table_Plugins.ToList().Find(x => x.Value.Name.ToLower() == tp.Name.ToLower()) == null)
                        {
                            Logger.Log(tp.Name + " : PlugIn Found");
                            Table_Plugins.Add(ltp);
                        }
                        else
                        {
                            Logger.Log(tp.Name + " : PlugIn Already Found");
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("LoadTablePlugins() : Exception : " + ex.Message); }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(Path))
                {
                    LoadTablePlugins(directory);
                }
            }
            else Logger.Log("Table PlugIns Directory Doesn't Exist (" + Path + ")");
        }

        void TablePlugIns_Initialize(Configuration config)
        {
            if (TablePlugIns != null && config != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    try
                    {
                        Table_PlugIn tp = ltp.Value;
                        tp.Initialize(config);

                        AddConfigurationPage(tp);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Plugin Exception! : " + ex.Message);
                    }
                }
            }
        }

        void TablePlugIns_Closing()
        {
            if (TablePlugIns != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    try
                    {
                        Table_PlugIn tp = ltp.Value;
                        tp.Closing();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Plugin Exception! : " + ex.Message);
                    }
                }
            }
        }

        #endregion


        public bool DeviceListShown
        {
            get { return (bool)GetValue(DeviceListShownProperty); }
            set { SetValue(DeviceListShownProperty, value); }
        }

        public static readonly DependencyProperty DeviceListShownProperty =
            DependencyProperty.Register("DeviceListShown", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        ObservableCollection<ListButton> devicelist;
        public ObservableCollection<ListButton> DeviceList
        {
            get
            {
                if (devicelist == null)
                    devicelist = new ObservableCollection<ListButton>();
                return devicelist;
            }

            set
            {
                devicelist = value;
            }
        }


        void CreateDeviceButton(Configuration config)
        {
            Controls.DeviceButton db = new Controls.DeviceButton();
            db.Description = config.Description.Description;
            db.Manufacturer = config.Description.Manufacturer;
            db.Model = config.Description.Model;
            db.Serial = config.Description.Serial;
            db.Id = config.Description.Machine_ID;

            ListButton lb = new ListButton();
            lb.ButtonContent = db;
            lb.ShowImage = false;
            lb.Selected += lb_Device_Selected;
            lb.DataObject = config;
            DeviceList.Add(lb);
        }

        void lb_Device_Selected(TH_WPF.ListButton lb)
        {
            foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;

            Configuration config = (Configuration)lb.DataObject;

            if (config != null)
            {
                SelectedDevice = config;

                ConfigurationTable = TH_Configuration.Converter.XMLToTable(config.ConfigurationXML);

                PageListShown = true;

                //LoadPageList(config);
            }
        }

        public bool PageListShown
        {
            get { return (bool)GetValue(PageListShownProperty); }
            set { SetValue(PageListShownProperty, value); }
        }

        public static readonly DependencyProperty PageListShownProperty =
            DependencyProperty.Register("PageListShown", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;


        

        private void TableList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageListShown = false;
        }

        private void AddDevice_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentPage != null)
            {
                if (CurrentPage.GetType() != typeof(Pages.AddDevice))
                {
                    CurrentPage = new Pages.AddDevice(); 
                }
            }
            else CurrentPage = new Pages.AddDevice();
        }


    }

}
