using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TunrSync.Data;
using TunrSync.Data.Models;

namespace TunrSync
{
    public class SyncAgent : INotifyPropertyChanged
    {
        /// <summary>
        /// Number of bytes to hash from the beginning each music file
        /// </summary>
        public const int Md5Size = 128 * 1024;

        /// <summary>
        /// Extensions supported by Tunr for upload
        /// </summary>
        public readonly string[] SupportedExtensions = { "*.mp3", "*.ogg", "*.m4a", "*.flac" };

        /// <summary>
        /// Number of threads used to upload / download
        /// </summary>
        private const int ThreadCount = 4;

        /// <summary>
        /// Number of threads used for indexing
        /// </summary>
        private const int IndexThreadCount = 12;

        /// <summary>
        /// Stores the configuration used to sync
        /// </summary>
        public readonly ConfigModel Configuration;

        /// <summary>
        /// Whether or not this sync agent is currently syncing
        /// </summary>
        public bool IsSyncing
        {
            get
            {
                return isSyncing;
            }
            set
            {
                if (isSyncing != value)
                {
                    isSyncing = value;
                    OnPropertyChanged("IsSyncing");
                }
            }
        }
        private bool isSyncing;

        /// <summary>
        /// Maintains a status string displayed to the user
        /// </summary>
        public string StatusMessage
        {
            get
            {
                return statusMessage;
            }
            set
            {
                if (statusMessage != value)
                {
                    statusMessage = value;
                    OnPropertyChanged("StatusMessage");
                }
            }
        }
        private string statusMessage;

        #region Progress properties
        public int Progress
        {
            get
            {
                int numerator = FilesIndexed + FilesUploaded + FilesDownloaded;
                int denominator = FilesToIndex + FilesToUpload + FilesToDownload;
                if (denominator <= 0)
                {
                    return 0;
                }
                return (int)((numerator / (double)denominator)*100);
            }
        }

        private int FilesIndexed
        {
            get
            {
                return filesIndexed;
            }
            set
            {
                if (filesIndexed != value)
                {
                    filesIndexed = value;
                    OnPropertyChanged("FilesIndexed");
                    OnPropertyChanged("Progress");
                }
            }
        }
        private int filesIndexed;

        private int FilesToIndex
        {
            get
            {
                return filesToIndex;
            }
            set
            {
                if (filesToIndex != value)
                {
                    filesToIndex = value;
                    OnPropertyChanged("FilesToIndex");
                    OnPropertyChanged("Progress");
                }
            }
        }
        private int filesToIndex;

        private int FilesUploaded
        {
            get
            {
                return filesUploaded;
            }
            set
            {
                if (filesUploaded != value)
                {
                    filesUploaded = value;
                    OnPropertyChanged("FilesUploaded");
                    OnPropertyChanged("Progress");
                }
            }
        }
        private int filesUploaded;

        private int FilesToUpload
        {
            get
            {
                return filesToUpload;
            }
            set
            {
                if (filesToUpload != value)
                {
                    filesToUpload = value;
                    OnPropertyChanged("FilesToUpload");
                    OnPropertyChanged("Progress");
                }
            }
        }
        private int filesToUpload;

        private int FilesDownloaded
        {
            get
            {
                return filesDownloaded;
            }
            set
            {
                if (filesDownloaded != value)
                {
                    filesDownloaded = value;
                    OnPropertyChanged("FilesDownloaded");
                    OnPropertyChanged("Progress");
                }
            }
        }
        private int filesDownloaded;

        private int FilesToDownload
        {
            get
            {
                return filesToDownload;
            }
            set
            {
                if (filesToDownload != value)
                {
                    filesToDownload = value;
                    OnPropertyChanged("FilesToDownload");
                    OnPropertyChanged("Progress");
                }
            }
        }
        private int filesToDownload;

        #endregion

        /// <summary>
        /// List of currently running uploads
        /// </summary>
        public ObservableCollection<TransferModel> CurrentTransfers { get; set; }

        /// <summary>
        /// Event handler for property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public SyncAgent(ConfigModel config)
        {
            Configuration = config;
            CurrentTransfers = new ObservableCollection<TransferModel>();
        }

