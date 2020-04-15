using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCController.Local.WinApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripButton("Exit", null, CleanExit));

            var icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);

            var _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = icon;
            _notifyIcon.Visible = true;
            _notifyIcon.BalloonTipText = "Running";
            _notifyIcon.BalloonTipTitle = "PCController";
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.DoubleClick += OpenInBrowser;
            _notifyIcon.ShowBalloonTip(2000);

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        PCController.Local.Program.Main(args);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Trace.WriteLine(ex.Message);
                    }
                }
            });

            Application.Run();
        }

        private static void OpenInBrowser(object sender, EventArgs e)
        {
        }

        private static void CleanExit(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}