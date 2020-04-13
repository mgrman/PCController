using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local
{
    public class RemoteServer
    {
        public Uri Uri { get; private set; }
        public string PIN { get; private set; }
    }
}