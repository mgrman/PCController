using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface IControllerService
    {
        bool IsPlatformSupported { get; }

        Task InvokeCommandAsync(Command command, CancellationToken cancellationToken);
    }
}