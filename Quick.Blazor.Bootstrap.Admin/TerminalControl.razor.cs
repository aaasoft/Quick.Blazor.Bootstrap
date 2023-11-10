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

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class TerminalControl : IDisposable
    {
        private TerminalOptions terminalOptions;
        private Xterm terminal;        
        private IPtyConnection pty;
        private CancellationTokenSource cts;

        [Parameter]
        public string TextOpen { get; set; } = "Open";
        [Parameter]
        public string TextClose{ get; set; } = "Close";
        [Parameter]
        public string TextColumn { get; set; } = "Column";
        [Parameter]
        public string TextRow { get; set; } = "Row";

        private int _Columns = 80;
        public int Columns
        {
            get { return _Columns; }
            set
            {
                _Columns = value;
                terminal.Resize(Columns, Rows);
                pty.Resize(Columns, Rows);
            }
        }

        private int _Rows = 24;
        public int Rows
        {
            get { return _Rows; }
            set
            {
                _Rows = value;
                terminal.Resize(Columns, Rows);
                pty.Resize(Columns, Rows);
            }
        }

        public TerminalControl()
        {
            terminalOptions = new TerminalOptions
            {
                CursorBlink = true,
                CursorStyle = CursorStyle.Bar,
                WindowsMode = OperatingSystem.IsWindows()
            };
        }

        private async Task OnFirstRender()
        {
            await newShell();            
        }

        private void OnData(string t)
        {
            pty?.WriterStream?.Write(Encoding.Default.GetBytes(t));
        }

        private void killShell()
        {
            cts?.Cancel();
            cts = null;

            pty?.Kill();
            pty?.Dispose();
            pty = null;
        }

        private async Task newShell()
        {            
            killShell();
            await terminal.Clear();

            string app = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(Environment.SystemDirectory, "cmd.exe") : "sh";
            var options = new PtyOptions
            {
                Name = "Quick.Blazor.Bootstrap Terminal",
                Cols = await terminal.GetColumns(),
                Rows = await terminal.GetRows(),
                Cwd = Environment.CurrentDirectory,
                App = app,
                ForceWinPty = true
            };
            cts = new CancellationTokenSource();
            pty = await PtyProvider.SpawnAsync(options, cts.Token);
            pty.ProcessExited += Pty_ProcessExited;
            _ = Task.Run(() =>
            {
                var stream = pty.ReaderStream;
                var buffer = new byte[1024];
                try
                {
                    while (true)
                    {
                        var ret = stream.Read(buffer);
                        terminal.Write(buffer.Take(ret).ToArray());
                    }
                }
                catch { }
            });
        }

        private void Pty_ProcessExited(object sender, PtyExitedEventArgs e)
        {
            terminal.WriteLine($"{Environment.NewLine}Terminal has exited，exit code：{e.ExitCode}");
            killShell();
        }

        public void Dispose()
        {
            killShell();
        }
    }
}
