using JobManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCController.LocalApp
{
    internal static class Program
    {
        private static Process _process;
        private static Job _job;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripButton("Exit", null, CleanExit));

            var icon = Icon.ExtractAssociatedIcon(@"PCController.LocalApp.exe");

            var _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = icon;
            _notifyIcon.Visible = true;
            _notifyIcon.BalloonTipText = "Running";
            _notifyIcon.BalloonTipTitle = "PCController";
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.DoubleClick += OpenInBrowser;
            _notifyIcon.ShowBalloonTip(2000);

            RunServer();
            Application.Run();
        }

        private static void OpenInBrowser(object sender, EventArgs e)
        {
        }

        private static void RunServer()
        {
            var fs = File.Open("server.log", FileMode.Create, FileAccess.Write);
            var logWriter = new StreamWriter(fs);

            var path = Path.Combine("server", "PCController.Local.exe");
            var startInfo = new ProcessStartInfo()
            {
                FileName = path,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = "server"
            };
            startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

            _process = Process.Start(startInfo);
            _process.BeginErrorReadLine();

            _job = new Job();
            _job.AddProcess(_process.Handle);
            _job.AddProcess(_process.MainWindowHandle);

            _process.OutputDataReceived += (o, e) =>
            {
                Trace.WriteLine(e.Data);
                Console.Out.WriteLine(e.Data);
                logWriter.WriteLine(e.Data);
                logWriter.Flush();
                fs.Flush();
            };
            _process.ErrorDataReceived += (o, e) =>
            {
                Trace.WriteLine(e.Data);
                Console.Error.WriteLine(e.Data);
                logWriter.WriteLine(e.Data);
                logWriter.Flush();
                fs.Flush();
            };
            _process.BeginOutputReadLine();
        }

        private static void CleanExit(object? sender, EventArgs e)
        {
            _process?.Kill();
            Application.Exit();
        }
    }
}