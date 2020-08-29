using PCController.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Services
{
    public interface IControllerService
    {
        Task WakeOnLan();

        Task<bool> ValidatePin();

        Task InvokeCommandAsync( Command command, CancellationToken cancellationToken);
    }
}
