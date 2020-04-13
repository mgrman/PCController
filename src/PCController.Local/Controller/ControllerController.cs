using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PCController.Local.Services;

namespace PCController.Local.Controller
{
    [ApiController]
    public class ControllerController : ControllerBase
    {
        public const string LockRoute = "api/controller/lock";
        public const string SleepRoute = "api/controller/sleep";
        public const string ShutdownRoute = "api/controller/shutdown";
        public const string PinHeader = "pin";
        private readonly IControllerService _controllerService;
        private readonly Config _config;

        public ControllerController(IControllerService controllerService, Config config)
        {
            _controllerService = controllerService;
            _config = config;
        }

        [Route(LockRoute)]
        [HttpPost]
        public IActionResult Lock([FromHeader(Name = PinHeader)]string pin)
        {
            if (_config.PIN != pin)
            {
                return Unauthorized();
            }
            _controllerService.Lock();
            return Ok();
        }

        [Route(SleepRoute)]
        [HttpPost]
        public IActionResult Sleep([FromHeader(Name = PinHeader)]string pin)
        {
            if (_config.PIN != pin)
            {
                return Unauthorized();
            }
            _controllerService.Sleep();
            return Ok();
        }

        [Route(ShutdownRoute)]
        [HttpPost]
        public IActionResult Shutdown([FromHeader(Name = PinHeader)]string pin)
        {
            if (_config.PIN != pin)
            {
                return Unauthorized();
            }
            _controllerService.Shutdown();
            return Ok();
        }
    }
}