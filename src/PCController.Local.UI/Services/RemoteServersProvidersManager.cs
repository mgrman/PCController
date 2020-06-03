using Google.Protobuf;
using Grpc.Core;
using PCController.Common.DataTypes;
using PCController.Common.Utilities;
using PCController.Local.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace PCController.Local.UI.Services
{
    public class RemoteServersProvidersManager : IRemoteServersProvidersManager, IDisposable
    {
        private readonly RemoteServersProviders.RemoteServersProvidersClient grpcClient;
        private readonly IAccessTokenProvider accessTokenProvider;
        private readonly CancellationTokenSource cts;
        private readonly Task backgroundTask;
        private readonly Dictionary<string, EdtitableRemoteServersProvider> currentState = new Dictionary<string, EdtitableRemoteServersProvider>();

        private readonly BehaviorSubject<IReadOnlyList<IRemoteServersProvider>> providers = new BehaviorSubject<IReadOnlyList<IRemoteServersProvider>>(Array.Empty<IRemoteServersProvider>());

        public RemoteServersProvidersManager(RemoteServersProviders.RemoteServersProvidersClient grpcClient, IAccessTokenProvider accessTokenProvider)
        {
            this.grpcClient = grpcClient;
            this.accessTokenProvider = accessTokenProvider;

            this.cts = new CancellationTokenSource();
            function();
        }

        IObservable<IReadOnlyList<IRemoteServersProvider>> IRemoteServersProvidersManager.Providers => providers;

        public void Dispose()
        {
            this.cts.Cancel();
            this.cts.Dispose();
        }

        private async Task<Metadata> GetHeaders()
        {
            var tokenResult = await accessTokenProvider.RequestAccessToken();

            if (tokenResult.TryGetToken(out var token))
            {
                return new Metadata()
                {
                    { "Authorization", $"Bearer {token.Value}" }
                };
            }
            else
            {
                return null;
            }
        }

        private async Task function()
        {
            using var asyncCall = this.grpcClient.GetRemoteProviderUpdates(new Empty(), await GetHeaders(), null, this.cts.Token);
            var responseStream = asyncCall.ResponseStream;
            while (await responseStream.MoveNext(this.cts.Token))
            {
                if (this.cts.Token.IsCancellationRequested)
                {
                    break;
                }
                var update = responseStream.Current;
                HandleUpdateAction(update);
            }
        }

        private void HandleUpdateAction(UpdateActionData update)
        {
            switch (update.Action)
            {
                case Action.Add:
                    {
                        if (!currentState.TryGetValue(update.ProviderId, out var provider))
                        {
                            provider = new EdtitableRemoteServersProvider(update.ProviderId, grpcClient, this.GetHeaders);
                            currentState[update.ProviderId] = provider;
                            providers.OnNext(currentState.Values.ToArray());
                        }
                        if (update.IsPinProtectedServer)
                        {
                            provider.AddPinProtectedServer(update.ServerId, update.AdditionalInfoJson, update.OnlineStatus, update.InitialPin);
                        }
                        else
                        {
                            provider.AddServer(update.ServerId, update.AdditionalInfoJson, update.OnlineStatus);
                        }
                    }
                    break;

                case Action.Update:
                    {
                        if (currentState.TryGetValue(update.ProviderId, out var provider))
                        {
                            provider.UpdateServer(update.ServerId, update.OnlineStatus);
                        }
                    }
                    break;

                case Action.Remove:
                    {
                        if (currentState.TryGetValue(update.ProviderId, out var provider))
                        {
                            provider.RemoveServer(update.ServerId);
                        }
                    }
                    break;
            }
        }

        public class EditablePinServer : EditableServer, IPinProtectedServer
        {
            private BehaviorSubject<string> pin;

            public EditablePinServer(string providerName, string machineName, string additionalInfoJson, OnlineStatus onlineStatus, string pin, RemoteServersProviders.RemoteServersProvidersClient grpcClient, Func<Task<Metadata>> getHeaders)
                : base(providerName, machineName, additionalInfoJson, onlineStatus, grpcClient, getHeaders)
            {
                this.pin = new BehaviorSubject<string>(pin);
            }

            public ISubject<string> Pin => pin;

            public override async Task InvokeCommandAsync(Command command, CancellationToken cancellationToken)
            {
                await this.grpcClient.InvokeCommandAsync(new InvokeCommandData()
                {
                    ProviderId = this.providerName,
                    ServerId = MachineName,
                    Command = command.ToString(),
                    Pin = pin.Value
                }, await getHeaders(), null, cancellationToken);
            }
        }

        public class EditableServer : IRemoteServer
        {
            protected readonly Func<Task<Metadata>> getHeaders;
            protected readonly string providerName;
            protected readonly RemoteServersProviders.RemoteServersProvidersClient grpcClient;
            protected ISubject<OnlineStatus> isOnline;

            public EditableServer(string providerName, string machineName, string additionalInfoJson, OnlineStatus onlineStatus, RemoteServersProviders.RemoteServersProvidersClient grpcClient, Func<Task<Metadata>> getHeaders)
            {
                this.providerName = providerName;
                MachineName = machineName;
                this.grpcClient = grpcClient;
                AdditionalInfo = JsonSerializer.Deserialize<Dictionary<string, string>>(additionalInfoJson);
                isOnline = new BehaviorSubject<OnlineStatus>(onlineStatus);
                this.getHeaders = getHeaders;
            }

            public string MachineName { get; }

            public IReadOnlyDictionary<string, string> AdditionalInfo { get; }

            public IObservable<OnlineStatus> IsOnline => isOnline;

            public void UpdateOnlineStatus(OnlineStatus onlineStatus)
            {
                isOnline.OnNext(onlineStatus);
            }

            public virtual async Task InvokeCommandAsync(Command command, CancellationToken cancellationToken)
            {
                await grpcClient.InvokeCommandAsync(new InvokeCommandData()
                {
                    ProviderId = providerName,
                    ServerId = MachineName,
                    Command = command.ToString()
                }, await this.getHeaders(), null, cancellationToken);
            }
        }

        private class EdtitableRemoteServersProvider : IRemoteServersProvider
        {
            private readonly Dictionary<string, EditableServer> currentState = new Dictionary<string, EditableServer>();
            private readonly Func<Task<Metadata>> getHeaders;
            private ISubject<IReadOnlyList<IRemoteServer>> remoteServers = new BehaviorSubject<IReadOnlyList<IRemoteServer>>(Array.Empty<IRemoteServer>());
            private RemoteServersProviders.RemoteServersProvidersClient grpcClient;

            public EdtitableRemoteServersProvider(string providerName, RemoteServersProviders.RemoteServersProvidersClient grpcClient, Func<Task<Metadata>> getHeaders)
            {
                ProviderName = providerName;
                this.grpcClient = grpcClient;
                this.getHeaders = getHeaders;
            }

            public string ProviderName { get; }
            IObservable<IReadOnlyList<IRemoteServer>> IRemoteServersProvider.RemoteServers => remoteServers;

            public void AddPinProtectedServer(string id, string additionalInfoJson, OnlineStatus onlineStatus, string initialPin)
            {
                EditableServer server = new EditablePinServer(ProviderName, id, additionalInfoJson, onlineStatus, initialPin, grpcClient, this.getHeaders);
                currentState[id] = server;
                remoteServers.OnNext(currentState.Values.ToArray());
            }

            public void AddServer(string id, string additionalInfoJson, OnlineStatus onlineStatus)
            {
                EditableServer server = new EditableServer(ProviderName, id, additionalInfoJson, onlineStatus, grpcClient, this.getHeaders);
                currentState[id] = server;
                remoteServers.OnNext(currentState.Values.ToArray());
            }

            public void UpdateServer(string id, OnlineStatus onlineStatus)
            {
                if (currentState.TryGetValue(id, out var server))
                {
                    server.UpdateOnlineStatus(onlineStatus);
                }
            }

            public void RemoveServer(string id)
            {
                currentState.Remove(id);
                remoteServers.OnNext(currentState.Values.ToArray());
            }
        }
    }
}