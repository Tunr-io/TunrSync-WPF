using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TunrSync.Data.Models
{
    public class UploadTransferModel : INotifyPropertyChanged
    {
        public SyncAgent SyncAgent { get; set; }
        public FileInfo File { get; set; }
        public bool HasFailed { get; set; }
        public string FailureReason { get; set; }
        public int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }
        private int progress;
        public UploadTransferModel(SyncAgent syncAgent, FileInfo file)
        {
            SyncAgent = syncAgent;
            File = file;
            HasFailed = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Triggers upload of the file
        /// </summary>
        public async Task<bool> Upload()
        {
            ProgressMessageHandler progress = new ProgressMessageHandler();
            progress.HttpSendProgress += new EventHandler<HttpProgressEventArgs>(HttpSendProgress);
            HttpRequestMessage message = new HttpRequestMessage();
            MultipartFormDataContent content = new MultipartFormDataContent();

            try
            {
                FileStream filestream = new FileStream(File.FullName, FileMode.Open);
                var fileContent = new StreamContent(filestream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(fileContent, "file", File.Name);
                message.RequestUri = new Uri(App.BaseUrl + "/api/Library");
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SyncAgent.Configuration.Authentication.access_token);
                message.Method = HttpMethod.Post;
                message.Content = content;

                var client = HttpClientFactory.Create(progress);
                var result = await client.SendAsync(message);
                if (!result.IsSuccessStatusCode)
                {
                    HasFailed = true;
                    FailureReason = result.ReasonPhrase;
                    System.Diagnostics.Debug.WriteLine("Upload failed: " + FailureReason);
                    return false;
                }
            } catch (Exception e)
            {
                HasFailed = true;
                FailureReason = e.Message;
                System.Diagnostics.Debug.WriteLine("Upload failed: " + e.Message);
                return false;
            }
            return true;
        }

        private void HttpSendProgress(object sender, HttpProgressEventArgs e)
        {
            HttpRequestMessage request = sender as HttpRequestMessage;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Progress = e.ProgressPercentage;
            }));
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
