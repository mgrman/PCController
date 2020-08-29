using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using PCController.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.PWA.Client
{
    public class ControllerService : IControllerService
    {
        private HttpClientManager httpClient;
        private readonly ISyncLocalStorageService localStorageService;
        private readonly NavigationManager navigationManager;
        private readonly Task updateMacTask;
        private string macAddress;

        public ControllerService(HttpClientManager httpClient, ISyncLocalStorageService localStorageService, NavigationManager navigationManager)
        {
            this.httpClient = httpClient;
            this.localStorageService = localStorageService;
            this.navigationManager = navigationManager;
            updateMacTask= UpdateMacAddress();
        }

        private async Task UpdateMacAddress()
        {
            if (localStorageService.ContainKey(nameof(macAddress))){

                macAddress = localStorageService.GetItem<string>(nameof(macAddress));
            }
            else {
                try
                {
                    macAddress = await httpClient.GetFromJsonAsync<string>(Routes.MacAddressRoute);
                    localStorageService.SetItem(nameof(macAddress), macAddress);
                }
                catch
                {
                    macAddress = localStorageService.GetItem<string>(nameof(macAddress));
                }
            }
        }

        public async Task InvokeCommandAsync( Command command, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsJsonAsync(Routes.CommandRoute.Replace(Routes.CommandPlaceholder, command.ToString()), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(response.StatusCode.ToString());
            }
        }

        public async Task<bool> ValidatePin()
        {
                return await httpClient.GetFromJsonAsync<bool>(Routes.CheckPinRout);
        }

        public async Task WakeOnLan()
        {
            await updateMacTask;
            var baseUri = new Uri(navigationManager.BaseUri, UriKind.Absolute).Host;
            navigationManager.NavigateTo($"wol://aaa?ip={Uri.EscapeDataString(baseUri)}&mac={Uri.EscapeDataString(macAddress)}", true);
        }
    }
}
