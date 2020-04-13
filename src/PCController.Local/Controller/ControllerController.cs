using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PCController.Local.Services;

namespace PCController.Local.Controller
{
    [ApiController]
    public class ControllerController : ControllerBase
    {
        public const string CommandWord = "command";
        public const string CommandPlaceholder = "{" + CommandWord + "}";
        public const string CommandRoute = "api/controller/" + CommandPlaceholder;
        public const string PinHeader = "pin";
        private readonly IControllerService _controllerService;
        private readonly Config _config;

        public ControllerController(IControllerService controllerService, IOptionsSnapshot<Config> config)
        {
            _controllerService = controllerService;
            _config = config.Value;
        }

        [Route(CommandRoute)]
        [HttpPost]
        public async Task<IActionResult> InvokeCommandAsync([FromRoute(Name = CommandWord)]Command command, [FromHeader(Name = PinHeader)]string pin, CancellationToken cancellationToken)
        {
            if (!_controllerService.IsPlatformSupported)
            {
                return NotFound("This server does not support local controlling!");
            }
            if (_config.PIN != pin)
            {
                return Unauthorized();
            }
            await _controllerService.InvokeCommandAsync(command, cancellationToken);
            return Ok();
        }
    }
}