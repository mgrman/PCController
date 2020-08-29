using PCController.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Services
{
    public interface IControllerService
    {
        Task InvokeCommandAsync(string pin, Command command, CancellationToken cancellationToken);
    }
}
