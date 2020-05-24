using Microsoft.Extensions.DependencyInjection;
using PCController.Local.Services;

namespace PCController.Common
{
    public static class ServerExtensions
    {
        public static void AddSelfServer(this IServiceCollection services)
        {
            services.AddSingleton<IRemoteServersProvider, SelfServerProvider>();
        }
    }
}
