using Microsoft.AspNetCore.Components;
using Pty.Net;
using Quick.Localize;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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
        [Parameter]
        public IDictionary<string, string> PtyEnvironment { get; set; }
        [Parameter]
        public Dictionary<string, Action> OtherButtons { get; set; }

        public TerminalControl()
        {
            terminalOptions = new TerminalOptions
            {
                CursorBlink = true,
                CursorStyle = CursorStyle.Bar,
                Columns = Columns,
                Rows = Rows
            };
            if(OperatingSystem.IsWindows())
                terminalOptions.WindowsPty = new WindowsPty();
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

        private async Task OnData(string t)
        {
            ptyWriteStream?.WriteAsync(Encoding.Default.GetBytes(t));
            ptyWriteStream?.FlushAsync();
        }

        public async Task ExecuteCommand(string line)
        {
            await OnData(line + Environment.NewLine);
        }

        private void killShell()
        {
            if (pty != null)
            {
                if (OperatingSystem.IsMacOS())
                {
                    var process = Process.GetProcessById(pty.Pid);
                    if (process != null && !process.HasExited)
                        process.Kill(true);
                    return;
                }
                pty.Kill();
                pty.Dispose();
            }
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
            if (PtyEnvironment != null)
                options.Environment = PtyEnvironment;
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
            cts.Cancel();
            pty.ProcessExited -= Pty_ProcessExited;
            var message = Locale.GetString("Terminal process has exited with exit code {0}", e.ExitCode);
            terminal?.WriteLine(Environment.NewLine + message);
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
            killShell();
            base.Dispose();
        }
    }
}
