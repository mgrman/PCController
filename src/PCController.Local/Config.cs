using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local
{
    public class Config
    {
        public string PIN { get; set; }

        public IReadOnlyList<RemoteServer> RemoteServers { get; set; }
    }
}