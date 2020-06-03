using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using Microsoft.Extensions.Options;
using PCController.Common.DataTypes;

namespace PCController.Local.Services
{
    internal class HttpServersProvider : IRemoteServersProvider
    {
        public HttpServersProvider(IOptions<Config> config, INativeExtensions nativeExtensions, HttpClient httpClient)
        {
            var list = config.Value.RemoteServers.Select(o => new HttpServer(o, httpClient, nativeExtensions))
                .ToArray();
            this.RemoteServers = Observable.Return(list);
        }

        public IObservable<IReadOnlyList<IRemoteServer>> RemoteServers { get; }

        public string ProviderName => "HttpServers";
    }
}