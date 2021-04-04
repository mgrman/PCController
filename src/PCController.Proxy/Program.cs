using Microsoft.Extensions.Configuration;
using PCController.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Proxy
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var env =  Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            IConfiguration configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables()
              .AddCommandLine(args)
              .Build();

            var config = new Config();
            configuration.GetSection("PCController").Bind(config);

            if (string.IsNullOrEmpty(config.Email.Email))
            {
                return;
            }

            if (string.IsNullOrEmpty(config.Email.Imap.Host))
            {
                return;
            }
            var handler = new EmailWoLHandler(config.Email);

            await handler.Run();
        }

    }
}
