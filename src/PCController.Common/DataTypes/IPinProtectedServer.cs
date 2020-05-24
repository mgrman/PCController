using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using PCController.Local.Services;

namespace PCController.Common.DataTypes
{
    public interface IPinProtectedServer: IRemoteServer
    {
        ISubject<string> Pin { get; }
    }
}
