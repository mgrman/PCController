using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface INativeExtensions
    {
        bool IsPlatformSupported { get; }

        Task<bool> PingServerAsync(IPAddress server, CancellationToken cancellationToken);

        Task<bool> SendWolAsync(IPAddress server, CancellationToken cancellationToken);
    }
}
