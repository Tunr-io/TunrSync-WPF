using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TunrSync.Data
{
    public class AuthResponseModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public long expires_in { get; set; }
        public string userName { get; set; }
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
    }
}
