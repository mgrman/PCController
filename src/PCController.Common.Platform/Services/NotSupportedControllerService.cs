using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    internal class NotSupportedControllerService : IControllerService
    {
        public bool IsPlatformSupported => false;

        public Task InvokeCommandAsync(string pin, Command command, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
