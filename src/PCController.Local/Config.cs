using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local
{
    public class Config
    {
        public string ID { get; set; }

        public string PIN { get; set; }

        public Uri Uri { get; set; }

        public IReadOnlyList<RemoteServerConfig> RemoteServers { get; set; }
    }
}