using System;
using System.Collections.Generic;

namespace PCController.Local.Services
{
    public interface ISignalRManager
    {
        event Action OnlineStatusChanged;

        IEnumerable<string> ConnectedIds { get; }

        void AddClient(string id, string requestUri);

        bool IsConnected(string machineID);

        void RemoveClient(string id);
    }
}