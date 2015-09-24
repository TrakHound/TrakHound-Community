// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

using System.Xml;
using System.IO;
using System.Collections.ObjectModel;

using WinInterop = System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing.Printing;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using TH_Configuration;
using TH_Device_Client;
using TH_Global;
using TH_PlugIns_Client_Control;
using TH_WPF;
using TH_Updater;

using TrakHound_Client.Controls;

namespace TrakHound_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            Log_Initialize();

            Splash_Initialize();

            InitializeComponent();
            DataContext = this;

            Splash_UpdateStatus("...Initializing");
            this.SourceInitialized += new EventHandler(win_SourceInitialized);

            Application.Current.MainWindow = this;

            // Initialize Pages
            Pages_Initialize();

            // Set border thickness (maybe make this a static resource in XAML?)
            ResizeBorderThickness = 1;

            Splash_UpdateStatus("...Reading Configuration");
            ReadConfigurationFile();

            Splash_UpdateStatus("...Loading Plugins");
            LoadPlugIns();



            // Wait for the minimum splash time to elapse, then close the splash dialog
            while (SplashWait) { System.Threading.Thread.Sleep(200); }
            Splash_Close();
        }

        #region "Splash"

        Splash.Screen SPLSH;

        System.Timers.Timer Splash_TIMER;

        void Splash_Initialize()
        {

            SPLSH = new Splash.Screen();
            Splash_Show();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            Version version = assembly.GetName().Version;

            SPLSH.Version = "Version " + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();

            Splash_TIMER = new System.Timers.Timer();
            Splash_TIMER.Interval = 4000;
            Splash_TIMER.Elapsed += Splash_TIMER_Elapsed;
            Splash_TIMER.Enabled = true;

        }

        void Splash_Show() { this.Dispatcher.Invoke(new Action(Splash_Show_GUI), new object[] { }); }

        void Splash_Show_GUI() { SPLSH.Show(); }

        void Splash_Close() { if (SPLSH != null) SPLSH.Close(); }

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.Background;

        void Splash_UpdateStatus(string Status) { this.Dispatcher.Invoke(new Action<string>(Splash_UpdateStatus_GUI), Priority, new object[] { Status }); }

        void Splash_UpdateStatus_GUI(string Status) { SPLSH.Status = Status; }

        void Splash_AddPagePlugin(Control_PlugIn CP) { this.Dispatcher.Invoke(new Action<Control_PlugIn>(Splash_AddPagePlugin_GUI), new object[] { CP }); }

        void Splash_AddPagePlugin_GUI(Control_PlugIn CP) { SPLSH.AddPagePlugin(CP); }

        void Splash_AddGlobalPlugin(Control_PlugIn CP) { this.Dispatcher.Invoke(new Action<Control_PlugIn>(Splash_AddGlobalPlugin_GUI), new object[] { CP }); }

        void Splash_AddGlobalPlugin_GUI(Control_PlugIn CP) { SPLSH.AddGlobalPlugin(CP); }

        bool SplashWait = true;

        void Splash_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Splash_TIMER.Enabled = false;
            SplashWait = false;
        }

        #endregion

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

        // Keyboard Keys
        private void Main_Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            // Always get correct key (ex. Alt)
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // Page Tabs
            if (e.Key == Key.Tab && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                ChangePage_Backward();
            }
            else if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ChangePage_Forward();
            }

            // Toggle MainMenu Bar
            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                MainMenuBar_Show = !MainMenuBar_Show;
            }
        }

        private void Main_Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            messageCenter.Hide();

            PluginLauncher.Hide();
            MainMenu.Hide();
        }


        private void Main_Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Main_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.PagePlugIn_Configurations != null)
            {
                List<PlugInConfiguration> configs = Properties.Settings.Default.PagePlugIn_Configurations.ToList();

                if (configs != null)
                {
                    foreach (PlugInConfiguration config in configs)
                    {
                        if (config.enabled && PagePlugIns != null)
                        {
                            foreach (Lazy<Control_PlugIn> LCP in PagePlugIns.ToList())
                            {
                                if (LCP != null)
                                {
                                    if (LCP.IsValueCreated)
                                    {
                                        try
                                        {
                                            Control_PlugIn CP = LCP.Value;
                                            CP.Closing();
                                        }
                                        catch (Exception ex) { }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Properties.Settings.Default.Save();

        }

        #endregion

        #region "Pages"

        ObservableCollection<TH_TabHeader_Top> pagetabheaders;
        public ObservableCollection<TH_TabHeader_Top> PageTabHeaders
        {
            get
            {
                if (pagetabheaders == null) pagetabheaders = new ObservableCollection<TH_TabHeader_Top>();
                return pagetabheaders;
            }
            set
            {
                pagetabheaders = value;
            }
        }

        public void AddPageAsTab(object page, string title, ImageSource image)
        {
            // Check to see if Page already exists
            TH_TabItem TI = Pages_TABCONTROL.Items.Cast<TH_TabItem>().ToList().Find(x => x.Title.ToString().ToLower() == title.ToLower());

            if (TI == null)
            {
                TI = new TH_TabItem();
                TI.Content = CreatePage(page); ;
                TI.Title = title;
                TI.Closed += TI_Closed;

                TH_TabHeader_Top header = new TH_TabHeader_Top();
                header.Text = title;
                header.Image = image;
                header.TabParent = TI;
                header.Clicked += header_Clicked;
                header.CloseClicked += header_CloseClicked;
                TI.TH_Header = header;
                
                int zlevel = int.MaxValue;

                // Move all of the existing tabs to the front so that the new tab is behind it (so it can "slide" in behind it)
                for (int x = 0; x <= PageTabHeaders.Count - 1; x++)
                {
                    TH_TabHeader_Top tabHeader = (TH_TabHeader_Top)PageTabHeaders[x];
                    Panel.SetZIndex(tabHeader, zlevel - x);
                }

                PageTabHeaders.Add(header);

                Panel.SetZIndex(header, -1);

                TI.Template = (ControlTemplate)TryFindResource("TabItemControlTemplate");

                Pages_TABCONTROL.Items.Add(TI);
                Pages_TABCONTROL.SelectedItem = TI;
            }
            else
            {
                Pages_TABCONTROL.SelectedItem = TI;
            }
        }

        void TI_Closed(TH_TabItem tab)
        {
            List<TH_TabHeader_Top> headers = new List<TH_TabHeader_Top>();
            headers.AddRange(PageTabHeaders);

            List<TH_TabItem> tabs = Pages_TABCONTROL.Items.OfType<TH_TabItem>().ToList();

            foreach (TH_TabHeader_Top header in headers)
            {
                if (tabs.Find(x => x.Title.ToLower() == header.Text.ToLower()) == null)
                    PageTabHeaders.Remove(header);
            }
        }

        public TH_Page CreatePage(object control)
        {
            TH_Page Result = new TH_Page();

            Result.PageContent = control;

            return Result;
        }

        void header_Clicked(TH_TabHeader_Top header)
        {
            if (header.TabParent != null) Pages_TABCONTROL.SelectedItem = header.TabParent;
        }

        void header_CloseClicked(TH_TabHeader_Top header)
        {
            int index = 0; 

            if (header.IsSelected)
            {
                if (Pages_TABCONTROL.SelectedIndex < Pages_TABCONTROL.Items.Count - 1) 
                    index = Math.Min(Pages_TABCONTROL.Items.Count, Pages_TABCONTROL.SelectedIndex + 1);
                else 
                    index = Math.Max(0, Pages_TABCONTROL.SelectedIndex - 1);

                Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[index];
            }

            if (header.TabParent != null)
            {
                header.TabParent.Close();
            }
        }

        private void Pages_TABCONTROL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() == typeof(TabControl))
            {
                TabControl tc = (TabControl)sender;

                for (int x = 0; x <= PageTabHeaders.Count - 1; x++)
                {
                    if (x != tc.SelectedIndex)
                    {
                        PageTabHeaders[x].IsSelected = false;
                    }
                    else
                    {
                        PageTabHeaders[x].IsSelected = true;
                    }

                    ZoomLevel = 1;
                }
            }
        }

        void ChangePage_Forward()
        {
            if (Pages_TABCONTROL.Items.Count > 0)
            {
                int index = Pages_TABCONTROL.SelectedIndex;
                int max = Pages_TABCONTROL.Items.Count - 1;

                if (index < max)
                {
                    Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[index + 1];
                }
                else
                {
                    Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[0];
                }
            }
        }

        void ChangePage_Backward()
        {
            if (Pages_TABCONTROL.Items.Count > 0)
            {
                int index = Pages_TABCONTROL.SelectedIndex;
                int max = Pages_TABCONTROL.Items.Count - 1;

                if (index > 0)
                {
                    Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[index - 1];
                }
                else
                {
                    Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[max];
                }
            }
        }


        #region "Zoom"

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set
            { 
                SetValue(ZoomLevelProperty, value);

                if (Pages_TABCONTROL.SelectedIndex >= 0)
                {
                    TH_TabItem tab = (TH_TabItem)Pages_TABCONTROL.Items[Pages_TABCONTROL.SelectedIndex];

                    TH_Page page = (TH_Page)tab.Content;
                    page.ZoomLevel = value;

                    ZoomLevelDisplay = value.ToString("P0");

                    if (ZoomLevelChanged != null) ZoomLevelChanged(value);
                }

            }
        }

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(MainWindow), new PropertyMetadata(1D));


        public string ZoomLevelDisplay
        {
            get { return (string)GetValue(ZoomLevelDisplayProperty); }
            set 
            { 
                SetValue(ZoomLevelDisplayProperty, value);
            }
        }

        public static readonly DependencyProperty ZoomLevelDisplayProperty =
            DependencyProperty.Register("ZoomLevelDisplay", typeof(string), typeof(MainWindow), new PropertyMetadata("100%"));

        public delegate void ZoomLevelChanged_Handler(double zoomlevel);
        public event ZoomLevelChanged_Handler ZoomLevelChanged;

        #endregion

        void Pages_Initialize()
        {
            About_Initialize();
            Options_Initialize();
            Plugins_Initialize();
        }

        #region "About"

        public About.Manager aboutManager;

        void About_Initialize()
        {
            aboutManager = new About.Manager();

            aboutManager.AddPage(new About.Pages.Information.Page());
            aboutManager.AddPage(new About.Pages.License.Page());
        }

        public void About_Open()
        {
            AddPageAsTab(aboutManager, "About", new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/About_01.png")));
        }

        #endregion

        #region "Options"

        Options.Manager optionsManager;

        void Options_Initialize()
        {
            optionsManager = new Options.Manager();

            optionsManager.AddPage(new Options.Pages.General.Page());
            optionsManager.AddPage(new Options.Pages.Updates.Page());
        }

        public void Options_Open()
        {
            AddPageAsTab(optionsManager, "Options", new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/options_gear_30px.png")));
        }

        #endregion

        #region "Plugins"

        Plugins.Manager pluginsManager;

        Plugins.Pages.Installed.Page pluginsPage;

        void Plugins_Initialize()
        {
            pluginsManager = new Plugins.Manager();

            pluginsPage = new Plugins.Pages.Installed.Page();
            pluginsManager.AddPage(pluginsPage);
        }

        public void Plugins_Open()
        {
            AddPageAsTab(pluginsManager, "Plugins", new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Rocket_02.png")));
        }

        #endregion

        #endregion

        #region "Plugin Launcher"

        private void PluginLauncher_BT_Clicked(TH_Button bt)
        {
            Point point = bt.TransformToAncestor(Main_GRID).Transform(new Point(0, 0));
            PluginLauncher.Margin = new Thickness(0, point.Y + bt.RenderSize.Height, 0, 0);

            PluginLauncher.Shown = true;
        }

        private void PluginLauncher_ShownChanged(bool val)
        {
            PluginLauncher_BT.IsSelected = val;
        }

        void AddAppToList(Control_PlugIn cp)
        {

            Plugins.Launcher.PluginItem item = new Plugins.Launcher.PluginItem();
            item.plugin = cp;
            item.Text = cp.Title;
            item.Image = cp.Image;
            item.Clicked += item_Clicked;

            if (!PluginLauncher.Plugins.Contains(item)) PluginLauncher.Plugins.Add(item);

        }

        void item_Clicked(Plugins.Launcher.PluginItem item)
        {
            if (item.plugin != null) AddPageAsTab(item.plugin, item.plugin.Title, item.plugin.Image);
            PluginLauncher.Shown = false;
        }

        void RemoveAppFromList(Control_PlugIn cp)
        {



        }

        #endregion

        #region "Main Menu Button"

        private void MainMenu_BT_Clicked(TH_Button bt)
        {
            Point point = bt.TransformToAncestor(Main_GRID).Transform(new Point(0, 0));
            MainMenu.Margin = new Thickness(0, point.Y + bt.RenderSize.Height, 5, 0);

            MainMenu.Shown = true;
        }

        private void MainMenu_ShownChanged(bool val)
        {
            MainMenu_BT.IsSelected = val;
        }

        #endregion

        #region "Toolbars"

        #region "Main Menu"

        public bool MainMenuBar_Show
        {
            get { return (bool)GetValue(MainMenuBar_ShowProperty); }
            set 
            { 
                SetValue(MainMenuBar_ShowProperty, value); 

                if (value)
                {
                    MainMenuBar_Shown = true;

                    MainMenuBarLoaded_TIMER = new System.Timers.Timer();
                    MainMenuBarLoaded_TIMER.Interval = 200;
                    MainMenuBarLoaded_TIMER.Elapsed += MainMenuBarLoaded_TIMER_Elapsed;
                    MainMenuBarLoaded_TIMER.Enabled = true;
                }
            
            }
        }

        public static readonly DependencyProperty MainMenuBar_ShowProperty =
            DependencyProperty.Register("MainMenuBar_Show", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public bool MainMenuBar_Shown
        {
            get { return (bool)GetValue(MainMenuBar_ShownProperty); }
            set { SetValue(MainMenuBar_ShownProperty, value); }
        }

        public static readonly DependencyProperty MainMenuBar_ShownProperty =
            DependencyProperty.Register("MainMenuBar_Shown", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        System.Timers.Timer MainMenuBarLoaded_TIMER;

        void MainMenuBarLoaded_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sender.GetType() == typeof(System.Timers.Timer))
            {
                System.Timers.Timer timer = (System.Timers.Timer)sender;
                timer.Enabled = false;
            }

            this.Dispatcher.BeginInvoke(new Action(MainMenuBarLoaded_TIMER_Elapsed_GUI));
        }

        void MainMenuBarLoaded_TIMER_Elapsed_GUI()
        {
            MainMenuBar_Shown = false;
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            About_Open();
        }

        private void MenuItem_Options_Click(object sender, RoutedEventArgs e) { }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_RestoreDefaultConfiguration_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
        }

        #endregion

        object CreateMenuItemIcon(ImageSource img)
        {

            Rectangle rect = new Rectangle();
            rect.Width = 20;
            rect.Height = 20;
            rect.Fill = Brush_Functions.GetSolidBrushFromResource(this, "DWBlue");

            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = img;
            imgBrush.Stretch = Stretch.Uniform;

            rect.Resources.Add("IMG", imgBrush);

            Style style = new System.Windows.Style();
            style.TargetType = typeof(Rectangle);

            Setter setter = new Setter();
            setter.Property = Rectangle.OpacityMaskProperty;
            setter.Value = rect.TryFindResource("IMG");

            style.Setters.Add(setter);

            rect.Style = style;

            return rect;

        }

        #endregion

        #region "PlugIns"

        List<PlugInConfiguration> EnabledPlugIns;

        void LoadPlugIns()
        {
            EnabledPlugIns = new List<PlugInConfiguration>();

            Splash_UpdateStatus("...Loading Page Plugins");

            PagePlugIns_Find();

            PagePlugIns_Load();
        }

        #region "Pages"

        PagePlugs PPLUGS;

        class PagePlugs
        {
            // Store Page Plugins
            [ImportMany(typeof(Control_PlugIn))]
            public IEnumerable<Lazy<Control_PlugIn>> PlugIns { get; set; }
        }

        public List<Lazy<Control_PlugIn>> PagePlugIns;

        public void PagePlugIns_Find()
        {

            PagePlugIns = new List<Lazy<Control_PlugIn>>();

            string pagePath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            pagePath = TH_Global.FileLocations.TrakHound + @"\PlugIns\Pages\";
            if (Directory.Exists(pagePath)) PagePlugIns_Find_Recursive(pagePath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pagePath = AppDomain.CurrentDomain.BaseDirectory + @"PlugIns\Pages\";
            if (Directory.Exists(pagePath)) PagePlugIns_Find_Recursive(pagePath);


            // Add Buttons for Page Plugins on PlugIn Options page
            if (Properties.Settings.Default.PagePlugIn_Configurations != null && pluginsPage != null)
            {
                pluginsPage.ClearInstalledPageItems();

                foreach (PlugInConfiguration config in Properties.Settings.Default.PagePlugIn_Configurations.ToList())
                {
                    pluginsPage.AddInstalledPageItem(config);
                }
            }

        }

        List<string> DefaultEnablePlugins = new List<string> { "dashboard", "device compare", "table manager" };

        void PagePlugIns_Find_Recursive(string Path)
        {
            PPLUGS = new PagePlugs();

            var PageCatalog = new DirectoryCatalog(Path);
            var PageContainer = new CompositionContainer(PageCatalog);
            PageContainer.SatisfyImportsOnce(PPLUGS);

            if (PPLUGS.PlugIns != null)
            {

                List<PlugInConfiguration> configs;

                if (Properties.Settings.Default.PagePlugIn_Configurations != null)
                {
                    configs = Properties.Settings.Default.PagePlugIn_Configurations.ToList();
                }
                else
                {
                    configs = new List<PlugInConfiguration>();
                }

                foreach (Lazy<Control_PlugIn> LCP in PPLUGS.PlugIns.ToList())
                {
                    try
                    {
                        Control_PlugIn CP = LCP.Value;

                        Console.WriteLine(CP.Title + " Found in '" + Path + "'");

                        PlugInConfiguration config = configs.Find(x => x.name.ToUpper() == CP.Title.ToUpper());
                        if (config == null)
                        {
                            Console.WriteLine("Page PlugIn Configuration created for " + CP.Title);
                            config = new PlugInConfiguration();
                            config.name = CP.Title;
                            config.description = CP.Description;

                            // Automatically enable basic Plugins by TrakHound
                            if (DefaultEnablePlugins.Contains(config.name.ToLower()))
                            {
                                config.enabled = true;
                                Console.WriteLine("Default TrakHound Plugin Initialized as 'Enabled'");
                            }
                            else config.enabled = false;

                            config.parent = CP.DefaultParent;
                            config.category = CP.DefaultParentCategory;

                            config.SubCategories = CP.SubCategories;

                            configs.Add(config);
                        }
                        else Console.WriteLine("Page PlugIn Configuration found for " + CP.Title);

                        if (config.parent == null) config.EnabledChanged += PageConfig_EnabledChanged;

                        PagePlugIns.Add(LCP);

                    }
                    catch (Exception ex)
                    {
                        Message_Center.Message_Data mData = new Message_Center.Message_Data();
                        mData.title = "PlugIn Error";
                        mData.text = "Error during plugin intialization";
                        mData.additionalInfo = ex.Message;

                        messageCenter.AddError(mData);
                    }

                }


                // Create a copy of configs since we are modifying it
                List<PlugInConfiguration> tempConfigs = new List<PlugInConfiguration>();
                tempConfigs.AddRange(configs);

                foreach (PlugInConfiguration config in tempConfigs)
                {
                    if (configs.Contains(config))
                    {
                        if (config.parent != null)
                        {
                            if (config.category != null)
                            {
                                PlugInConfiguration match1 = configs.Find(x => x.name.ToUpper() == config.parent.ToUpper());
                                if (match1 != null)
                                {
                                    PlugInConfigurationCategory match2 = match1.SubCategories.Find(x => x.name.ToUpper() == config.category.ToUpper());
                                    if (match2 != null)
                                    {
                                        configs.Remove(config);
                                        if (match2.PlugInConfigurations.Find(x => x.name.ToUpper() == config.name.ToUpper()) == null)
                                        {
                                            match2.PlugInConfigurations.Add(config);
                                        }

                                    }
                                }
                            }
                        }
                    }
                }

                Properties.Settings.Default.PagePlugIn_Configurations = configs;
                Properties.Settings.Default.Save();
            }

            foreach (string directory in Directory.GetDirectories(Path, "*", SearchOption.AllDirectories))
            {
                PagePlugIns_Find_Recursive(directory);
            }
        }

        void PageConfig_EnabledChanged(PlugInConfiguration config)
        {
            if (config.enabled) PagePlugIns_Load(config);
            else PagePlugIns_Unload(config);

            Properties.Settings.Default.Save();

        }

        public void PagePlugIns_Load()
        {
            if (Properties.Settings.Default.PagePlugIn_Configurations != null)
            {
                foreach (PlugInConfiguration config in Properties.Settings.Default.PagePlugIn_Configurations.ToList())
                {
                    PagePlugIns_Load(config);
                }
            }
        }

        public void PagePlugIns_Load(PlugInConfiguration config)
        {
            if (config != null)
            {
                if (!EnabledPlugIns.Contains(config))
                {
                    if (config.enabled)
                    {
                        if (PagePlugIns != null)
                        {
                            Lazy<Control_PlugIn> LWP = PagePlugIns.Find(x => x.Value.Title.ToUpper() == config.name.ToUpper());
                            if (LWP != null)
                            {
                                try
                                {
                                    Control_PlugIn CP = LWP.Value;

                                    Splash_UpdateStatus("...Loading Page Plugin : " + CP.Title);
                                    Splash_AddPagePlugin(CP);

                                    CP.Devices = Devices;
                                    CP.DataEvent += CP_DataEvent;
                                    CP.SubCategories = config.SubCategories;

                                    CP.PlugIns = new List<Control_PlugIn>();

                                    if (CP.SubCategories != null)
                                    {
                                        foreach (PlugInConfigurationCategory subcategory in CP.SubCategories)
                                        {
                                            foreach (PlugInConfiguration subConfig in subcategory.PlugInConfigurations)
                                            {
                                                Lazy<Control_PlugIn> sLCP = PagePlugIns.Find(x => x.Value.Title.ToUpper() == subConfig.name.ToUpper());
                                                if (sLCP != null)
                                                {
                                                    CP.PlugIns.Add(sLCP.Value);
                                                }
                                            }
                                        }
                                    }

                                    CP.Initialize();

                                    AddAppToList(CP);

                                    if (CP.OpenOnStartUp)
                                    {
                                        AddPageAsTab(CP, CP.Title, CP.Image);
                                    }

                                    PagePlugIns_CreateOptionsPage(CP);

                                    //// Add to View Menu -------------------------------------------
                                    //TH_MenuItem mi = new TH_MenuItem();
                                    //mi.Header = CP.Title;
                                    //mi.Icon = CreateMenuItemIcon(CP.Image);
                                    //mi.Data = CP;
                                    //mi.Clicked += mi_Clicked;

                                    //View_MenuItem.Items.Add(mi);
                                    //// ------------------------------------------------------------

                                    //// Add to Navigation Toolbar ----------------------------------
                                    //Image Nav_IMG = new Image();
                                    //Nav_IMG.Source = CP.Image;

                                    //Border Nav_BD = new Border();
                                    //Nav_BD.Height = 20;
                                    //Nav_BD.Width = 20;
                                    //Nav_BD.Child = Nav_IMG;

                                    //NavigationItem Nav_BT = new NavigationItem();
                                    //Nav_BT.Content = Nav_BD;
                                    //Nav_BT.Data = CP;
                                    //Nav_BT.Clicked += Nav_BT_Clicked;
                                    //Nav_BT.Style = (Style)TryFindResource("ToolBarButtonStyle");
                                    //Navigation_TBAR.Items.Add(Nav_BT);
                                    //// -----------------------------------------------------------

                                    EnabledPlugIns.Add(config);
                                }
                                catch (Exception ex)
                                {
                                    Message_Center.Message_Data mData = new Message_Center.Message_Data();
                                    mData.title = "PlugIn Error";
                                    mData.text = "Error during plugin load";
                                    mData.additionalInfo = ex.Message;

                                    messageCenter.AddError(mData);
                                }
                            }
                        }
                    }
                }
            }
        }

        void CP_DataEvent(DataEvent_Data de_d)
        {
            if (Properties.Settings.Default.PagePlugIn_Configurations != null)
            {
                List<PlugInConfiguration> configs = Properties.Settings.Default.PagePlugIn_Configurations.ToList();

                foreach (PlugInConfiguration config in configs)
                {
                    if (config.enabled)
                    {
                        Lazy<Control_PlugIn> LCP = PagePlugIns.ToList().Find(x => x.Value.Title == config.name);
                        if (LCP != null)
                        {
                            if (LCP.IsValueCreated)
                            {
                                Control_PlugIn CP = LCP.Value;
                                CP.Update_DataEvent(de_d);
                            }
                        }
                    }
                }
            }
        }

        void Nav_BT_Clicked(object data)
        {
            Control_PlugIn cp = data as Control_PlugIn;

            AddPageAsTab(cp, cp.Title, cp.Image);
        }

        void mi_Clicked(object data)
        {
            Control_PlugIn cp = data as Control_PlugIn;

            AddPageAsTab(cp, cp.Title, cp.Image);
        }


        public void PagePlugIns_Unload(PlugInConfiguration config)
        {
            if (config != null)
            {
                if (!config.enabled)
                {
                    // Remove TabItem
                    foreach (TH_TabItem ti in Pages_TABCONTROL.Items.OfType<TH_TabItem>().ToList())
                    {
                        if (ti.Header != null)
                        {
                            if (ti.Header.ToString().ToUpper() == config.name.ToUpper())
                            {
                                if (ti.Content.GetType() == typeof(Grid))
                                {
                                    Grid grid = ti.Content as Grid;
                                    grid.Children.Clear();
                                }
                                Pages_TABCONTROL.Items.Remove(ti);
                            }
                        }
                    }

                    if (optionsManager != null)
                    {
                        foreach (ListButton lb in optionsManager.Pages_STACK.Children.OfType<ListButton>().ToList())
                        {
                            if (lb.Text.ToUpper() == config.name.ToUpper())
                            {
                                optionsManager.Pages_STACK.Children.Remove(lb);
                            }
                        }
                    }

                    if (EnabledPlugIns.Contains(config)) EnabledPlugIns.Remove(config);
                }
            }
        }

        void PagePlugIns_CreateOptionsPage(Control_PlugIn cp)
        {

            if (cp.Options != null) optionsManager.AddPage(cp.Options);

        }


        #endregion

        #endregion

        #region "Clients"

        // Device Update Interval
        public int clientUpdateInterval = 5000;

        List<Device_Client> Devices = new List<Device_Client>();

        List<ReturnData> DeviceData = new List<ReturnData>();

        private void ReadConfigurationFile()
        {

            UpdateExceptionsThrown = new List<string>();

            string configPath;

            string localPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + "Configuration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "Configuration.Xml";

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            if (System.IO.File.Exists(configPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);

                int index = 0;

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

                                                Configuration config = GetSettingsFromNode(ChildNode);
                                                if (config != null)
                                                {
                                                    Device_Client device = new Device_Client(config);
                                                    device.Index = index;
                                                    device.DataUpdated += Device_DataUpdated;
                                                    Devices.Add(device);
                                                    index += 1;
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                        }
                    }             
                }
            }
        }

        List<string> UpdateExceptionsThrown;

        void Device_DataUpdated(ReturnData rd)
        {
            if (rd != null)
            {
                //ReturnData dd = DeviceData.Find(x => x.configuration.DataBaseName == rd.configuration.DataBaseName);
                //if (dd != null)
                //{
                //    dd = rd;
                //}
                //else
                //{
                //    DeviceData.Add(rd);
                //}

                // Set Update Interval if changed
                if (rd.index >= 0) 
                {
                    Device_Client client = Devices[rd.index];
                    if (client.UpdateInterval != clientUpdateInterval)
                        client.UpdateInterval = clientUpdateInterval;
                }

                // Send ReturnData object to all plugins
                if (Properties.Settings.Default.PagePlugIn_Configurations != null)
                {
                    List<PlugInConfiguration> configs = Properties.Settings.Default.PagePlugIn_Configurations.ToList();

                    foreach (PlugInConfiguration config in configs)
                    {
                        if (config.enabled && PagePlugIns != null)
                        {
                            foreach (Lazy<Control_PlugIn> LCP in PagePlugIns.ToList())
                            {
                                if (LCP != null)
                                {
                                    if (LCP.IsValueCreated)
                                    {
                                        if (LCP.Value.Title.ToLower() == config.name.ToLower())
                                        {
                                            try
                                            {
                                                Control_PlugIn CP = LCP.Value;
                                                CP.Update(rd);
                                            }
                                            catch (Exception ex)
                                            {
                                                if (!UpdateExceptionsThrown.Contains(config.name))
                                                {
                                                    Message_Center.Message_Data mData = new Message_Center.Message_Data();
                                                    mData.title = config.name + ": PlugIn Error";
                                                    mData.text = "Error during plugin update";
                                                    mData.additionalInfo = ex.Message;

                                                    messageCenter.AddError(mData);
                                                    UpdateExceptionsThrown.Add(config.name);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Configuration GetSettingsFromNode(XmlNode Node)
        {

            Configuration Result = null;

            string configPath = null;

            foreach (XmlNode ChildNode in Node.ChildNodes)
            {
                switch (ChildNode.Name.ToLower())
                {
                    case "configuration_path": configPath = ChildNode.InnerText; break;
                }
            }

            if (configPath != null)
            {
                configPath = GetConfigurationPath(configPath);

                Result = Configuration.ReadConfigFile(configPath);

                if (Result == null)
                {
                    Message_Center.Message_Data mData = new Message_Center.Message_Data();
                    mData.title = "Device Configuration Error";
                    mData.text = "Could not load device configuration from " + configPath;
                    mData.additionalInfo = "Check to make sure the file exists at " 
                        + configPath 
                        + " and that the format is correct and restart TrakHound Client."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "For more information please contact us at info@TrakHound.org";
                    if (messageCenter != null) messageCenter.AddError(mData);
                }
            }

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

                // Check local app Path
                path = AppDomain.CurrentDomain.BaseDirectory + "Configuration Files\\" + original;
                 if (File.Exists(path)) return path;

                // if no files exist return null
                return null;
            }
            else return path;
        }

        #endregion

        #region "Message Center"

        public int NotificationsCount
        {
            get { return (int)GetValue(NotificationsCountProperty); }
            set { SetValue(NotificationsCountProperty, value); }
        }

        public static readonly DependencyProperty NotificationsCountProperty =
            DependencyProperty.Register("NotificationsCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        private void MessageCenter_ToolBarItem_Clicked()
        {
            messageCenter.Shown = !messageCenter.Shown;
        }

        #endregion

        #region "Developer Console"

        public bool DevConsole_Shown
        {
            get { return (bool)GetValue(DevConsole_ShownProperty); }
            set { SetValue(DevConsole_ShownProperty, value); }
        }

        public static readonly DependencyProperty DevConsole_ShownProperty =
            DependencyProperty.Register("DevConsole_Shown", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        private void DeveloperConsole_ToolBarItem_Clicked()
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

        #endregion

    }

    class NavigationItem : Button
    {

        public object Data { get; set; }

        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(Data);
        }

        public delegate void Clicked_Handler(object data);

        public event Clicked_Handler Clicked;

    }



}
