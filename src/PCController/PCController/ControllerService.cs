using PCController.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PCController
{
    public class ControllerService : INotifyPropertyChanged
    {
        private readonly ISyncLocalStorageService localStorageService;
        private string baseAddress;
        private string macAddress;
        private string pin;
        private string errorMessage;
        private readonly HttpClient httpClient;

        public event PropertyChangedEventHandler PropertyChanged;

        public ControllerService(ISyncLocalStorageService localStorageService, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.localStorageService = localStorageService;
            macAddress = localStorageService.ContainKey(nameof(MacAddress)) ? localStorageService.GetItemAsString(nameof(MacAddress)) : "";
            baseAddress = localStorageService.ContainKey(nameof(BaseAddress)) ? localStorageService.GetItemAsString(nameof(BaseAddress)) : "";
            pin = localStorageService.ContainKey(nameof(PIN)) ? localStorageService.GetItemAsString(nameof(PIN)) : "";

            var cmds = Enum.GetValues(typeof(ControllerCommandType)).Cast<ControllerCommandType>()
                .Select(o => new LabeledCommand(o.ToString(), new Command(() => InvokeCommandAsync(o))))
                .ToList();


            SupportedCommands = new[]
            {
                new LabeledCommand("WoL",new Command(() => WakeOnLan()))
            }
            .Concat(cmds)
            .ToList();
        }

        public string BaseAddress
        {
            get => baseAddress; set
            {
                if (baseAddress == value)
                {
                    return;
                }
                baseAddress = value;
                localStorageService.SetItem(nameof(BaseAddress), value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BaseAddress)));

                UpdateMacAddress();
            }
        }

        public string MacAddress
        {
            get => macAddress; set
            {
                if (macAddress == value)
                {
                    return;
                }
                macAddress = value;
                localStorageService.SetItem(nameof(MacAddress), value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MacAddress)));
            }
        }

        public string PIN
        {
            get => pin; set
            {
                if (pin == value)
                {
                    return;
                }
                pin = value;
                localStorageService.SetItem(nameof(PIN), value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PIN)));
            }
        }

        public IReadOnlyList<LabeledCommand> SupportedCommands { get; }
        public string ErrorMessage { get => errorMessage; private set
            {
                errorMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
        }

        private async Task UpdateMacAddress()
        {
                MacAddress = await GetFromJsonAsync<string>(Routes.MacAddressRoute);
          
        }

        public async Task InvokeCommandAsync(ControllerCommandType command)
        {
            ErrorMessage = "";
            try
            {
                var response = await PostAsJsonAsync(Routes.CommandRoute.Replace(Routes.CommandPlaceholder, command.ToString()), CancellationToken.None);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(response.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public async Task<bool> ValidatePin()
        {
            return await GetFromJsonAsync<bool>(Routes.CheckPinRout);
        }

        public  void WakeOnLan()
        {
            ErrorMessage = "";
            try
            {
                var ip = IPAddress.Parse(new Uri(BaseAddress, UriKind.Absolute).Host);
                PhysicalAddress macAddress;
                if (string.IsNullOrEmpty(MacAddress))
                {
                    ArpRequestResult res = ArpRequest.Send(ip);
                    if (res.Exception != null)
                    {
                        ExceptionDispatchInfo.Capture(res.Exception).Throw();
#pragma warning disable CS8597 // Never happens due to ExceptionDispatchInfo throwing.
                        throw null;
#pragma warning restore CS8597
                    }
                    else
                    {
                        macAddress = res.Address;

                    }
                }
                else
                {
                    macAddress = PhysicalAddress.Parse(MacAddress);
                }


                macAddress.SendWol(ip);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
        private async Task<TValue> GetFromJsonAsync<TValue>(string? requestUri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + requestUri);
            request.Headers.Add(Routes.PinHeader, PIN);

            Task<HttpResponseMessage> taskResponse = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            using (HttpResponseMessage response = await taskResponse.ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                var text = await response.Content!.ReadAsStringAsync();

                return JsonSerializer.Deserialize<TValue>(text);
            }
        }

        private Task<HttpResponseMessage> PostAsJsonAsync(string? requestUri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + requestUri);
            request.Headers.Add(Routes.PinHeader, PIN);

            return httpClient.SendAsync(request, cancellationToken);

        }
    }

    public class LabeledCommand
    {
        public LabeledCommand(string label, ICommand command)
        {
            Label = label;
            Command = command;
        }

        public string Label { get; }
        public ICommand Command { get; }
    }
}
