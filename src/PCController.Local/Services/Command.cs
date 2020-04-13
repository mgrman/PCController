using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public enum Command
    {
        Shutdown,

        Sleep,

        Lock,

        PlayMedia,

        PauseMedia,

        StopMedia,

        IncreaseVolume,

        DecreaseVolume,

        MuteVolume,
    }
}