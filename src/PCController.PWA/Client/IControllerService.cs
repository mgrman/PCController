using PCController.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.PWA.Client
{
    public interface IControllerService
    {
        Task WakeOnLan();

        Task<bool> ValidatePin();

        Task InvokeCommandAsync( Command command, CancellationToken cancellationToken);
    }
}
