using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PCController.Local.Hubs;
using PCController.Local.Services;
using PCController.SignalR.Common;

namespace PCController.Common
{
    public static class ServerExtensions
    {
        public static void AddSignalRServer(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<SignalRHubToClientConnectionServersProvider>();
            services.AddSingleton<ISignalRManager>(o => o.GetRequiredService<SignalRHubToClientConnectionServersProvider>());
            services.AddSingleton<IRemoteServersProvider>(o=>o.GetRequiredService<SignalRHubToClientConnectionServersProvider>());
        }

        public static void MapSignalRServer(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHub<StatusHub>(SignalRConfig.RelativePath);
        }
    }
}
