using PCController.Local.Services;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Common.DataTypes
{
    public interface IPinProtectedServer : IRemoteServer
    {
        string InitialPin { get; }

        Task InvokeCommandAsync(Command command, string pin, CancellationToken cancellationToken);
    }
}