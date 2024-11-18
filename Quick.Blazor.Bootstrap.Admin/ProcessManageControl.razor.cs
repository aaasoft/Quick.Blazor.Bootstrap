using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Admin.Model;
using Quick.Blazor.Bootstrap.Utils;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    [UnsupportedOSPlatform("browser")]
    public partial class ProcessManageControl : ComponentBase_WithGettextSupport
    {
        private static string TextAskToKillProcess => Locale.GetString("Are you sure to kill process[Id: {0},Name: {1}]?");
        private static string TextFailed => Locale.GetString("Failed");
        private static string TextPressSearchButtonTip => Locale.GetString("Press 'Search' button to load process list.");
        private static string TextViewProcess => Locale.GetString("View Process");
        private static string TextKillProcess => Locale.GetString("Kill Process");
        private static string TextKillProcessTree => Locale.GetString("Kill Process Tree");
        private static string TextKeywords => Locale.GetString("Keywords");
        private static string TextColumnPID => Locale.GetString("PID");
        private static string TextColumnName => Locale.GetString("Name");
        private static string TextColumnThreads => Locale.GetString("Threads");
        private static string TextColumnMemory => Locale.GetString("Memory");
        private static string TextColumnOperate => Locale.GetString("Operate");
        private static string TextOrderBy => Locale.GetString("OrderBy");
        private static string TextOrderByPID => Locale.GetString("PID");
        private static string TextOrderByName => Locale.GetString("Name");
        private static string TextOrderByMemory => Locale.GetString("Memory");

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
        private readonly Utils.UnitStringConverting storageUSC = Utils.UnitStringConverting.StorageUnitStringConverting;
        [Parameter]
        public string OrderBy { get; set; } = nameof(FileInfo.Name);
        [Parameter]
        public bool OrderByAsc { get; set; } = true;

        private ProcessInfo[] Processes;

        public static Dictionary<string, object> PrepareParameters(ProcessInfoButton[] processViewOtherButtons)
        {
            return new Dictionary<string, object>()
            {
                [nameof(ProcessViewOtherButtons)] = processViewOtherButtons,
            };
        }


        private string getOrderByButtonIconClass(string field)
        {
            if (field != OrderBy)
                return null;
            if (OrderByAsc)
                return "fa fa-caret-up";
            return "fa fa-caret-down";
        }

        private void changeOrderByAscOrDesc(string orderBy)
        {
            var isOrderByChanged = OrderBy != orderBy;
            if (isOrderByChanged)
            {
                OrderBy = orderBy;
                OrderByAsc = true;
            }
            else
            {
                OrderByAsc = !OrderByAsc;
            }
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
                    switch (OrderBy)
                    {
                        case "pid":
                            if (OrderByAsc)
                                processInfos = processInfos.OrderBy(t => t.PID);
                            else
                                processInfos = processInfos.OrderByDescending(t => t.PID);
                            break;
                        case "name":
                            if (OrderByAsc)
                                processInfos = processInfos.OrderBy(t => t.Name);
                            else
                                processInfos = processInfos.OrderByDescending(t => t.Name);
                            break;
                        case "threads":
                            if (OrderByAsc)
                                processInfos = processInfos.OrderBy(t => t.ThreadsCount);
                            else
                                processInfos = processInfos.OrderByDescending(t => t.ThreadsCount);
                            break;
                        case "memory":
                            if (OrderByAsc)
                                processInfos = processInfos.OrderBy(t => t.Memory);
                            else
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