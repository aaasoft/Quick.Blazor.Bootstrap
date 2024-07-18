using System;
using System.Threading;
using Microsoft.AspNetCore.Components;

namespace TestWebApplication.Pages
{
    public partial class LogViewTest : ComponentBase, IDisposable
    {
        private Quick.Blazor.Bootstrap.Admin.LogViewControl control;
        private Timer refreshTimer;

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
                refreshTimer = new Timer(addLog, null, 0, 1000);
        }

        private void addLog(object _)
        {
            control.AddLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {Guid.NewGuid()}");
        }

        public void Dispose()
        {
            refreshTimer?.Dispose();
        }
    }
}
