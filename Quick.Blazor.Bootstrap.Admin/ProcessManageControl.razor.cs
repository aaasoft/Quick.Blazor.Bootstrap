﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class ProcessManageControl
    {
        [Parameter]
        public string TextAskToKillProcess { get; set; } = "Are you sure to kill process[Id: {0},Name: {1}]?";
        [Parameter]
        public string TextFailed { get; set; } = "Failed";
        [Parameter]
        public string TextRefresh { get; set; } = "Refresh";
        [Parameter]
        public string TextPressRefreshButtonTip { get; set; } = "Press 'Refresh' button to load process list.";
        [Parameter]
        public string TextKillProcess { get; set; } = "Kill Process";
        [Parameter]
        public string TextKillProcessTree { get; set; } = "Kill Process Tree";

        [Parameter]
        public RenderFragment IconRefresh { get; set; }
        [Parameter]
        public RenderFragment IconProcess { get; set; }
        [Parameter]
        public RenderFragment IconKillProcess { get; set; }
        [Parameter]
        public RenderFragment IconKillProcessTree { get; set; }

        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private Utils.UnitStringConverting storageUSC = Utils.UnitStringConverting.StorageUnitStringConverting;

        private ProcessInfo[] Processes;

        public class ProcessInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string MemoryInfo { get; set; }
            public string StartTime { get; set; }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        private void refreshProcesss()
        {
            modalLoading.Show(null, null, true, null);
            Task.Run(() =>
            {
#pragma warning disable CA1416 // 验证平台兼容性
                Processes = Process.GetProcesses().Select(t => new ProcessInfo()
                {
                    Id = t.Id,
                    Name = t.ProcessName,
                    MemoryInfo = getProcessMemInfo(t),
                    StartTime = getProcessStartTime(t)
                }).ToArray();
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
                              throw new ApplicationException($"Can't found process[Id:{info.Id}].");
                          process.Kill(entireProcessTree);
                          refreshProcesss();
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