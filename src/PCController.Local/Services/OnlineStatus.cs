using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public enum OnlineStatus
    {
        Unknown = 0,
        Offline,
        DeviceOnline,
        ServerOnline,
    }
}