using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using PCController.Local.Services;

namespace PCController.Common
{
    public static class ServerExtensions
    {
        public static void AddPlatformServices(this IServiceCollection services)
        {
            services.AddSingleton<IProcessHelper, ProcessHelper>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddSingleton<IControllerService, WindowsControllerService>();
                services.AddSingleton<INativeExtensions, WindowsNativeExtensions>();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                services.AddSingleton<IControllerService, LinuxControllerService>();
                services.AddSingleton<INativeExtensions, LinuxNativeExtensions>();
            }
            else
            {
                services.AddSingleton<IControllerService, NotSupportedControllerService>();
                services.AddSingleton<INativeExtensions, NotSupportedNativeExtensions>();
            }
        }
    }
}
