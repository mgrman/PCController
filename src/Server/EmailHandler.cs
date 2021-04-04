using MailKit;
using MailKit.Net.Imap;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PCController.Services;
using PCController.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Server
{
    public class EmailHandler
    {
        private readonly IdleClient client;
        private readonly IControllerService controllerService;

        public EmailHandler(EmailConfig config, IControllerService controllerService)
        {
            this.controllerService = controllerService;
            client = new IdleClient(config, "GoogleHome");

            Task.Run(client.RunAsync);

            client.NewMessageArrived += MessageArrived;
        }

        private void MessageArrived(string subject, ref bool isHandled)
        {

            var cmdMatch = Regex.Match(subject, @"Command (\d*) (.*)");
            if (!cmdMatch.Success)
            {
                return;
            }

            var pin = cmdMatch.Groups[1].Value;
            if (!Enum.TryParse<ControllerCommandType>(cmdMatch.Groups[2].Value, out var commandType))
            {
                return;
            }

            isHandled = true;
            controllerService.InvokeCommandAsync(pin, commandType, CancellationToken.None);
        }
    }

    public static class EmailHandlerExtensions
    {
        private static List<EmailHandler> emailHandlers = new List<EmailHandler>();

        public static void ReceiveEmailCommands(this IApplicationBuilder builder)
        {
            var config = builder.ApplicationServices.GetService<IOptions<Config>>();

            if (string.IsNullOrEmpty(config.Value.Email.Email))
            {
                return;
            }

            if (string.IsNullOrEmpty(config.Value.Email.Imap.Host))
            {
                return;
            }

            var controllerService = builder.ApplicationServices.GetService<IControllerService>();

            var handler = new EmailHandler(config.Value.Email, controllerService);

            emailHandlers.Add(handler);
        }
    }
}
