using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TunrSync.Data
{
    public class DataModel
    {
        /// <summary>
        /// Configuration class used for storing persistent application settings
        /// </summary>
        public ConfigModel Configuration
        {
            get
            {
                if (configuration == null)
                {
                    // Attempt load of configuration
                    if (File.Exists(ConfigModel.ConfigPath))
                    {
                        using (var sr = new StreamReader(ConfigModel.ConfigPath))
                        using (var jsonTextReader = new JsonTextReader(sr))
                        {
                            configuration = new JsonSerializer().Deserialize<ConfigModel>(jsonTextReader);
                        }
                    } else
                    {
                        configuration = new ConfigModel();
                    }
                    configuration.PropertyChanged += Configuration_PropertyChanged;
                }
                return configuration;
            }
        }
        private ConfigModel configuration;

        /// <summary>
        /// Sync agent used for actually running sync jobs
        /// </summary>
        public SyncAgent SyncAgent {
            get
            {
                if (syncAgent == null)
                {
                    syncAgent = new SyncAgent(Configuration);
                }
                return syncAgent;
            }
        }
        private SyncAgent syncAgent;

        /// <summary>
        /// Triggered when the configuration object has changed - save for persistence
        /// </summary>
        private void Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string json = JsonConvert.SerializeObject(Configuration, Formatting.Indented);
            File.WriteAllText(ConfigModel.ConfigPath, json);
        }

        public DataModel()
        {
        }
    }
}
