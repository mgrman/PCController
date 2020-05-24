using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PCController.Http.Server.Controllers;
using PCController.Local.Services;

namespace PCController.Common
{
    public static class ServerExtensions
    {
        public static void AddControlViaHttp(this IServiceCollection services)
        {
            services.AddSingleton<IRemoteServersProvider, HttpServersProvider>();
            services.AddMvc()
                .AddApplicationPart(typeof(CommandsController).Assembly)
                .AddControllersAsServices();
        }

        public static void MapControlViaHttp(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapControllers();
        }
    }
}
