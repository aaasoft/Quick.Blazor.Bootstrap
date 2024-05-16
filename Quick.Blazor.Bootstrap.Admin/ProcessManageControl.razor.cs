using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class ProcessManageControl : ComponentBase
    {
        [Parameter]
        public string TextAskToKillProcess { get; set; } = Locale.Catalog.GetString("Are you sure to kill process[Id: {0},Name: {1}]?");
        [Parameter]
        public string TextFailed { get; set; } = Locale.Catalog.GetString("Failed");
        [Parameter]
        public string TextPressSearchButtonTip { get; set; } = Locale.Catalog.GetString("Press 'Search' button to load process list.");
        [Parameter]
        public string TextKillProcess { get; set; } = Locale.Catalog.GetString("Kill Process");
        [Parameter]
        public string TextKillProcessTree { get; set; } = Locale.Catalog.GetString("Kill Process Tree");
        [Parameter]
        public string TextKeywords { get; set; } = Locale.Catalog.GetString("Keywords");
        [Parameter]
        public string TextColumnPID { get; set; } = Locale.Catalog.GetString("PID");
        [Parameter]
        public string TextColumnName { get; set; } = Locale.Catalog.GetString("Name");
        [Parameter]
        public string TextColumnThreads { get; set; } = Locale.Catalog.GetString("Threads");
        [Parameter]
        public string TextColumnMemory { get; set; } = Locale.Catalog.GetString("Memory");
        [Parameter]
        public string TextColumnOperate { get; set; } = Locale.Catalog.GetString("Operate");

        [Parameter]
        public string TextOrderBy { get; set; } = Locale.Catalog.GetString("OrderBy");
        [Parameter]
        public string TextOrderByPID { get; set; } = Locale.Catalog.GetString("PID");
        [Parameter]
        public string TextOrderByName { get; set; } = Locale.Catalog.GetString("Name");
        [Parameter]
        public string TextOrderByMemory { get; set; } = Locale.Catalog.GetString("Memory");

        [Parameter]
        public string IconSearch { get; set; }="fa fa-search";
        [Parameter]
        public string IconKillProcess { get; set; }="fa fa-stop";
        [Parameter]
        public string IconKillProcessTree { get; set; }="fa fa-tree";

        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private string searchKeywords;
        private Utils.UnitStringConverting storageUSC = Utils.UnitStringConverting.StorageUnitStringConverting;
        private string orderByField = "pid";

        private ProcessInfo[] Processes;

        public class ProcessInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public long MemoryUsed { get; set; }
            public string MemoryInfo { get; set; }
            public int Threads { get; set; }
            public string StartTime { get; set; }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        private string getFieldButtonClass(string field)
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
#pragma warning disable CA1416 // 验证平台兼容性
                var processInfos = Process.GetProcesses()
                .Where(t => string.IsNullOrEmpty(searchKeywords) || t.Id.ToString() == searchKeywords || t.ProcessName.Contains(searchKeywords))
                .Select(t => new ProcessInfo()
                {
                    Id = t.Id,
                    Name = t.ProcessName,
                    MemoryUsed = t.WorkingSet64,
                    MemoryInfo = getProcessMemInfo(t),
                    Threads = t.Threads.Count,
                    StartTime = getProcessStartTime(t)
                });
                switch (orderByField)
                {
                    case "pid":
                        processInfos = processInfos.OrderBy(t => t.Id);
                        break;
                    case "name":
                        processInfos = processInfos.OrderBy(t => t.Name);
                        break;
                    case "memory":
                        processInfos = processInfos.OrderByDescending(t => t.MemoryUsed);
                        break;
                }
                Processes = processInfos.ToArray();
#pragma warning restore CA1416 // 验证平台兼容性
                modalLoading.Close();
                InvokeAsync(StateHasChanged);
            });
        }

        private string getProcessMemInfo(Process process)
        {
            try
            {
#pragma warning disable CA1416 // 验证平台兼容性
                return storageUSC.GetString(process.WorkingSet64, 2, true) + "B";
#pragma warning restore CA1416 // 验证平台兼容性
            }
            catch
            {
                return null;
            }
        }

        private string getProcessStartTime(Process process)
        {
            try
            {
#pragma warning disable CA1416 // 验证平台兼容性
                return process.StartTime.ToString();
#pragma warning restore CA1416 // 验证平台兼容性
            }
            catch
            {
                return null;
            }
        }

        private void KillProcess(ProcessInfo info, bool entireProcessTree)
        {
            var title = entireProcessTree ? TextKillProcessTree : TextKillProcess;
            modalAlert.Show(title, string.Format(TextAskToKillProcess, info.Id, info.Name), () =>
              {
                  Task.Run(() =>
                  {
                      try
                      {
#pragma warning disable CA1416 // 验证平台兼容性
                          var process = Process.GetProcessById(info.Id);
                          if (process == null)
                              throw new ApplicationException(Locale.Catalog.GetString("Can't found process[Id:{0}].", info.Id));
                          process.Kill(entireProcessTree);
                          search();
#pragma warning restore CA1416 // 验证平台兼容性
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