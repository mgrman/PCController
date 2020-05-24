using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCController.Local.WinApp
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripButton("Exit", null, CleanExit));

            var icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly()
                .Location);

            var notifyIcon = new NotifyIcon();
            notifyIcon.Icon = icon;
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipText = "Running";
            notifyIcon.BalloonTipTitle = "PCController";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ContextMenuStrip = menu;
            notifyIcon.DoubleClick += OpenInBrowser;
            notifyIcon.ShowBalloonTip(2000);

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Local.Program.Main(args);
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
