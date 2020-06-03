using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PCController.Common.DataTypes;
using PCController.Local.Services;
using PCController.Common.Utilities;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace PCController.Local
{
    [Authorize]
    public partial class RemoteServersProvidersService : RemoteServersProviders.RemoteServersProvidersBase
    {
        private readonly IEnumerable<IRemoteServersProvider> providers;

        public RemoteServersProvidersService(IEnumerable<IRemoteServersProvider> providers)
        {
            this.providers = providers;
        }

        public override async Task GetRemoteProviderUpdates(Empty request, IServerStreamWriter<UpdateActionData> responseStream, ServerCallContext context)
        {
            List<IDisposable> disposables = new List<IDisposable>();
            Dictionary<IRemoteServer, IDisposable> serversSubs = new Dictionary<IRemoteServer, IDisposable>();

            foreach (var provider in providers)
            {
                provider.RemoteServers
                    .ToSetChanges()
                    .Subscribe(serversChange =>
                    {
                        switch (serversChange.Action)
                        {
                            case ChangeAction.Added:
                                {
                                    responseStream.WriteAsync(new UpdateActionData()
                                    {
                                        Action = Action.Add,
                                        ProviderId = provider.ProviderName,
                                        ServerId = serversChange.Value.MachineName,
                                        AdditionalInfoJson = JsonSerializer.Serialize(serversChange.Value.AdditionalInfo),
                                        OnlineStatus = OnlineStatus.Unknown,
                                        IsPinProtectedServer = serversChange.Value is IPinProtectedServer,
                                        InitialPin = (serversChange.Value as IPinProtectedServer)?.InitialPin ?? string.Empty
                                    });

                                    var sub = serversChange.Value.IsOnline.Subscribe(isOnline =>
                                    {
                                        responseStream.WriteAsync(new UpdateActionData()
                                        {
                                            Action = Action.Update,
                                            ProviderId = provider.ProviderName,
                                            ServerId = serversChange.Value.MachineName,
                                            OnlineStatus = isOnline
                                        });
                                    });
                                    serversSubs[serversChange.Value] = sub;
                                }
                                break;

                            case ChangeAction.Removed:
                                {
                                    responseStream.WriteAsync(new UpdateActionData()
                                    {
                                        Action = Action.Remove,
                                        ProviderId = provider.ProviderName,
                                        ServerId = serversChange.Value.MachineName,
                                        AdditionalInfoJson = null,//JsonSerializer.Serialize(serversChange.Value.AdditionalInfo)
                                        OnlineStatus = OnlineStatus.Unknown
                                    });
                                    if (serversSubs.Remove(serversChange.Value, out var sub))
                                    {
                                        sub.Dispose();
                                    }
                                }
                                break;

                            default:
                                throw new InvalidOperationException();
                        }
                    })
                    .TrackSubscription(disposables);
            }

            await context.CancellationToken.WhenCanceled();
            disposables.DisposeAll();
            serversSubs.Values.DisposeAll();
        }

        public override async Task<Empty> InvokeCommand(InvokeCommandData request, ServerCallContext context)
        {
            var provider = providers.FirstOrDefault(p => p.ProviderName == request.ProviderId);
            if (provider == null)
            {
                throw new InvalidOperationException();
            }

            var servers = await provider.RemoteServers.ToTask();
            if (servers == null)
            {
                throw new InvalidOperationException();
            }
            var server = servers.FirstOrDefault(s => s.MachineName == request.ServerId);
            if (server == null)
            {
                throw new InvalidOperationException();
            }

            if (server is IPinProtectedServer pinProtectedServer)
            {
                await pinProtectedServer.InvokeCommandAsync(Enum.Parse<Command>(request.Command), request.Pin, context.CancellationToken);
            }
            else
            {
                await server.InvokeCommandAsync(Enum.Parse<Command>(request.Command), context.CancellationToken);
            }
            return new Empty();
        }
    }
}