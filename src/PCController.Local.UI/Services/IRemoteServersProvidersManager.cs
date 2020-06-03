using System;
using System.Collections.Generic;
using PCController.Common.DataTypes;

namespace PCController.Local.Services
{
    internal interface IRemoteServersProvidersManager
    {
        IObservable<IReadOnlyList<IRemoteServersProvider>> Providers { get; }
    }
}