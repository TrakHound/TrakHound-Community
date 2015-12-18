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
//using System.Windows.Shapes;

using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace TrakHound_Client_Updater
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

            Update_Start();
        }

        static void OpenClient()
        {
            try
            {
                string clientPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "trakhound-client.exe";

                //string clientPath = @"F:\feenux\TrakHound\TrakHound\Client\TrakHound-Client\bin\Debug\trakhound-client.exe";

                Process.Start(clientPath);
            }
            catch (Exception ex) { Console.WriteLine("TrakHound-Client-Updater.OpenClient() :: " + ex.Message); }
        }


        Thread update_THREAD;

        void Update_Start()
        {
            Loading = true;

            if (update_THREAD != null) update_THREAD.Abort();

            update_THREAD = new Thread(new ThreadStart(Update_Worker));
            update_THREAD.Start();
        }

        void Update_Worker()
        {
            string[] keyNames = GetRegistryKeyNames();
            if (keyNames != null)
                if (keyNames.Length > 0)
                {
                    //Visibility = System.Windows.Visibility.Visible;

                    // Set Progress Maximum
                    this.Dispatcher.BeginInvoke(new Action<double>(Update_ProgressMaximum), new object[] { GetMax(keyNames) });

                    foreach (string keyName in keyNames)
                    {
                        Console.WriteLine("Registry Update Key Found :: " + keyName);

                        this.Dispatcher.BeginInvoke(new Action<string>(Update_ApplicationName), new object[] { keyName });

                        string keyValue = GetRegistryKey(keyName);
                        if (keyValue != null)
                        {
                            if (keyValue.Contains(";"))
                            {
                                string unzipDirectory = keyValue.Substring(0, keyValue.IndexOf(';'));
                                string copyDirectory = keyValue.Substring(keyValue.IndexOf(';') + 1);

                                Console.WriteLine(keyName + " = " + keyValue);
                                Console.WriteLine("unzipDirectory = " + unzipDirectory);
                                Console.WriteLine("copyDirectory = " + copyDirectory);

                                bool success = true;

                                foreach (string filePath in Directory.GetFiles(unzipDirectory))
                                {
                                    string fileName = Path.GetFileName(filePath);

                                    string copyPath = copyDirectory + "\\" + fileName;

                                    this.Dispatcher.BeginInvoke(new Action<string>(Update_Status), new object[] { "Updating... (" + fileName + ")" });

                                    try
                                    {
                                        File.Copy(filePath, copyPath, true);
                                        this.Dispatcher.BeginInvoke(new Action(Update_ProgressValue), new object[] { });
                                    }
                                    catch (Exception ex)
                                    {
                                        success = false;

                                        //MessageBox.Show("AppStart.Update() :: Exception :: " + ex.Message);
                                        this.Dispatcher.BeginInvoke(new Action<string>(Update_DEBUGException), new object[] { "AppStart.Update() :: Exception :: " + ex.Message });

                                        Console.WriteLine("AppStart.Update() :: Exception :: " + ex.Message);
                                    }

                                    // Give time to let status update
                                    Thread.Sleep(100);
                                }
                                
                                if (success) DeleteRegistryKey(keyName);
                            }
                        }
                    }

                    this.Dispatcher.BeginInvoke(new Action(Update_Completed), new object[] { });

                    Thread.Sleep(2000);

                }

            this.Dispatcher.BeginInvoke(new Action(Update_Finished), new object[] { });
        }

        int GetMax(string[] keyNames)
        {
            int result = 0;

            foreach (string keyName in keyNames)
            {
                string keyValue = GetRegistryKey(keyName);
                if (keyValue != null)
                {
                    if (keyValue.Contains(";"))
                    {
                        string unzipDirectory = keyValue.Substring(0, keyValue.IndexOf(';'));
                        foreach (string filePath in Directory.GetFiles(unzipDirectory))
                        {
                            result += 1;
                        }
                    }
                }
            }

            return result;
        }

        void Update_DEBUGException(string message) { MessageBox.Show(message); }


        void Update_ProgressValue() { ProgressValue += 1; }

        void Update_ProgressMaximum(double max) { ProgressMaximum = max; }

        void Update_ApplicationName(string name) { ApplicationName = name; }

        void Update_Status(string status) { Status = status; }

        void Update_Completed()
        {
            Status = "Update(s) Completed Successfully";
            Loading = false;
        }

        void Update_Finished()
        {
            OpenClient();

            Close();
        }

        #region "Properties"

        public string ApplicationName
        {
            get { return (string)GetValue(ApplicationNameProperty); }
            set { SetValue(ApplicationNameProperty, value); }
        }

        public static readonly DependencyProperty ApplicationNameProperty =
            DependencyProperty.Register("ApplicationName", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public double ProgressValue
        {
            get { return (double)GetValue(ProgressValueProperty); }
            set { SetValue(ProgressValueProperty, value); }
        }

        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(double), typeof(MainWindow), new PropertyMetadata(0d));


        public double ProgressMaximum
        {
            get { return (double)GetValue(ProgressMaximumProperty); }
            set { SetValue(ProgressMaximumProperty, value); }
        }

        public static readonly DependencyProperty ProgressMaximumProperty =
            DependencyProperty.Register("ProgressMaximum", typeof(double), typeof(MainWindow), new PropertyMetadata(0d));


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        #endregion

        public static string GetRegistryKey(string keyName)
        {
            string Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("Updates");

                // Open CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                RegistryKey updateKey = updatesKey.OpenSubKey(keyName);

                // Read value for [keyName] to [keyValue]
                Result = updateKey.GetValue(keyName).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoUpdater_GetRegistryKey() : " + ex.Message);
            }

            return Result;
        }

        public static string[] GetRegistryKeyNames()
        {
            string[] Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("Updates");

                Result = updatesKey.GetSubKeyNames();



                //// Open CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                //RegistryKey updateKey = updatesKey.OpenSubKey(keyName);

                //// Read value for [keyName] to [keyValue]
                //Result = updateKey.GetValue(keyName).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoUpdater_GetRegistryKeys() : " + ex.Message);
            }

            return Result;
        }

        public static void DeleteRegistryKey(string keyName)
        {
            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound", true);

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("Updates", true);

                // Delete CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                updatesKey.DeleteSubKey(keyName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoUpdater_DeleteRegistryKey() : " + ex.Message);
            }
        }
    }
}
