using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCController.Local.Controller;

namespace PCController.Local.Services
{
    public interface IProcessHelper
    {
        Task<bool> StartProcessAndCheckExitCodeAsync(string path, string args, CancellationToken cancellationToken);

        Task<string> StartProcessAndReadOutAsync(string path, string args, CancellationToken cancellationToken);
    }
}
