using PCController.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Topology;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PCController
{
    public enum PCStatus
    {
        Offline,
        DeviceOnline,
        ServerOnline,
        ValidPIN
    }

    public class ControllerService : INotifyPropertyChanged, IDisposable
    {
        private readonly ISyncLocalStorageService localStorageService;
        private readonly HttpClient httpClient;
        private readonly Ping pinger;
        private string baseAddress;
        private string macAddress;
        private string pin;
        private string errorMessage = string.Empty;
        private PCStatus status;
        private bool isDisposed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ControllerService(ISyncLocalStorageService localStorageService)
        {
            this.httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3)
            };
            this.localStorageService = localStorageService;
            macAddress = localStorageService.ContainKey(nameof(MacAddress)) ? localStorageService.GetItemAsString(nameof(MacAddress)) : "";
            baseAddress = localStorageService.ContainKey(nameof(BaseAddress)) ? localStorageService.GetItemAsString(nameof(BaseAddress)) : "";
            pin = localStorageService.ContainKey(nameof(PIN)) ? localStorageService.GetItemAsString(nameof(PIN)) : "";

            var cmds = Enum.GetValues(typeof(ControllerCommandType)).Cast<ControllerCommandType>()
                .Select(o => new LabeledCommand(o.ToString(), new Command(() => InvokeCommandInBackground(o))))
                .ToList();

            SupportedCommands = new[]
            {
                new LabeledCommand("WoL",new Command(() => WakeOnLan()))
            }
            .Concat(cmds)
            .ToList();

            pinger = new Ping();

            Task.Run(async () =>
           {
               while (!isDisposed)
               {
                   var start = DateTime.Now;

                   Status = await GetStatusAsync();

                   var time = TimeSpan.FromSeconds(3) - (DateTime.Now - start);
                   if (time.Ticks > 0)
                   {
                       await Task.Delay(time);
                   }
                   else
                   {
                       await Task.Yield();
                   }
               }
           });
        }

        private async Task<PCStatus> GetStatusAsync()
        {
            var ip = IPAddress.Parse(new Uri(BaseAddress, UriKind.Absolute).Host);
            var pingReply = pinger.Send(ip);
            if (pingReply.Status != IPStatus.Success)
            {
                return PCStatus.Offline;
            }
            try
            {
                var res = await ValidatePin();
                if (res)
                {
                    return PCStatus.ValidPIN;
                }
                else
                {
                    return PCStatus.ServerOnline;
                }
            }
            catch
            {
                return PCStatus.DeviceOnline;
            }
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
                InvokePropertyChanged(nameof(BaseAddress));

                Task.Run( async() =>
                {

                    await UpdateMacAddress();
                });
            }
        }

        private void InvokePropertyChanged(string name)
        {
            Device.InvokeOnMainThreadAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
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
                InvokePropertyChanged(nameof(MacAddress));
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
                InvokePropertyChanged(nameof(PIN));
            }
        }

        public PCStatus Status
        {
            get => status; set
            {
                if (status == value)
                {
                    return;
                }
                status = value;
                InvokePropertyChanged(nameof(Status));
            }
        }

        public IReadOnlyList<LabeledCommand> SupportedCommands { get; }

        public string ErrorMessage
        {
            get => errorMessage; private set
            {
                errorMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
        }

        private async Task UpdateMacAddress()
        {
            MacAddress = await GetFromJsonAsync<string>(Routes.MacAddressRoute);
        }

        private async void InvokeCommandInBackground(ControllerCommandType command)
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

        private async Task<bool> ValidatePin()
        {
            return await GetFromJsonAsync<bool>(Routes.CheckPinRout);
        }

        private void WakeOnLan()
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

                var mask = new NetMask(255, 255, 255, 0);
                var broadcastAddress = ip.GetBroadcastAddress(mask);

                IPAddress.Broadcast.SendWol(macAddress);
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
            using HttpResponseMessage response = await taskResponse.ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var text = await response.Content!.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TValue>(text);
        }

        private Task<HttpResponseMessage> PostAsJsonAsync(string? requestUri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + requestUri);
            request.Headers.Add(Routes.PinHeader, PIN);

            return httpClient.SendAsync(request, cancellationToken);
        }

        public void Dispose()
        {
            isDisposed = true;
            pinger.Dispose();
            httpClient.Dispose();
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