﻿using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;
using Quick.Localize;
using System;

namespace Quick.Blazor.Bootstrap
{
    public partial class ModalAlert : ComponentBase_WithGettextSupport
    {
        [Parameter]
        public bool DialogSizeSmall { get; set; }
        [Parameter]
        public bool DialogSizeLarge { get; set; }
        [Parameter]
        public bool DialogSizeExtraLarge { get; set; }
        [Parameter]
        public bool DialogScrollable { get; set; } = true;
        [Parameter]
        public string ModalHeaderCls { get; set; }
        [Parameter]
        public string ModalBodyCls { get; set; }
        [Parameter]
        public string ModalFooterCls { get; set; }
        
        public static string TextOk => Locale.GetString("OK");
        public static string TextCancel => Locale.GetString("Cancel");
        public string[] ContentLines => Content?.Split('\n') ?? new string[0];
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
