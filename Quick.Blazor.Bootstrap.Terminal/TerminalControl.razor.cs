using Microsoft.AspNetCore.Components;
using Pty.Net;
using Quick.Localize;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XtermBlazor;

namespace Quick.Blazor.Bootstrap.Terminal
{
    public partial class TerminalControl : ComponentBase_WithGettextSupport
    {
        private TerminalOptions terminalOptions;
        private Xterm terminal;
        private IPtyConnection pty;
        private Stream ptyWriteStream;
        private CancellationTokenSource cts;

        private static string TextOpen => Locale.GetString("Open");
        private static string TextClose => Locale.GetString("Close");
        private static string TextColumn => Locale.GetString("Column");
        private static string TextRow => Locale.GetString("Row");

        [Parameter]
        public string App { get; set; }
        [Parameter]
        public int Columns { get; set; } = 120;
        [Parameter]
        public int Rows { get; set; } = 30;
        public int ActualColumns
        {
            get { return Columns; }
            set
            {
                if (value <= 0)
                    return;
                Columns = value;
                terminal?.Resize(Columns, Rows);
                pty?.Resize(Columns, Rows);
            }
        }

        public int ActualRows
        {
            get { return Rows; }
            set
            {
                if (value <= 0)
                    return;
                Rows = value;
                terminal?.Resize(Columns, Rows);
                pty?.Resize(Columns, Rows);
            }
        }

        [Parameter]
        public string WelcomeText { get; set; }
        [Parameter]
        public string WorkingDir { get; set; }

        public TerminalControl()
        {
            terminalOptions = new TerminalOptions
            {
                CursorBlink = true,
                CursorStyle = CursorStyle.Bar,
                WindowsMode = OperatingSystem.IsWindows(),
                Columns = Columns,
                Rows = Rows
            };
        }

        private async Task OnFirstRender()
        {
            if (!string.IsNullOrEmpty(WelcomeText))
                await terminal.WriteLine(WelcomeText);
            try
            {
                await newShell();
            }
            catch (Exception ex)
            {
                await terminal.WriteLine(ex.ToString());
            }
        }

        private void OnData(string t)
        {
            ptyWriteStream?.Write(Encoding.Default.GetBytes(t));
            ptyWriteStream?.Flush();
        }

        private void killShell()
        {
            if (pty != null)
                pty.Kill();
        }

        private async Task newShell()
        {
            killShell();
            await terminal.Clear();
            var cwd = WorkingDir;
            if (string.IsNullOrEmpty(cwd))
                cwd = Environment.CurrentDirectory;
            string app = App;
            if (string.IsNullOrEmpty(app))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    app = "powershell";
                }
                else
                {
                    app = "/bin/bash";
                    if (!File.Exists(app))
                        app = "sh";
                }
            }
            var options = new PtyOptions
            {
                Name = "Quick.Blazor.Bootstrap Terminal",
                Cols = Columns,
                Rows = Rows,
                Cwd = cwd,
                App = app,
                ForceWinPty = true
            };
            cts = new CancellationTokenSource();
            var cancallationToken = cts.Token;
            pty = await PtyProvider.SpawnAsync(options, cancallationToken);
            ptyWriteStream = pty.WriterStream;
            pty.ProcessExited += Pty_ProcessExited;
            _ = Task.Run(async () =>
            {
                var ptyReaderStream = pty.ReaderStream;
                var buffer = new byte[1024];
                try
                {
                    while (true)
                    {
                        var ret = await ptyReaderStream.ReadAsync(buffer, 0, buffer.Length, cancallationToken);
                        if (ret <= 0)
                            break;
                        await terminal.Write(buffer.Take(ret).ToArray());
                    }
                }
                catch { }
            });
        }

        private void Pty_ProcessExited(object sender, PtyExitedEventArgs e)
        {
            pty.ProcessExited -= Pty_ProcessExited;
            var message = Locale.GetString("Terminal process has exited with exit code {0}",e.ExitCode);
            terminal?.WriteLine(message);
            if (OperatingSystem.IsWindows())
            {
                pty.Kill();
                pty.Dispose();
            }
            ptyWriteStream = null;
            pty = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            killShell();
        }
    }
}
