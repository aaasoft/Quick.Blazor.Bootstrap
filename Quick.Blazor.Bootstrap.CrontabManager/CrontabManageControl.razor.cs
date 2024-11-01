using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.CrontabManager
{
    public partial class CrontabManageControl : ComponentBase_WithGettextSupport
    {
        private ModalAlert modalAlert;
        [Parameter]
        public int Rows { get; set; } = 10;
        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        private LogViewControl logViewControl;

        private string TextSuccess => Locale.GetString("Success");
        private string TextSave => Locale.GetString("Save");
        private string TextFailed => Locale.GetString("Failed");

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
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                Core.CrontabManager.Instance.NewConsoleHistory += OnCurrentContainerNewConsoleHistory;
                foreach (var line in Core.CrontabManager.Instance.ConsoleHistoryLines)
                    logViewControl.AddLine(line);
            }
        }

        private void Save()
        {
            try
            {
                Core.CrontabManager.Instance.Save(Content);
                modalAlert.Show(TextSave, TextSuccess);
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

        private void OnCurrentContainerNewConsoleHistory(object sender, string e)
        {
            logViewControl.AddLine(e);
        }

        public override void Dispose()
        {
            base.Dispose();
            Core.CrontabManager.Instance.NewConsoleHistory -= OnCurrentContainerNewConsoleHistory;
        }

    }
}
