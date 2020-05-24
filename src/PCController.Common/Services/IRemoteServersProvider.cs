using System;
using System.Collections.Generic;
using PCController.Common.DataTypes;

namespace PCController.Local.Services
{
    public interface IRemoteServersProvider
    {
        IObservable<IReadOnlyList<IRemoteServer>> RemoteServers { get; }
    }
}
