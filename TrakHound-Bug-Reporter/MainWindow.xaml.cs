using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using System.Threading;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using TH_GitHub;
using TH_Global;
using TH_Global.Web;
using TH_Global.Functions;

namespace TrakHound_Bug_Reporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Init();
        }

        public MainWindow(string subject, string message)
        {
            Init();

            Subject = subject;
            Message = message;
        }

        public MainWindow(Exception ex)
        {
            Init();

            Subject = "Bug Report : [Exception] : " + DateTime.Now.ToString(); ;

            string type = "Exception";
            string exMessage = ex.Message;
            string exSource = ex.Source;
            string exHelplink = ex.HelpLink;
            string exTargetSite = ex.TargetSite.Name;

            string stackTrace = ex.StackTrace;

            string n = Environment.NewLine;

            string format =
                "Bug Type : {0}" + n + n +
                "Message : {1}" + n + n +

                "Source : {2}" + n +
                "Help Link : {3}" + n +
                "Target Site : {4}" + n + n +

                "Stack Trace : {5}";

            Message = String.Format(
                format,
                type,
                exMessage,
                exSource,
                exHelplink,
                exTargetSite,
                stackTrace
                );
        }

        private void Init()
        {
            InitializeComponent();
            DataContext = this;

            Loading = true;
            LoadingStatus = "Loading..";

            // Gathered Data
            Timestamp = DateTime.Now.ToString();
            OperatingSystem = GetOperatingSystem();
            //MemoryUsed = GetMemoryUsed();

            GitHub_Login();

            GetLogFiles();
        }

        private LogFileInfo logFileInfo;

        #region "Dependency Properties"

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public string LoadingStatus
        {
            get { return (string)GetValue(LoadingStatusProperty); }
            set { SetValue(LoadingStatusProperty, value); }
        }

        public static readonly DependencyProperty LoadingStatusProperty =
            DependencyProperty.Register("LoadingStatus", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public string Timestamp
        {
            get { return (string)GetValue(TimestampProperty); }
            set { SetValue(TimestampProperty, value); }
        }

        public static readonly DependencyProperty TimestampProperty =
            DependencyProperty.Register("Timestamp", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public string OperatingSystem
        {
            get { return (string)GetValue(OperatingSystemProperty); }
            set { SetValue(OperatingSystemProperty, value); }
        }

        public static readonly DependencyProperty OperatingSystemProperty =
            DependencyProperty.Register("OperatingSystem", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public string MemoryUsed
        {
            get { return (string)GetValue(MemoryUsedProperty); }
            set { SetValue(MemoryUsedProperty, value); }
        }

        public static readonly DependencyProperty MemoryUsedProperty =
            DependencyProperty.Register("MemoryUsed", typeof(string), typeof(MainWindow), new PropertyMetadata(null));



        public bool IncludeLogFiles
        {
            get { return (bool)GetValue(IncludeLogFilesProperty); }
            set { SetValue(IncludeLogFilesProperty, value); }
        }

        public static readonly DependencyProperty IncludeLogFilesProperty =
            DependencyProperty.Register("IncludeLogFiles", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));


        public string LogFilePath
        {
            get { return (string)GetValue(LogFilePathProperty); }
            set { SetValue(LogFilePathProperty, value); }
        }

        public static readonly DependencyProperty LogFilePathProperty =
            DependencyProperty.Register("LogFilePath", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public bool IncludeScreenshot
        {
            get { return (bool)GetValue(IncludeScreenshotProperty); }
            set { SetValue(IncludeScreenshotProperty, value); }
        }

        public static readonly DependencyProperty IncludeScreenshotProperty =
            DependencyProperty.Register("IncludeScreenshot", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));


        public string ScreenshotPath
        {
            get { return (string)GetValue(ScreenshotPathProperty); }
            set { SetValue(ScreenshotPathProperty, value); }
        }

        public static readonly DependencyProperty ScreenshotPathProperty =
            DependencyProperty.Register("ScreenshotPath", typeof(string), typeof(MainWindow), new PropertyMetadata(null));



        public string Subject
        {
            get { return (string)GetValue(SubjectProperty); }
            set { SetValue(SubjectProperty, value); }
        }

        public static readonly DependencyProperty SubjectProperty =
            DependencyProperty.Register("Subject", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        #endregion

        #region "Event Handlers"

        private void Send_Clicked(TH_WPF.Button bt)
        {
            Send();
        }

        private void ViewLogFiles_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("explorer.exe", LogFilePath);
        }

        private void ViewScreenshot_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("explorer.exe", ScreenshotPath);
        }

        #endregion

        #region "Gathered Data"

        private string GetOperatingSystem()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                        select x.GetPropertyValue("Caption")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }

        //private string GetMemoryUsed()
        //{
        //    Process proc = Process.GetCurrentProcess();

        //    Int64 memory = proc.PrivateMemorySize64;

        //    if (memory > Properties.Settings.Default.Usage_MemoryUsed_Value)
        //    {
        //        Properties.Settings.Default.Usage_MemoryUsed_Value = memory;
        //        Properties.Settings.Default.Usage_MemoryUsed_Date = DateTime.Now;
        //        Properties.Settings.Default.Save();
        //    }

        //    return String_Functions.FileSizeSuffix(memory);
        //}


        private class LogFileInfo
        {
            public string TempDir { get; set; }
            public string ZipPath { get; set; }
        }

        private void GetLogFiles()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(GetLogFiles_Worker));
        }

        private void GetLogFiles_Worker(object o)
        {
            LogFileInfo result = null;

            try
            {
                // Create path for tempPath directory
                string tempDir = FileLocations.TrakHoundTemp;
                string tempName = String_Functions.RandomString(10);
                string tempPath = Path.Combine(tempDir, tempName);
                string zipPath = Path.Combine(tempDir, tempName + ".zip");
                Directory.CreateDirectory(tempPath);

                // Get Individual Files
                string debugPath = GetLogFile(FileLocations.DebugLogs);
                string errorPath = GetLogFile(FileLocations.ErrorLogs);
                string notificationsPath = GetLogFile(FileLocations.NotificationLogs);
                string warningPath = GetLogFile(FileLocations.WarningLogs);

                if (debugPath != null || errorPath != null || notificationsPath != null || warningPath != null)
                {
                    // Copy files to tempPath directory
                    if (debugPath != null) File.Copy(debugPath, Path.Combine(tempPath, "Debug-" + Path.GetFileName(debugPath)), true);
                    if (errorPath != null) File.Copy(errorPath, Path.Combine(tempPath, "Error-" + Path.GetFileName(errorPath)), true);
                    if (notificationsPath != null) File.Copy(notificationsPath, Path.Combine(tempPath, "Notification-" + Path.GetFileName(notificationsPath)), true);
                    if (warningPath != null) File.Copy(warningPath, Path.Combine(tempPath, "Warning-" + Path.GetFileName(warningPath)), true);

                    // Create Zip file for tempPath
                    CreateZipFile(zipPath, tempPath);

                    result = new LogFileInfo();
                    result.TempDir = tempPath;
                    result.ZipPath = zipPath;
                }
            }
            catch (Exception ex) { Logger.Log("Exception : " + ex.Message); }

            Dispatcher.BeginInvoke(new Action<LogFileInfo>(GetLogFiles_GUI), UI_Functions.PRIORITY_BACKGROUND, new object[] { result });
        }

        private void GetLogFiles_GUI(LogFileInfo info)
        {
            Loading = false;

            logFileInfo = info;
            if (info != null) LogFilePath = info.TempDir;
        }

        private string GetLogFile(string path)
        {
            foreach (var filePath in Directory.GetFiles(path))
            {
                string filename = Path.GetFileName(filePath);

                string match1 = "Log-" + DateTime.Now.ToString("yyyy-M-d") + ".xml";
                //string match2 = "Log-" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToString("yyyy-M-d") + ".xml";

                //if (filename == match1 || filename == match2)
                if (filename == match1)
                {
                    return filePath;
                }
            }

            return null;
        }


        public void CreateZipFile(string destPath, string sourcePath)
        {

            FileStream fsOut = File.Create(destPath);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);

            // This setting will strip the leading part of the folder path in the entries, to
            // make the entries relative to the starting folder.
            // To include the full path for each entry up to the drive root, assign folderOffset = 0.
            int folderOffset = sourcePath.Length + (sourcePath.EndsWith("\\") ? 0 : 1);

            CompressFolder(sourcePath, zipStream, folderOffset);

            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        private void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }


        public string GetScreenshot(Visual vis)
        {
            // Create path for tempPath directory
            string tempDir = FileLocations.TrakHoundTemp;
            string tempName = String_Functions.RandomString(10) + ".jpg";
            string tempPath = Path.Combine(tempDir, tempName);

            Window_Functions.GetScreenShot(vis, tempPath);

            return tempPath;
        }

        #endregion

        #region "GitHub"

        public bool GitHubCreateIssue
        {
            get { return (bool)GetValue(GitHubCreateIssueProperty); }
            set { SetValue(GitHubCreateIssueProperty, value); }
        }

        public static readonly DependencyProperty GitHubCreateIssueProperty =
            DependencyProperty.Register("GitHubCreateIssue", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));


        public string GitHubUsername
        {
            get { return (string)GetValue(GitHubUsernameProperty); }
            set { SetValue(GitHubUsernameProperty, value); }
        }

        public static readonly DependencyProperty GitHubUsernameProperty =
            DependencyProperty.Register("GitHubUsername", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public string GitHubPassword
        {
            get { return (string)GetValue(GitHubPasswordProperty); }
            set { SetValue(GitHubPasswordProperty, value); }
        }
        public static readonly DependencyProperty GitHubPasswordProperty =
            DependencyProperty.Register("GitHubPassword", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public bool GitHubLoggedIn
        {
            get { return (bool)GetValue(GitHubLoggedInProperty); }
            set { SetValue(GitHubLoggedInProperty, value); }
        }

        public static readonly DependencyProperty GitHubLoggedInProperty =
            DependencyProperty.Register("GitHubLoggedIn", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        private class GitHub
        {
            public static HTTP.HeaderData LoginHeaderData { get; set; }

            public static HTTP.HeaderData GetLogin()
            {
                string token = Properties.Settings.Default.Github_Token;
                //string token = null;
                if (!string.IsNullOrEmpty(token)) // Use OAuth2 Token
                {
                    return Authentication.GetOAuth2Header(token);
                }

                return null;
            }

            public static bool CreateIssue(HTTP.HeaderData loginHeader)
            {
                var issue = new TH_GitHub.Issues.Issue();
                issue.Title = "Test Issue Using Token";
                issue.Content = "Issue Content";
                issue.Comments = "Issue Comments";
                issue.Username = "TrakHound";
                issue.Type = Issues.IssueType.UserSubmitted;

                return Issues.Create(issue, loginHeader);
            }
        }

        private void Login_Clicked(TH_WPF.Button bt)
        {
            var loginData = GithubLogin.Show();
            if (loginData != null)
            {
                GitHub_Login();
            }
        }

        private void GitHub_Login()
        {
            string username = Properties.Settings.Default.Github_Username;
            string token = Properties.Settings.Default.Github_Token;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(token))
            {
                GitHubUsername = username;
                GitHubLoggedIn = true;
            }
        }

        #endregion

        #region "Send"

        private void Send()
        {
            Loading = true;
            LoadingStatus = "Sending Bug Report..";

            // Create Issue on Github
            if (GitHubLoggedIn && GitHubCreateIssue)
            {
                HTTP.HeaderData loginHeader = null;
                loginHeader = GitHub.GetLogin();
                if (loginHeader != null)
                {
                    bool success = GitHub.CreateIssue(loginHeader);
                    if (!success)
                    {
                        TH_WPF.MessageBox.Show("Error Creating GitHub Issue. Check GitHub login credentials and try again. If problems persist, uncheck 'Create GitHub Issue' and create an issue using www.GitHub.com", "Error Creating GitHub Issue", TH_WPF.MessageBoxButtons.Ok);
                    }
                }
            }

            string subject = Subject;
            string body = CreateBody();

            var attachments = new List<string>();
            if (logFileInfo != null) attachments.Add(logFileInfo.ZipPath);
            if (ScreenshotPath != null) attachments.Add(ScreenshotPath);

            Cursor = Cursors.Wait;
            SendEmail(subject, body, attachments.ToArray());
            Cursor = Cursors.Arrow;

            Close();
            TH_WPF.MessageBox.Show("Bug Report Sent Successfully");
        }

        #region "Email"

        private string CreateBody()
        {


            return Message;
        }

        private static bool SendEmail(string subject, string body, string[] attachments)
        {
            bool result = false;

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("box897.bluehost.com");
                mail.From = new MailAddress("bugs@trakhound.org");
                mail.To.Add("bugs@trakhound.org");
                mail.Subject = subject;
                mail.Body = body;

                // Add attachments
                foreach (var file in attachments)
                {
                    var att = new Attachment(file);
                    mail.Attachments.Add(att);
                }

                SmtpServer.Port = 25;
                SmtpServer.Credentials = new System.Net.NetworkCredential("bugs@trakhound.org", @"_EM51=RuY?y\a{U");

                SmtpServer.Send(mail);

                result = true;
            }
            catch (SmtpException ex)
            {
                Logger.Log("SmtpException :: " + ex.Message, Logger.LogLineType.Error);
            }
            catch (Exception ex)
            {
                Logger.Log("Exception :: " + ex.Message, Logger.LogLineType.Error);
            }

            return result;
        }

        #endregion

        #endregion

    }
}
