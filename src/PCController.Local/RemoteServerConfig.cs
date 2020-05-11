using Microsoft.AspNetCore.SignalR.Client;
using PCController.Local.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local
{
    public class RemoteServerConfig
    {
        public Uri Uri { get; set; }
        public string PIN { get; set; }
    }
}