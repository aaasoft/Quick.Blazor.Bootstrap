﻿using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class ModalLoading : ComponentBase_WithGettextSupport
    {
        [Parameter]
        public bool DialogScrollable { get; set; } = true;
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
        public static string TextCancel => Locale.GetString("Cancel");

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

        public void Show(string title, string content, bool showSpinner, Action cancelCallback = null)
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
