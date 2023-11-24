using Microsoft.AspNetCore.Components;
using Pty.Net;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XtermBlazor;

namespace Quick.Blazor.Bootstrap.Terminal
{
    public partial class TerminalControl : IDisposable
    {
        private TerminalOptions terminalOptions;
        private Xterm terminal;        
        private IPtyConnection pty;
        private Stream ptyWriteStream;
        private CancellationTokenSource cts;

        [Parameter]
        public string TextOpen { get; set; } = "Open";
        [Parameter]
        public string TextClose{ get; set; } = "Close";
        [Parameter]
        public string TextColumn { get; set; } = "Column";
        [Parameter]
        public string TextRow { get; set; } = "Row";

        private int _Columns = 120;
        [Parameter]
        public int Columns
        {
            get { return _Columns; }
            set
            {
                if (value <= 0)
                    return;
                _Columns = value;
                terminal?.Resize(Columns, Rows);
                pty?.Resize(Columns, Rows);
            }
        }

        private int _Rows = 30;
        [Parameter]
        public int Rows
        {
            get { return _Rows; }
            set
            {
                if (value <= 0)
                    return;
                _Rows = value;
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
            cts?.Cancel();
            cts = null;

            ptyWriteStream = null;
            if (OperatingSystem.IsWindows())
            {
                pty?.Kill();
                pty?.Dispose();
            }            
            pty = null;
        }

        private async Task newShell()
        {
            killShell();
            await terminal.Clear();
            var cwd = WorkingDir;
            if (string.IsNullOrEmpty(cwd))
                cwd = Environment.CurrentDirectory;
            string app = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                app = Path.Combine(Environment.SystemDirectory, "cmd.exe");
            }
            else
            {
                app = Environment.GetEnvironmentVariable("SHELL");
                if (string.IsNullOrEmpty(app))
                    app = "sh";
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
            terminal.WriteLine($"{Environment.NewLine}. Terminal process has exited with exit code {e.ExitCode}");
            killShell();
        }

        public void Dispose()
        {
            killShell();
        }
    }
}
