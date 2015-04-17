using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TunrSync.Data;

namespace TunrSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string BaseUrl = "https://play.tunr.io";

        public static string DefaultMusicPath
        {
            get
            {
                string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                return Path.Combine(homePath, "Music");
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if ((Resources["DataModel"] as DataModel).Configuration.Authentication == null)
            {
                LogInWindow logInWindow = new LogInWindow();
                logInWindow.Show();
            } else
            {
                SyncWindow syncWindow = new SyncWindow();
                syncWindow.Show();
            }
        }
    }
}
