using Microsoft.AspNetCore.Components;
using Quick.Localize;
using Quick.Utils;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace Quick.Blazor.Bootstrap.Admin
{
    [UnsupportedOSPlatform("browser")]
    public partial class ProcessModulesViewControl : ComponentBase_WithGettextSupport
    {
        private string TextRefresh => Locale<ProcessViewControl>.GetString("Refresh");
        private string TextColumnModuleName => Locale<ProcessViewControl>.GetString("Module Name");
        private string TextColumnFileName => Locale<ProcessViewControl>.GetString("File Name");
        private string TextColumnFileVersion => Locale<ProcessViewControl>.GetString("File Version");
        private string TextColumnModuleMemorySize => Locale<ProcessViewControl>.GetString("Module Memory Size");

        private readonly UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;

        [Parameter]
        public int PID { get; set; }
        private Process process;

        private bool IsLoading = false;

        private string ExceptionString;
        private ProcessModule[] processModules;

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
                    processModules = process.Modules.Cast<ProcessModule>().ToArray();
                }
                catch (Exception ex)
                {
                    ExceptionString = ExceptionUtils.GetExceptionString(ex);
                }
                IsLoading = false;
                InvokeAsync(StateHasChanged);
            });
        }

        private string getFileVersion(ProcessModule module)
        {
            try
            {
                return module.FileVersionInfo?.FileVersion;
            }
            catch
            {
                return null;
            }
        }
    }
}
