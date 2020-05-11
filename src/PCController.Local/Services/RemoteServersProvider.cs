using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCController.Local.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using PCController.Local.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace PCController.Local.Services
{
    public class RemoteServersProvider : IRemoteServersProvider
    {
        public RemoteServersProvider(HttpClient httpClient, IOptionsSnapshot<Config> config, INativeExtensions nativeExtensions)
        {
            RemoteServers = config.Value.RemoteServers.Select(o => new RemoteServer(o, nativeExtensions, httpClient))
                .ToArray();
        }

        public IReadOnlyList<RemoteServer> RemoteServers { get; }
    }
}
