using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public class PinAuthenticationStateProvider : AuthenticationStateProvider, IPinHandler
    {
        private readonly string _expectedPIN;
        private readonly ILocalStorageService _localStorage;
        private string _pin;

        private Task<AuthenticationState> _cachedState;
        private bool _initialized;

        public PinAuthenticationStateProvider(IOptionsSnapshot<Config> config, ILocalStorageService localStorage)
        {
            _expectedPIN = config.Value.PIN;

            _cachedState = Task.FromResult(GetState());
            _localStorage = localStorage;
        }

        public string PIN
        {
            get => _pin;
            set
            {
                if (_pin == value)
                {
                    return;
                }
                _pin = value;
                UpdateStoredPin(value);

                _cachedState = Task.FromResult(GetState());
                this.NotifyAuthenticationStateChanged(_cachedState);
            }
        }

        public async Task<bool> InitializeJSAsync()
        {
            if (_initialized)
            {
                return false;
            }
            _initialized = true;
            PIN = await _localStorage.GetItemAsync<string>("pin");
            return true;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return _cachedState;
        }

        private async void UpdateStoredPin(string pin)
        {
            await _localStorage.SetItemAsync("pin", pin);
        }

        private AuthenticationState GetState()
        {
            if (_pin == _expectedPIN)
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "admin"),
                }, "pin");

                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            else
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
    }
}