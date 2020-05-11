using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface INativeExtensions
    {
        bool IsPlatformSupported { get; }

        Task<bool> PingServer(RemoteServer server, CancellationToken cancellationToken);

        Task<string> GetMac(RemoteServer server, CancellationToken cancellationToken);
    }
}