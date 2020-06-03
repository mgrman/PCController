using System.Threading;
using System.Threading.Tasks;

namespace PCController.Common.DataTypes
{
    public interface INetworkAccessibleServer : IRemoteServer
    {
        Task WakeUpAsync(CancellationToken cancellationToken);
    }
}
