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
    public class UploadTransferModel : TransferModel
    {
        public FileInfo File { get; set; }
        public new string Name {
            get
            {
                return File.Name;
            }
        }
        public UploadTransferModel(SyncAgent syncAgent, FileInfo file) : base(syncAgent)
        {
            File = file;
        }

        /// <summary>
        /// Triggers upload of the file
        /// </summary>
        public override async Task<bool> Start()
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
    }
}
