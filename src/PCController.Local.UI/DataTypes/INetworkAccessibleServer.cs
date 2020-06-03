using System.Threading;
using System.Threading.Tasks;

namespace PCController.Common.DataTypes
{
    internal interface INetworkAccessibleServer : IRemoteServer
    {
        Task WakeUpAsync(CancellationToken cancellationToken);
    }
}
