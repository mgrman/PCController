using Microsoft.Extensions.DependencyInjection;
using PCController.Local.Services;

namespace PCController.Common
{
    public static class ServerExtensions
    {
        public static void AddSignalRClient(this IServiceCollection services)
        {
            services.AddSingleton<IRemoteServersProvider, SignalRClientToHubConnectionServersProvider>();
        }
    }
}
