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

using WinInterop = System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Interop;

using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Diagnostics;

using TH_Global;
using TH_Updater;


namespace TrakHound_Client_Updater
{
    public partial class MainWindow
    {

        static void CheckForUpdates()
        {



        }




        //void AutoUpdater_Start()
        //{
        //    var check = new UpdateCheck();
        //    check.AppInfoReceived += AutoUpdater_AppInfoReceived;
        //    check.Start("http://www.feenux.com/trakhound/appinfo/th/bundle-appinfo.json");
        //}

        //void AutoUpdater_AppInfoReceived(UpdateCheck.AppInfo info)
        //{
        //    if (info != null)
        //    {
        //        // Print Auto Update info to Console
        //        Logger.Log("---- Auto-Update Info ----");
        //        Logger.Log("TrakHound - Client");
        //        Logger.Log("Release Type : " + info.releaseType);
        //        Logger.Log("Version : " + info.version);
        //        Logger.Log("Build Date : " + info.buildDate);
        //        Logger.Log("Download URL : " + info.downloadUrl);
        //        Logger.Log("Update URL : " + info.updateUrl);
        //        Logger.Log("File Size : " + info.size);
        //        Logger.Log("--------------------------");

        //        this.Dispatcher.BeginInvoke(new Action<UpdateCheck.AppInfo>(AutoUpdater_AppInfoReceived_GUI), new object[] { info });
        //    }
        //}

    }
}
