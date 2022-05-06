using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class ModalAlert
    {
        [Parameter]
        public bool DialogSizeSmall { get; set; }
        [Parameter]
        public bool DialogSizeLarge { get; set; }
        [Parameter]
        public bool DialogSizeExtraLarge { get; set; }

        public static string TextOk { get; set; } = "OK";
        public static string TextCancel { get; set; } = "Cancel";

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
    }
}
