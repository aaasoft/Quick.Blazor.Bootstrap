using Microsoft.AspNetCore.Components;
using System;

namespace Quick.Blazor.Bootstrap
{
    public partial class ModalAlert : ComponentBase
    {
        [Parameter]
        public bool DialogSizeSmall { get; set; }
        [Parameter]
        public bool DialogSizeLarge { get; set; }
        [Parameter]
        public bool DialogSizeExtraLarge { get; set; }
        [Parameter]
        public bool DialogScrollable { get; set; } = true;

        public static string TextOk { get; set; } = "OK";
        public static string TextCancel { get; set; } = "Cancel";
        public string[] ContentLines => Content.Split('\n');
        private string Title { get; set; }
        private string Content { get; set; }
        private Action OkCallback { get; set; }
        private Action CancelCallback { get; set; }
        private bool Visiable { get; set; }
        private bool UsePreTag { get; set; } = false;
        public void Show(string title, string content, Action okCallback = null, Action cancelCallback = null, bool usePreTag = false)
        {
            Title = title;
            Content = content;
            OkCallback = okCallback;
            CancelCallback = cancelCallback;
            UsePreTag = usePreTag;
            Visiable = true;
            InvokeAsync(StateHasChanged);
        }

        public void Ok()
        {
            OkCallback?.Invoke();
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
