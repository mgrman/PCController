using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCController.Services;
using PCController.Shared;

namespace PCController.Server.Controllers
{

    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly Config config;
        private readonly IControllerService controllerService;
        private readonly ILogger<CommandsController> logger;

        public CommandsController(IControllerService controllerService, IOptions<Config> config, ILogger<CommandsController> logger)
        {
            this.controllerService = controllerService;
            this.logger = logger;
            this.config = config.Value;
        }

        [HttpGet]
        [Route(Routes.StatusRoute)]
        public void StatusCheck() { }

        [HttpGet]
        [Route(Routes.MacAddressRoute)]
        public IActionResult Mac()
        {
            var host = HttpContext.Request.Host.Host;
            const int MIN_MAC_ADDR_LENGTH = 12;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {

                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ipInfo in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            var ip = ipInfo.Address.ToString();
                            if (string.Equals(host,ip, System.StringComparison.OrdinalIgnoreCase))
                            {
                                string tempMac = nic.GetPhysicalAddress().ToString();
                                if (nic.Speed > maxSpeed &&
                                    !string.IsNullOrEmpty(tempMac) &&
                                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                                {
                                    logger.LogDebug("New Max Speed = " + nic.Speed + ", MAC: " + tempMac);
                                    maxSpeed = nic.Speed;
                                    return new JsonResult(tempMac);
                                }
                            }
                        }
                    }
                }
            }

            return new JsonResult("");

        }



        [HttpGet]
        [Route(Routes.CheckPinRout)]
        public IActionResult PinCheck([FromHeader(Name = Routes.PinHeader)]string pin) =>new JsonResult(pin == config.Pin);


        [Route(Routes.CommandRoute)]
        [HttpPost]
        public async Task<IActionResult> InvokeCommandAsync([FromRoute(Name = Routes.CommandWord)]
            Command command, [FromHeader(Name = Routes.PinHeader)]
            string pin, CancellationToken cancellationToken)
        {
            if (this.config.Pin != pin)
            {
                return this.Unauthorized();
            }

            await this.controllerService.InvokeCommandAsync(pin, command, cancellationToken);
            return this.Ok();
        }
    }
}
