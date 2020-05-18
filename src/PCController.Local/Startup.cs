using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PCController.Local.Hubs;
using PCController.Local.Services;

namespace PCController.Local
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddSignalR();
            services.AddServerSideBlazor();
            services.AddBlazoredLocalStorage();

            services.AddScoped<PinAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider, PinAuthenticationStateProvider>(c => c.GetRequiredService<PinAuthenticationStateProvider>());
            services.AddScoped<IPinHandler, PinAuthenticationStateProvider>(c => c.GetRequiredService<PinAuthenticationStateProvider>());

            services.AddHttpClient();
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

            services.AddSingleton<IProcessHelper, ProcessHelper>();

            services.AddSingleton<IRemoteServersProvider, RemoteServersProvider>();

            services.AddSingleton<ISignalRManager, SignalRManager>();
            services.Configure<Config>(Configuration.GetSection("PCController"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var config = app.ApplicationServices.GetService<IOptions<Config>>();
            if (string.IsNullOrEmpty(config.Value.Name))
            {
                throw new InvalidOperationException("Missing ID in configuration!!!");
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<StatusHub>(StatusHub.RelativePath);
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}