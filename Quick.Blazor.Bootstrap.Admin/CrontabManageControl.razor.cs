using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class CrontabManageControl : ComponentBase_WithGettextSupport
    {
        private ModalAlert modalAlert;
        [Parameter]
        public int Rows { get; set; } = 20;
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private string TextSuccess => Locale.GetString("Success");
        private string TextSaveSuccess => Locale.GetString("Save success!");
        private string TextFailed => Locale.GetString("Failed");
        private string TextRows => Locale.GetString("Rows");
        private string TextEncoding => Locale.GetString("Encoding");

        [Parameter]
        public string IconSave { get; set; } = "fa fa-save";
        [Parameter]
        public string IconStart { get; set; } = "fa fa-play";
        [Parameter]
        public string IconStop { get; set; } = "fa fa-stop";

        private string Content { get; set; }

        protected override void OnParametersSet()
        {
            Content = Core.CrontabManager.Instance.Load();
            Core.CrontabManager.Instance.ConsoleHistoryChanged += OnCurrentContainerConsoleHistoryChanged;
        }

        private void Save()
        {
            try
            {
                Core.CrontabManager.Instance.Save(Content);
                modalAlert.Show(TextSuccess, "保存成功！");
            }
            catch (Exception ex)
            {
                modalAlert.Show(TextFailed, ex.Message);
            }
        }

        private void Start()
        {
            Core.CrontabManager.Instance.Start();
        }

        private void Stop()
        {
            Core.CrontabManager.Instance.Stop();
        }

        private void scrollToBottom()
        {
            JSRuntime.InvokeVoidAsync("eval",
    @"this.setTimeout(function () {
var els = document.getElementsByName('console');
for(var i=0;i<els.length;i++)
els[i].scrollTop = els[i].scrollHeight;
},100);"
                );
        }

        private void OnCurrentContainerConsoleHistoryChanged(object sender, EventArgs e)
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
                scrollToBottom();
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            Core.CrontabManager.Instance.ConsoleHistoryChanged -= OnCurrentContainerConsoleHistoryChanged;
        }

    }
}
