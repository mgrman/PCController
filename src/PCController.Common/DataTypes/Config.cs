using System;
using System.Collections.Generic;

namespace PCController.Local
{
    public class Config
    {
        public string Name { get; set; }

        public string Pin { get; set; }

        public Uri Uri { get; set; }

        public IReadOnlyList<RemoteServerConfig> RemoteServers { get; set; }
    }
}
