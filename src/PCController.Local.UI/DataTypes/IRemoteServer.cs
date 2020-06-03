using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PCController.Local;
using PCController.Local.Services;

namespace PCController.Common.DataTypes
{
    internal interface IRemoteServer
    {
        string MachineName { get; }

        IReadOnlyDictionary<string, string> AdditionalInfo { get; }

        IObservable<OnlineStatus> IsOnline { get; }

        Task InvokeCommandAsync(Command command, CancellationToken cancellationToken);
    }
}