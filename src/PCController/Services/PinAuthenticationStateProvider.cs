using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PCController.Services
{
    public class PinAuthenticationStateProvider : AuthenticationStateProvider, IPinHandler
    {
        private readonly string _expectedPIN;
        private string _pin;

        private Task<AuthenticationState> _cachedState;

        public PinAuthenticationStateProvider(IConfig config)
        {
            _expectedPIN = config.PIN;

            _cachedState = Task.FromResult(GetState());
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
                _cachedState = Task.FromResult(GetState());

                this.NotifyAuthenticationStateChanged(_cachedState);
            }
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return _cachedState;
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