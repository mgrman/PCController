using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface IRemoteServersProvider
    {
        IReadOnlyList<RemoteServer> RemoteServers { get; }
    }
}