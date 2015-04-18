using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TunrSync.Data.SampleData
{
    class SampleDataModel : DataModel
    {
        public SampleDataModel() : base()
        {
            this.SyncAgent.IsSyncing = true;
            this.SyncAgent.CurrentUploads.Add(new Models.UploadTransferModel(this.SyncAgent, new System.IO.FileInfo("C:\\Users\\Hayden\\OneDrive\\Music\\38 Special\\Anthology\\Hold On Loosely.mp3")));
            this.SyncAgent.CurrentUploads.Add(new Models.UploadTransferModel(this.SyncAgent, new System.IO.FileInfo("C:\\Users\\Hayden\\OneDrive\\Music\\38 Special\\Flashback\\Caught Up In You.mp3")));
        }
    }
}
