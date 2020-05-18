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
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public Uri Uri { get; set; }
        public string PIN { get; set; }
    }
}