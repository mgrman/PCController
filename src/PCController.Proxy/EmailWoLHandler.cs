using MailKit;
using MailKit.Net.Imap;
using PCController.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Topology;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Server
{
    public class EmailWoLHandler
    {
        private readonly IdleClient client;

        public EmailWoLHandler(EmailConfig config)
        {
            client = new IdleClient(config, "GoogleHome");


            client.NewMessageArrived += MessageArrived;
        }

        public  Task Run()
        {
            return client.RunAsync();
        }

        private void MessageArrived(string subject, ref bool isHandled)
        {
            var cmdMatch = Regex.Match(subject, @"^Command WOL (.+?) (.+?)$");
            if (!cmdMatch.Success)
            {
                return;
            }


            var ipText = cmdMatch.Groups[1].Value;
            var macText = cmdMatch.Groups[2].Value;
            Console.WriteLine($"Handling WOL to {ipText} {macText}");

            try
            {
                var ip = IPAddress.Parse(ipText);
                PhysicalAddress macAddress;
                if (string.IsNullOrEmpty(macText))
                {
                    if(Environment.OSVersion.Platform != PlatformID.Win32NT)
                    {
                        throw new InvalidOperationException("MAC adress is required on Non-Windows OS");
                    }
                    ArpRequestResult res = ArpRequest.Send(ip);
                    if (res.Exception != null)
                    {
                        ExceptionDispatchInfo.Capture(res.Exception).Throw();
#pragma warning disable CS8597 // Never happens due to ExceptionDispatchInfo throwing.
                        throw null;
#pragma warning restore CS8597
                    }
                    else
                    {
                        macAddress = res.Address;
                    }
                }
                else
                {
                    macAddress = PhysicalAddress.Parse(macText);
                }

                macAddress.SendWol(ip);

                var mask = new NetMask(255, 255, 255, 0);
                var broadcastAddress = ip.GetBroadcastAddress(mask);

                IPAddress.Broadcast.SendWol(macAddress);
                isHandled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem with WOL: " + ex.Message);
                isHandled = false;
            }
        }
    }

}
