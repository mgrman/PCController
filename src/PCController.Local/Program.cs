using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PCController.Local
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                Console.WriteLine($"{Environment.CurrentDirectory}");
                var currentDirSegments = Environment.CurrentDirectory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Console.WriteLine($"{currentDirSegments[0]}");
                for (var i = 1; i < currentDirSegments.Length; i++)
                {
                    var parentDir = string.Join(Path.DirectorySeparatorChar,
                        currentDirSegments.Take(i)
                            .ToArray());
                    var configPathInParentDir = Path.Combine(parentDir, "PCController.appConfig");

                    Console.WriteLine($"{File.Exists(configPathInParentDir)} {configPathInParentDir}");

                    config.AddJsonFile(configPathInParentDir, true);
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
