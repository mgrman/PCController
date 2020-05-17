using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public class SignalRManager : ISignalRManager
    {
        private ConcurrentDictionary<string, string> _connectedIds = new ConcurrentDictionary<string, string>();

        public event Action OnlineStatusChanged;

        public IEnumerable<string> ConnectedIds => _connectedIds.Values;

        public bool IsConnected(string machineID)
        {
            return _connectedIds.Values.Contains(machineID);
        }

        public void AddClient(string id, string requestUri)
        {
            _connectedIds[id] = requestUri;
            OnlineStatusChanged?.Invoke();
        }

        public void RemoveClient(string id)
        {
            if (!_connectedIds.Remove(id, out _))
            {
                //throw new InvalidOperationException($"Disconnected client {id} which was never connected!");
            }
            OnlineStatusChanged?.Invoke();
        }
    }
}