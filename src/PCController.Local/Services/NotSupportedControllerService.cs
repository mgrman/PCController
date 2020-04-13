﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public class NotSupportedControllerService : IControllerService
    {
        public bool IsPlatformSupported => false;

        public async Task InvokeCommandAsync(Command command, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}