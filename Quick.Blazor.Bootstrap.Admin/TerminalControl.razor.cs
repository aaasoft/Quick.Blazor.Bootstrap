using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class TerminalControl : IDisposable
    {
        private ModalAlert modalAlert = null;
        private ToastStack toastStack = null;

        private const int MAX_CONSOLE_LINES = 10000;
        private static EventCallbackFactory EventCallbackFactory = new EventCallbackFactory();

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        private Process process;
        private StreamWriter writer;

        private bool isEnterKeyPressed = false;
        private int outputBufferIndex = 0;
        private byte[] outputBuffer = new byte[10 * 1024];

        public int ConsoleHeight = 500;
        public string ConsoleCommand = string.Empty;
        public string ConsoleLastLine;
        public List<string> ConsoleHistoryList = new List<string>();

        [Parameter]
        public string TextOpen { get; set; } = "Open";
        [Parameter]
        public string TextClose{ get; set; } = "Close";
        [Parameter]
        public string TextClear { get; set; } = "Clear";
        [Parameter]
        public string TextReboot{ get; set; } = "Reboot";
        [Parameter]
        public string TextRebootConfirm { get; set; } = "Are you sure to reboot computer?";
        [Parameter]
        public string TextLoading{ get; set; } = "Loading...";

        public void PushConsoleHistory(params string[] lines)
        {
            if (lines == null || lines.Length == 0)
                return;
            lock (ConsoleHistoryList)
            {
                ConsoleHistoryList.AddRange(lines);
                while (ConsoleHistoryList.Count > MAX_CONSOLE_LINES)
                    ConsoleHistoryList.RemoveAt(0);
            }
            isConsoleHistoryChanged = true;
        }
        private Timer refreshConsoleHistoryTimer;
        private bool isConsoleHistoryChanged = false;
        protected override void OnInitialized()
        {
            newShell();
            refreshConsoleHistoryTimer = new Timer(refreshConsoleHistoryFunc);
            refreshConsoleHistoryTimer.Change(0, 200);
        }
        private void refreshConsoleHistoryFunc(object _)
        {
            if (isConsoleHistoryChanged)
            {
                InvokeAsync(StateHasChanged).ContinueWith(t => scrollToBottom());
                isConsoleHistoryChanged = false;
            }
        }

        private void killShell()
        {
            if (process != null)
            {
#pragma warning disable CA1416 // 验证平台兼容性
                if (!process.HasExited)
                    process.Kill(true);
#pragma warning restore CA1416 // 验证平台兼容性
                process = null;
                ConsoleCommand = null;
                ConsoleLastLine = null;
                outputBufferIndex = 0;
                isEnterKeyPressed = false;
            }
        }

        private void newShell()
        {
            killShell();

            var shellFileName = string.Empty;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    shellFileName = "cmd.exe";
                    break;
                case PlatformID.Unix:
                default:
                    shellFileName = "sh";
                    break;
            }

#pragma warning disable CA1416 // 验证平台兼容性
            var psi = new ProcessStartInfo(shellFileName);
            psi.RedirectStandardOutput = true;

            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            process = Process.Start(psi);
            writer = process.StandardInput;
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.Exited += Process_Exited;
            PushConsoleHistory($"Terminal process[{process.StartInfo.FileName}](Id:{process.Id}) started.");
            process.BeginErrorReadLine();
            beginReadOutput(process.StandardOutput.BaseStream, process.StandardOutput.CurrentEncoding);
            focusCommandInput();
#pragma warning restore CA1416 // 验证平台兼容性
        }

        private void beginReadOutput(Stream stream, Encoding encoding)
        {
            Task.Run(() =>
            {
                var ret = stream.Read(outputBuffer, outputBufferIndex, outputBuffer.Length - outputBufferIndex);
                if (ret <= 0)
                    return;
                outputBufferIndex += ret;
                var str = encoding.GetString(outputBuffer, 0, outputBufferIndex);
                if (isEnterKeyPressed)
                {
                    isEnterKeyPressed = false;
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        outputBufferIndex = 0;
                        beginReadOutput(stream, encoding);
                        return;
                    }
                }
                str = str.Replace("\r", string.Empty);
                switch (str)
                {
                    case "\f":
                        ClearConsoleHistory();
                        beginReadOutput(stream, encoding);
                        return;
                }

                var lines = str.Split(new char[] { '\n' });

                //如果是回车或者换行
                if (str.EndsWith('\n'))
                {
                    ConsoleLastLine = null;
                    outputBufferIndex = 0;
                    PushConsoleHistory(lines.Take(lines.Length - 1).ToArray());
                }
                else
                {
                    PushConsoleHistory(lines.Take(lines.Length - 1).ToArray());

                    var tmpLine = lines.LastOrDefault();
                    ConsoleLastLine = tmpLine;
                    outputBufferIndex = encoding.GetBytes(tmpLine, 0, tmpLine.Length, outputBuffer, 0);
                }
                beginReadOutput(stream, encoding);
            });
        }

        private void ClearConsoleHistory()
        {
            ConsoleHistoryList.Clear();
            InvokeAsync(StateHasChanged);
        }

        private EventCallback ConsoleSetHeight(int height)
        {
            return EventCallbackFactory.Create(this, () =>
            {
                ConsoleHeight = height;
                InvokeAsync(StateHasChanged);
            });
        }

        private void scrollToBottom()
        {
            JSRuntime.InvokeVoidAsync("eval",
    @"this.setTimeout(function () {
var el = document.getElementById('console');
el.scrollTop = el.scrollHeight;
},100);"
                );
        }

        private void focusCommandInput()
        {
            JSRuntime.InvokeVoidAsync("eval",
    @"this.setTimeout(function () {
var el = document.getElementById('txtCommand');
el.focus();
},100);"
                );
        }

        private void onKeyPress(KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case "Enter":
                    Task.Delay(10).ContinueWith(t =>
                    {
                        executeCommand(ConsoleCommand);
                        this.InvokeAsync(() =>
                        {
                            ConsoleCommand = string.Empty;
                        });
                    });
                    break;
            }
        }

        private void executeCommand(string line)
        {
            PushConsoleHistory(ConsoleLastLine + line);
            isEnterKeyPressed = true;
            writer.WriteLine(line);
            writer.Flush();
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
#pragma warning disable CA1416 // 验证平台兼容性
            PushConsoleHistory(e.Data);
#pragma warning restore CA1416 // 验证平台兼容性
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            var process = (Process)sender;
#pragma warning disable CA1416 // 验证平台兼容性
            PushConsoleHistory($"Terminal process[{process.StartInfo.FileName}](Id:{process.Id})has exited，exit code：{process.ExitCode}");
#pragma warning restore CA1416 // 验证平台兼容性
            killShell();
        }

        private void Reboot()
        {
            modalAlert.Show(TextReboot, TextRebootConfirm,
            () =>
            {
                var cmdLine = (string)null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    cmdLine = "shutdown /r /t 00";
                else
                    cmdLine = "reboot";
                ConsoleCommand = cmdLine;
                executeCommand(cmdLine);
                InvokeAsync(StateHasChanged);
            },
            null);
        }

        public void Dispose()
        {
            killShell();
        }
    }
}
