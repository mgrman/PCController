using Blazored.LocalStorage;
using PCController.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.PWA.Client
{
    public class HttpClientManager
    {
        private readonly HttpClient httpClient;
        private readonly ISyncLocalStorageService localStorageService;

        public HttpClientManager(HttpClient httpClient, ISyncLocalStorageService localStorageService)
        {
            this.httpClient = httpClient;
            this.localStorageService = localStorageService;

            BaseAddress = localStorageService.ContainKey(nameof(BaseAddress)) ? localStorageService.GetItemAsString(nameof(BaseAddress)) : "";
            PIN = localStorageService.ContainKey(nameof(PIN)) ? localStorageService.GetItemAsString(nameof(PIN)) : "";
        }

        public string BaseAddress { get; set; }

        public string PIN { get; set; }

        public async Task<TValue> GetFromJsonAsync<TValue>( string? requestUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BaseAddress+requestUri);
            request.Headers.Add(Routes.PinHeader, PIN);

            Task<HttpResponseMessage> taskResponse = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            using (HttpResponseMessage response = await taskResponse.ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content!.ReadFromJsonAsync<TValue>( null, cancellationToken).ConfigureAwait(false);
            }
        }

        public  Task<HttpResponseMessage> PostAsJsonAsync(string? requestUri,CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + requestUri);
            request.Headers.Add(Routes.PinHeader, PIN);

            return httpClient.SendAsync(request, cancellationToken);

        }
    }
}