        /// <summary>
        /// Starts the synchronization process
        /// </summary>
        public async void Sync()
        {
            IsSyncing = true;
            StatusMessage = "Preparing to sync...";

            // Clear current upload list
            CurrentTransfers.Clear();

            // Fetch local and remote libraries
            StatusMessage = "Scanning local music directory ...";
            var localLibrary = await ScanLibrary();
            StatusMessage = "Found " + localLibrary.Keys.Count + " candidate files";

            StatusMessage = "Fetching Tunr cloud library ...";
            var cloudLibrary = await FetchLibrary();
            StatusMessage = "Found " + cloudLibrary.Keys.Count + " tracks on Tunr cloud";

            // Compare libraries to figure out what to upload / download
            var toDownload = cloudLibrary.Keys.Except(localLibrary.Keys);
            var toUpload = localLibrary.Keys.Except(cloudLibrary.Keys);

            // Upload missing files
            StatusMessage = "Uploading new music to Tunr cloud ...";
            var filesToUpload = localLibrary.Where(v => toUpload.Contains(v.Key)).Select(v => v.Value).ToList();
            await UploadFiles(filesToUpload);

            StatusMessage = "Sync complete";
            IsSyncing = false;
        }

        /// <summary>
        /// Scans the local library for upload candidates
        /// </summary>
        /// <returns>Dictionary of hash to file information for upload candidates</returns>
        private async Task<Dictionary<string, FileInfo>> ScanLibrary()
        {
            StatusMessage = "Scanning local directory ...";
            var directory = new DirectoryInfo(Configuration.SyncPath);
            var files = SupportedExtensions.AsParallel().SelectMany(searchPattern =>
                directory.EnumerateFiles(searchPattern,
                    SearchOption.AllDirectories)).ToList();
            var localIndex = new ConcurrentDictionary<string, FileInfo>();
            FilesToIndex = files.Count;
            FilesIndexed = 0;

            List<FileInfo>[] indexList = new List<FileInfo>[IndexThreadCount];
            for (int i = 0; i < IndexThreadCount; i++)
            {
                indexList[i] = new List<FileInfo>();
            }
            for (int i = 0; i < files.Count; i++)
            {
                indexList[i % IndexThreadCount].Add(files[i]);
            }

            await Task.WhenAll(indexList.Select(l => Task.Factory.StartNew(() => {
                foreach (var file in l)
                {
                    var hash = AudioHash.HashAudio(file.FullName);
                    if (!localIndex.ContainsKey(hash))
                    {
                        localIndex.TryAdd(hash, file);
                    }
                    FilesIndexed++;
                }
            })));

            StatusMessage = "Found " + FilesIndexed + " candidate files";
            return new Dictionary<string, FileInfo>(localIndex);
        }

        /// <summary>
        /// Fetches the user's Tunr library
        /// </summary>
        /// <returns>Dictionary of MD5 hash to library tracks</returns>
        private async Task<Dictionary<string, Song>> FetchLibrary()
        {
            using (var client = new HttpClient())
            {
                // New code:
                client.BaseAddress = new Uri(App.BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration.Authentication.access_token);
                StringBuilder urlparams = new StringBuilder();
                var library_request = await client.GetAsync("api/Library");
                if (!library_request.IsSuccessStatusCode)
                {
                    throw new ApplicationException("Could not fetch library.");
                }
                var responseString = await library_request.Content.ReadAsStringAsync();
                var songs = JsonConvert.DeserializeObject<List<Song>>(responseString);
                return songs.GroupBy(s => s.AudioMd5Hash).ToDictionary(k => k.Key, k => k.First());
            }
        }

        /// <summary>
        /// Uploads a list of files to Tunr
        /// </summary>
        private async Task UploadFiles(List<FileInfo> files)
        {
            // Update progress count
            FilesToUpload = files.Count;
            FilesUploaded = 0;

            // Initialize a list for each thread
            List<FileInfo>[] threadLists = new List<FileInfo>[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                threadLists[i] = new List<FileInfo>();
            }

            // Evenly distribute files
            for (int i = 0; i < files.Count; i++)
            {
                threadLists[i % ThreadCount].Add(files[i]);
            }

            // Start threads
            int totalNumber = files.Count;
            int successCount = 0;
            int failCount = 0;
            await Task.WhenAll(threadLists.Select(l => Task.Run(async () => {
                foreach (var file in l)
                {
                    try
                    {
                        var newUpload = new UploadTransferModel(this, file);
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            CurrentTransfers.Add(newUpload);
                        }));
                        var result = await newUpload.Start();
                        if (!result)
                        {
                            failCount++;
                            FilesUploaded++;
                            continue;
                        }
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            CurrentTransfers.Remove(newUpload);
                        }));
                    }
                    catch (Exception)
                    {
                        failCount++;
                        FilesUploaded++;
                        continue;
                    }
                    successCount++;
                    FilesUploaded++;
                }
            })));
        }

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
    }
}
