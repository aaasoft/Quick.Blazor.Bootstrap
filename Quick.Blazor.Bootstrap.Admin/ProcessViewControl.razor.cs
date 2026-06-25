using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Admin.Model;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Blazor.Bootstrap.Utils;
using Quick.Localize;
using Quick.Shell.Utils;
using Quick.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    [UnsupportedOSPlatform("browser")]
    public partial class ProcessViewControl : ComponentBase_WithGettextSupport
    {
        private string TextProcessTitle => Locale<ProcessViewControl>.GetString("Process [{0}] {1}");
        private string TextRefresh => Locale<ProcessViewControl>.GetString("Refresh");
        private string TextSuccess => Locale<ProcessViewControl>.GetString("Success");
        private string TextFailed => Locale<ProcessViewControl>.GetString("Failed");
        private string TextAskToKillProcess => Locale<ProcessViewControl>.GetString("Are you sure to kill process[Id: {0},Name: {1}]?");
        private string TextKillProcessTree => Locale<ProcessViewControl>.GetString("Kill Process Tree");
        private string TextColumnPID => Locale<ProcessViewControl>.GetString("PID");
        private string TextColumnName => Locale<ProcessViewControl>.GetString("Name");
        private string TextColumnThreads => Locale<ProcessViewControl>.GetString("Threads");
        private string TextColumnMemory => Locale<ProcessViewControl>.GetString("Memory");
        private string TextColumnFileName => Locale<ProcessViewControl>.GetString("File Name");
        private string TextColumnCmdLine => Locale<ProcessViewControl>.GetString("Cmd Line");
        private string TextColumnWorkDirectory => Locale<ProcessViewControl>.GetString("Work Directory");
        private string TextColumnStartTime => Locale<ProcessViewControl>.GetString("Start Time");
        private string TextChildProcesses => Locale<ProcessViewControl>.GetString("Child Processes");
        private string TextColumnError => Locale<ProcessViewControl>.GetString("Error");
        private string TextProcessHasExited => Locale<ProcessViewControl>.GetString("Process has exited");

        private UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;

        [Parameter]
        public int PID { get; set; }
        [Parameter]
        public ProcessInfoButton[] OtherButtons { get; set; }
        private string Title;
        private ProcessInfo ProcessInfo;
        private ProcessInfo CurrentChildProcess;
        private ProcessInfo[] ChildProcesses;

        private string ExceptionString;

        private CancellationTokenSource cts = new CancellationTokenSource();
        public bool ProcessHasExited { get; private set; } = true;


        private ModalAlert modalAlert;

        protected override void OnParametersSet()
        {
            try
            {
                var process = Process.GetProcessById(PID);
                ProcessHasExited = process.HasExited;
                process.WaitForExitAsync(cts.Token).ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        return;
                    ProcessHasExited = true;
                    InvokeAsync(StateHasChanged);
                });
                RefreshProcess();
            }
            catch
            {
                ProcessInfo = new ProcessInfo()
                {
                    PID = PID
                };
            }
        }

        private string getProcessMemInfo(ProcessInfo processInfo)
        {
            try
            {
                return storageUSC.GetString(processInfo.Memory, 2, true) + "B";
            }
            catch
            {
                return null;
            }
        }

        private void RefreshProcess()
        {
            CurrentChildProcess = null;
            ChildProcesses = null;
            Title = string.Format(TextProcessTitle, PID, null);
            try
            {
                if (!ProcessHasExited)
                {
                    ProcessInfo = new ProcessInfo(PID, true);
                    Title = string.Format(TextProcessTitle, ProcessInfo.PID, ProcessInfo.Name);
                    ChildProcesses = ProcessInfo.GetChildProcesses();
                }
            }
            catch (Exception ex)
            {
                ExceptionString = ExceptionUtils.GetExceptionString(ex);
            }
            InvokeAsync(StateHasChanged);
        }

        private void btnKillProcess_Click()
        {
            modalAlert.Show(TextKillProcessTree, string.Format(TextAskToKillProcess, PID, ProcessInfo.Name),
                new ()
                {
                    OkCallback = () =>
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                var process = Process.GetProcessById(PID);
                                process.Kill(true);
                                modalAlert.Show(TextKillProcessTree, TextSuccess);
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show(TextKillProcessTree, TextFailed + "." + ex.Message);
                                RefreshProcess();
                            }
                        });
                    },
                    ShowCancelButton = true
                });
        }

        public override void Dispose()
        {
            cts.Cancel();
            base.Dispose();
        }
    }
}
