using Microsoft.AspNetCore.Components;
using Quick.Localize;
using Quick.Utils;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace Quick.Blazor.Bootstrap.Admin
{
    [UnsupportedOSPlatform("browser")]
    public partial class ProcessThreadsViewControl : ComponentBase_WithGettextSupport
    {
        private string TextRefresh => Locale<ProcessViewControl>.GetString("Refresh");
        private string TextColumnThreadId => Locale<ProcessViewControl>.GetString("Thread Id");
        private string TextColumnStartTime => Locale<ProcessViewControl>.GetString("Start Time");
        private string TextColumnThreadState => Locale<ProcessViewControl>.GetString("Thread State");
        private string TextColumnWaitReason => Locale<ProcessViewControl>.GetString("Wait Reason");
        private string TextColumnTotalProcessTime => Locale<ProcessViewControl>.GetString("Total Process Time");
        private string TextColumnUserProcessorTime => Locale<ProcessViewControl>.GetString("User Processor Time");
        private string TextColumnPrivilegedProcessorTime => Locale<ProcessViewControl>.GetString("Privileged Processor Time");

        [Parameter]
        public int PID { get; set; }
        private Process process;

        private bool IsLoading = false;

        private string ExceptionString;
        private ProcessThread[] processThreads;

        protected override void OnParametersSet()
        {
            RefreshProcess();
        }

        private void RefreshProcess()
        {
            IsLoading = true;
            Task.Run(() =>
            {
                try
                {
                    if (process == null)
                        process = Process.GetProcessById(PID);
                    process.Refresh();
                    processThreads = process.Threads.Cast<ProcessThread>().ToArray();
                }
                catch (Exception ex)
                {
                    ExceptionString = ExceptionUtils.GetExceptionString(ex);
                }
                IsLoading = false;
                InvokeAsync(StateHasChanged);
            });
        }

        private string getStartTime(ProcessThread thread)
        {
            try
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                    return thread.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch{}
            return default;
        }

        private string getWaitReason(ProcessThread thread)
        {
            try
            {
                if (thread.ThreadState == System.Diagnostics.ThreadState.Wait)
                    return thread.WaitReason.ToString();
            }
            catch { }
            return default;
        }

        private string getTotalProcessorTime(ProcessThread thread)
        {
            try
            {
                return thread.TotalProcessorTime.ToString();
            }
            catch{}
            return default;
        }
        
        private string getUserProcessorTime(ProcessThread thread)
        {
            try
            {
                return thread.UserProcessorTime.ToString();
            }
            catch{}
            return default;
        }

        private string getPrivilegedProcessorTime(ProcessThread thread)
        {
            try
            {
                return thread.PrivilegedProcessorTime.ToString();
            }
            catch{}
            return default;
        }
    }
}
