using System.Collections.Generic;

namespace PCController.Local
{
    public interface IConfig
    {
        string PIN { get; set; }
        IReadOnlyList<RemoteServer> RemoteServers { get; set; }
    }
}