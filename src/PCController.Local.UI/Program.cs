using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PCController.Local.Services;
using Microsoft.AspNetCore.Components.Authorization;
using PCController.Local.UI.Services;
using Blazored.LocalStorage;
using Blazored.Modal;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Grpc.Net.Client;
using Grpc.Core;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace PCController.Local.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddBlazoredModal();

            builder.Services.AddApiAuthorization();

            builder.Services.AddScoped<IRemoteServersProvidersManager, RemoteServersProvidersManager>();
            builder.Services.AddScoped(services =>
            {
                // Create a gRPC-Web channel pointing to the backend server
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var baseUri = services.GetRequiredService<NavigationManager>().BaseUri;

                var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions
                {
                    HttpClient = httpClient,
                });

                // Now we can instantiate gRPC clients for this channel
                return new RemoteServersProviders.RemoteServersProvidersClient(channel);
            });

            await builder.Build().RunAsync();
        }
    }
}