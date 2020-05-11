using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local.Hubs
{
    public class StatusHub : Hub
    {
        public const string RelativePath = "/statusHub";
        public static readonly Uri RelativeUri = new Uri(RelativePath, UriKind.Relative);

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}