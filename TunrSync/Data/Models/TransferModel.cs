using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TunrSync.Data.Models
{
    public abstract class TransferModel : INotifyPropertyChanged
    {
        public SyncAgent SyncAgent { get; set; }
        public bool HasFailed { get; set; }
        public string FailureReason { get; set; }
        public string Name { get; set; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        public TransferModel(SyncAgent syncAgent)
        {
            SyncAgent = syncAgent;
            HasFailed = false;
        }

        public abstract Task<bool> Start();

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
