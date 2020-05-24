using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using PCController.Local.Services;

namespace PCController.Common.DataTypes
{
    public interface IRemoteServer
    {
        string MachineName { get; }

        IEnumerable<(string key, string value)> AdditionalInfo { get; }

        ISubject<string> Pin { get; }

        IObservable<OnlineStatus> IsOnline { get; }

        Task InvokeCommandAsync(Command command, CancellationToken cancellationToken);
    }
}
