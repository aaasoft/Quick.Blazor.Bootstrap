using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class ModalLoading
    {
        [Parameter]
        public bool DialogSizeSmall { get; set; }
        [Parameter]
        public bool DialogSizeLarge { get; set; }
        [Parameter]
        public bool DialogSizeExtraLarge { get; set; }

        private string Title { get; set; }
        private string Content { get; set; }
        private bool ShowSpinner { get; set; }
        private Action CancelCallback { get; set; }
        private int? ProgressPercent;
        private string ProgressText;

        private bool Visiable { get; set; }

        public void UpdateContent(string content)
        {
            Content = content;
            InvokeAsync(StateHasChanged);
        }

        public void UpdateProgress(int? percent, string text)
        {
            ProgressPercent = percent;
            ProgressText = text;
            InvokeAsync(StateHasChanged);
        }

        public void Show(string title, string content, bool showSpinner, Action cancelCallback)
        {
            Title = title;
            Content = content;
            ShowSpinner = showSpinner;
            CancelCallback = cancelCallback;
            Visiable = true;
            InvokeAsync(StateHasChanged);
        }

        public void Cancel()
        {
            CancelCallback?.Invoke();
        }

        public void Close()
        {
            Title = null;
            Content = null;
            ProgressPercent = null;
            ProgressText = null;
            CancelCallback = null;
            Visiable = false;
            InvokeAsync(StateHasChanged);
        }
    }
}
