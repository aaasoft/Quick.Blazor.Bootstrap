using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Quick.Localize;

namespace Quick.Blazor.Bootstrap
{
    public class ModalAlertOptions
    {
        public bool ShowOkButton { get; set; } = true;
        public Action OkCallback { get; set; }
        public bool ShowCancelButton { get; set; } = false;
        private Action _CancelCallback;
        public Action CancelCallback
        {
            get => _CancelCallback;
            set
            {
                _CancelCallback = value;
                if (value != null)
                    ShowCancelButton = true;
            }
        }
        public bool ShowCloseButton { get; set; } = true;
        public Action CloseCallback { get; set; }
        public bool UsePreTag { get; set; } = false;
    }

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

        public static string TextOk => Locale<ModalAlert>.GetString("OK");
        public static string TextCancel => Locale<ModalAlert>.GetString("Cancel");
        public string[] ContentLines => Content?.Split('\n') ?? new string[0];
        private string Title { get; set; }
        private string Content { get; set; }
        private bool Visiable { get; set; }
        private ModalAlertOptions options = new();

        public void Show(string title, string content, ModalAlertOptions options)
        {
            Title = title;
            Content = content;
            if (options == null)
                options = new();
            this.options = options;
            Visiable = true;
            InvokeAsync(StateHasChanged);
        }

        public void Show(string title, string content)
        {
            Show(title, content, null);
        }

        public void Ok()
        {
            options.OkCallback?.Invoke();
            Close();
        }

        public void Cancel()
        {
            options.CancelCallback?.Invoke();
            Close();
        }

        public void Close()
        {
            options.CloseCallback?.Invoke();
            Title = null;
            Content = null;
            Visiable = false;
            InvokeAsync(StateHasChanged);
        }

        public void SwitchDialogSizeExtraLarge()
        {
            DialogSizeExtraLarge = !DialogSizeExtraLarge;
        }
    }
}
