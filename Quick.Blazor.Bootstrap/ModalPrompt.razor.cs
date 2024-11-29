using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class ModalPrompt : ComponentBase_WithGettextSupport
    {
        [Parameter]
        public bool DialogSizeSmall { get; set; }
        [Parameter]
        public bool DialogSizeLarge { get; set; }
        [Parameter]
        public bool DialogSizeExtraLarge { get; set; }
        [Parameter]
        public string ModalHeaderCls { get; set; }
        [Parameter]
        public string ModalBodyCls { get; set; }
        [Parameter]
        public string ModalFooterCls { get; set; }
        public static string TextOk => Locale.GetString("OK");
        public static string TextCancel => Locale.GetString("Cancel");

        private string Title { get; set; }
        private string Content { get; set; }
        private Action<string> OkCallback { get; set; }
        private Action CancelCallback { get; set; }
        private bool Visiable { get; set; }

        public void Show(string title, string content, Action<string> okCallback = null, Action cancelCallback = null)
        {
            Title = title;
            Content = content;
            OkCallback = okCallback;
            CancelCallback = cancelCallback;
            Visiable = true;
            this.StateHasChanged();
        }

        public void Ok()
        {
            OkCallback?.Invoke(Content);
            Close();
        }

        public void Cancel()
        {
            CancelCallback?.Invoke();
            Close();
        }

        public void Close()
        {
            Title = null;
            Content = null;
            OkCallback = null;
            CancelCallback = null;
            Visiable = false;
            InvokeAsync(StateHasChanged);
        }

        public void SwitchDialogSizeExtraLarge()
        {
            DialogSizeExtraLarge = !DialogSizeExtraLarge;
        }
    }
}
