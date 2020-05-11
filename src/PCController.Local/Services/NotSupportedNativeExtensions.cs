using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public class NotSupportedNativeExtensions : INativeExtensions
    {
        public bool IsPlatformSupported => false;

        public Task<bool> PingServer(RemoteServer server, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<string> GetMac(RemoteServer server, CancellationToken cancellationToken) => throw new NotImplementedException();

    }
}