using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface IProcessHelper
    {
        Task<bool> StartProcessAndCheckExitCodeAsync(string path, string args, CancellationToken cancellationToken);

        Task<string> StartProcessAndReadOutAsync(string path, string args, CancellationToken cancellationToken);
    }
}
