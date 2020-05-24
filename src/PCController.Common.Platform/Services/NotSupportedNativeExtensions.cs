using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    internal class NotSupportedNativeExtensions : INativeExtensions
    {
        public bool IsPlatformSupported => false;

        public Task<bool> PingServerAsync(IPAddress server, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<bool> SendWolAsync(IPAddress server, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
