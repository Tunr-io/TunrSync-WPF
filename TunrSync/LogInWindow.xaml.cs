using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using TunrSync.Data;

namespace TunrSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LogInWindow : Window, INotifyPropertyChanged
    {
        private bool isProcessing = false;
        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            set
            {
                if (isProcessing != value)
                {
                    isProcessing = value;
                    OnPropertyChanged("IsProcessing");
                }
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public LogInWindow()
        {
            InitializeComponent();
        }

        private async void ButtonLogIn_Click(object sender, RoutedEventArgs e)
        {
            IsProcessing = true;
            using (var client = new HttpClient())
            {
                // New code:
                client.BaseAddress = new Uri(App.BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", TextBoxEmail.Text),
                        new KeyValuePair<string, string>("password", TextBoxPass.Password)
                    });
                HttpResponseMessage response = await client.PostAsync("/Token", content);
                if (response.IsSuccessStatusCode)
                {
                    var authString = await response.Content.ReadAsStringAsync();
                    var auth = JsonConvert.DeserializeObject<AuthResponseModel>(authString);
                    (DataContext as DataModel).Configuration.Authentication = auth;
                    IsProcessing = false;
                    (new SyncWindow()).Show();
                    this.Close();
                }
                else
                {
                    IsProcessing = false;
                    MessageBox.Show("We couldn't log in with those credentials. Please try again.", "Couldn't log in");
                }
            }
        }
    }
}
