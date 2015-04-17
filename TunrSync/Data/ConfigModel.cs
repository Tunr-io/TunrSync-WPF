using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TunrSync.Data
{
    public class ConfigModel : INotifyPropertyChanged
    {
        [JsonIgnore]
        public static string ConfigPath
        {
            get
            {
                var pathConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".TunrSync");
                if (!Directory.Exists(pathConfig))
                {
                    Directory.CreateDirectory(pathConfig);
                }
                return Path.Combine(pathConfig, "config.json");
            }
        }

        /// <summary>
        /// Holds authentication data
        /// </summary>
        [JsonIgnore]
        public AuthResponseModel Authentication
        {
            get
            {
                return authentication;
            }
            set
            {
                if (authentication != value)
                {
                    authentication = value;
                    OnPropertyChanged("Authentication");
                }
            }
        }
        [JsonProperty]
        private AuthResponseModel authentication;

        /// <summary>
        /// Stores the path the user specified to synchronize
        /// </summary>
        [JsonIgnore]
        public string SyncPath
        {
            get
            {
                return syncPath;
            }
            set
            {
                if (syncPath != value)
                {
                    syncPath = value;
                    OnPropertyChanged("SyncPath");
                }
            }
        }
        [JsonProperty]
        private string syncPath;

        /// <summary>
        /// Whether or not the sync agent will upload new tracks
        /// </summary>
        [JsonIgnore]
        public bool DownloadEnabled
        {
            get
            {
                return downloadEnabled;
            }
            set
            {
                if (downloadEnabled != value)
                {
                    downloadEnabled = value;
                    OnPropertyChanged("DownloadEnabled");
                }
            }
        }
        [JsonProperty]
        private bool downloadEnabled;

        /// <summary>
        /// Whether or not the sync agent will download new tracks
        /// </summary>
        [JsonIgnore]
        public bool UploadEnabled
        {
            get
            {
                return uploadEnabled;
            }
            set
            {
                if (uploadEnabled != value)
                {
                    uploadEnabled = value;
                    OnPropertyChanged("UploadEnabled");
                }
            }
        }
        [JsonProperty]
        private bool uploadEnabled;

        /// <summary>
        /// Method to fire the PropertyChanged event when a property has changed in this class
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public ConfigModel()
        {
            syncPath = App.DefaultMusicPath;
            uploadEnabled = true;
            downloadEnabled = false;
        }
    }
}
