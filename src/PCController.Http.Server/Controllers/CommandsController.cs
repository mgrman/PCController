using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PCController.Local;
using PCController.Local.Services;

namespace PCController.Http.Server.Controllers
{
    [ApiController]
    public class CommandsController : ControllerBase
    {
        public const string CommandWord = "command";
        public const string CommandPlaceholder = "{" + CommandWord + "}";
        public const string StatusRoute = "api/status";
        public const string CommandRoute = "api/controller/" + CommandPlaceholder;
        public const string PinHeader = "pin";
        private readonly Config config;
        private readonly IControllerService controllerService;

        public CommandsController(IControllerService controllerService, IOptions<Config> config)
        {
            this.controllerService = controllerService;
            this.config = config.Value;
        }

        [HttpGet]
        [Route(StatusRoute)]
        public async Task<IActionResult> StatusCheck() => this.Ok();

        [HttpGet]
        [Route("/test")]
        // GET
        public IActionResult Index() => this.Ok("Hello");

        [Route(CommandRoute)]
        [HttpPost]
        public async Task<IActionResult> InvokeCommandAsync([FromRoute(Name = CommandWord)]
            Command command, [FromHeader(Name = PinHeader)]
            string pin, CancellationToken cancellationToken)
        {
            if (!this.controllerService.IsPlatformSupported)
            {
                return this.NotFound("This server does not support local controlling!");
            }

            if (this.config.Pin != pin)
            {
                return this.Unauthorized();
            }

            await this.controllerService.InvokeCommandAsync(pin, command, cancellationToken);
            return this.Ok();
        }
    }
}
