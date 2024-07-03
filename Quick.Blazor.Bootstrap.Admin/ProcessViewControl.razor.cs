using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Localize;
using Quick.Shell.Utils;
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
        private string TextRefresh => Locale.GetString("Refresh");
        private string TextSuccess => Locale.GetString("Success");
        private string TextFailed => Locale.GetString("Failed");
        private string TextAskToKillProcess => Locale.GetString("Are you sure to kill process[Id: {0},Name: {1}]?");
        private string TextKillProcessTree => Locale.GetString("Kill Process Tree");
        private string TextColumnPID => Locale.GetString("PID");
        private string TextColumnName => Locale.GetString("Name");
        private string TextColumnThreads => Locale.GetString("Threads");
        private string TextColumnMemory => Locale.GetString("Memory");
        private string TextColumnFileName => Locale.GetString("File Name");
        private string TextColumnCmdLine => Locale.GetString("Cmd Line");
        private string TextColumnWorkDirectory => Locale.GetString("Work Directory");
        private string TextColumnStartTime => Locale.GetString("Start Time");
        private string TextColumnError => Locale.GetString("Error");
        private string TextProcessHasExited => Locale.GetString("Process has exited");

        private UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;

        [Parameter]
        public int PID { get; set; }
        [Parameter]
        public Dictionary<string, Action> OtherButtons { get; set; }
        private ProcessInfo ProcessInfo;

        private string ExceptionString;

        private CancellationTokenSource cts = new CancellationTokenSource();
        public bool ProcessHasExited { get; private set; } = true;


        private ModalAlert modalAlert;
        private ModalLoading modalLoading;

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
            try
            {
                if (!ProcessHasExited)
                    ProcessInfo = new ProcessInfo(PID);
            }
            catch (Exception ex)
            {
                ExceptionString = ExceptionUtils.GetExceptionString(ex);
            }
            InvokeAsync(StateHasChanged);
        }

        private void btnKillProcess_Click()
        {
            modalAlert.Show(TextKillProcessTree, string.Format(TextAskToKillProcess, PID, ProcessInfo.Name), () =>
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
              }, null);
        }

        public override void Dispose()
        {
            cts.Cancel();
            base.Dispose();
        }
    }
}
