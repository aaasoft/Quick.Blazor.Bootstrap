using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Admin.Model;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    [UnsupportedOSPlatform("browser")]
    public partial class ProcessManageControl : ComponentBase_WithGettextSupport
    {
        private string TextAskToKillProcess => Locale.GetString("Are you sure to kill process[Id: {0},Name: {1}]?");
        private string TextFailed => Locale.GetString("Failed");
        private string TextPressSearchButtonTip => Locale.GetString("Press 'Search' button to load process list.");
        private string TextViewProcess => Locale.GetString("View Process");
        private string TextKillProcess => Locale.GetString("Kill Process");
        private string TextKillProcessTree => Locale.GetString("Kill Process Tree");
        private string TextKeywords => Locale.GetString("Keywords");
        private string TextColumnPID => Locale.GetString("PID");
        private string TextColumnName => Locale.GetString("Name");
        private string TextColumnThreads => Locale.GetString("Threads");
        private string TextColumnMemory => Locale.GetString("Memory");
        private string TextColumnOperate => Locale.GetString("Operate");
        private string TextOrderBy => Locale.GetString("OrderBy");
        private string TextOrderByPID => Locale.GetString("PID");
        private string TextOrderByName => Locale.GetString("Name");
        private string TextOrderByMemory => Locale.GetString("Memory");

        [Parameter]
        public string IconSearch { get; set; } = "fa fa-search";
        [Parameter]
        public string IconViewProcess { get; set; } = "fa fa-list-alt";
        [Parameter]
        public string IconKillProcess { get; set; } = "fa fa-stop";
        [Parameter]
        public string IconKillProcessTree { get; set; } = "fa fa-tree";
        [Parameter]
        public ProcessInfoButton[] ProcessViewOtherButtons { get; set; }

        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ModalWindow modalWindow;

        private string searchKeywords;
        private Utils.UnitStringConverting storageUSC = Utils.UnitStringConverting.StorageUnitStringConverting;
        private string orderByField = "pid";

        private ProcessInfo[] Processes;

        public static Dictionary<string, object> PrepareParameters(ProcessInfoButton[] processViewOtherButtons)
        {
            return new Dictionary<string, object>()
            {
                [nameof(ProcessViewOtherButtons)] = processViewOtherButtons,
            };
        }


        private string getOrderByButtonClass(string field)
        {
            if (field == orderByField)
                return "";
            return "disabled";
        }
        private void changeOrderField(string orderByField)
        {
            this.orderByField = orderByField;
            search();
        }

        private void search()
        {
            modalLoading.Show(null, null, true, null);
            Task.Run(() =>
            {
                try
                {
                    var processInfos = Process.GetProcesses()
                    .Where(t => string.IsNullOrEmpty(searchKeywords) || t.Id.ToString() == searchKeywords || t.ProcessName.Contains(searchKeywords))
                    .Select(t => new ProcessInfo(t));
                    switch (orderByField)
                    {
                        case "pid":
                            processInfos = processInfos.OrderBy(t => t.PID);
                            break;
                        case "name":
                            processInfos = processInfos.OrderBy(t => t.Name);
                            break;
                        case "memory":
                            processInfos = processInfos.OrderByDescending(t => t.Memory);
                            break;
                    }
                    Processes = processInfos.ToArray();
                }
                catch (Exception ex)
                {
                    modalAlert.Show("错误", ExceptionUtils.GetExceptionString(ex));
                }
                finally
                {
                    modalLoading.Close();
                    InvokeAsync(StateHasChanged);
                }
            });
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

        private void ViewProcess(ProcessInfo info)
        {
            try
            {
                modalWindow.Show<ProcessViewControl>(
                    string.Format(TextViewProcess, info.PID, info.Name),
                    ProcessViewControl.PrepareParameters(info.PID, ProcessViewOtherButtons));
            }
            catch (Exception ex)
            {
                modalAlert.Show(TextFailed, ExceptionUtils.GetExceptionMessage(ex));
            }
        }

        private void KillProcess(ProcessInfo info, bool entireProcessTree)
        {
            var title = entireProcessTree ? TextKillProcessTree : TextKillProcess;
            modalAlert.Show(title, string.Format(TextAskToKillProcess, info.PID, info.Name), () =>
              {
                  Task.Run(() =>
                  {
                      try
                      {
                          var process = Process.GetProcessById(info.PID);
                          if (process == null)
                              throw new ApplicationException(Locale.GetString("Can't found process[Id:{0}].", info.PID));
                          process.Kill(entireProcessTree);
                          search();
                      }
                      catch (Exception ex)
                      {
                          Task.Delay(100).ContinueWith(task => modalAlert.Show(TextFailed, ex.Message, null, null));
                      }
                  });
              }, null);
        }
    }
}