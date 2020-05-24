using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace PCController.Local.Services
{
    public class PinAuthenticationStateProvider : AuthenticationStateProvider, IPinHandler
    {
        private readonly string expectedPin;
        private readonly ILocalStorageService localStorage;

        private Task<AuthenticationState> cachedState;
        private bool initialized;
        private string pin;

        public PinAuthenticationStateProvider(IOptionsSnapshot<Config> config, ILocalStorageService localStorage)
        {
            this.expectedPin = config.Value.Pin;

            this.cachedState = Task.FromResult(this.GetState());
            this.localStorage = localStorage;
        }

        public string Pin
        {
            get => this.pin;
            set
            {
                if (this.pin == value)
                {
                    return;
                }

                this.pin = value;
                this.UpdateStoredPin(value);

                this.cachedState = Task.FromResult(this.GetState());
                this.NotifyAuthenticationStateChanged(this.cachedState);
            }
        }

        public async Task InitializeJsAsync()
        {
            if (this.initialized)
            {
                return;
            }

            this.initialized = true;
            this.Pin = await this.localStorage.GetItemAsync<string>("pin");
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() => this.cachedState;

        private async void UpdateStoredPin(string pin)
        {
            await this.localStorage.SetItemAsync("pin", pin);
        }

        private AuthenticationState GetState()
        {
            if (this.pin == this.expectedPin)
            {
                var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "admin")
                    },
                    "pin");

                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
}
