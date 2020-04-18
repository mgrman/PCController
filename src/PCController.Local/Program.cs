using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PCController.Local
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var currentDirSegments = Environment.CurrentDirectory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    for (int i = 1; i < currentDirSegments.Length; i++)
                    {
                        var parentDir = Path.Combine(currentDirSegments.Take(i).ToArray());
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