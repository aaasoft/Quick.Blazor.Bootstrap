using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class ModalWindow
    {
        [Parameter]
        public bool DialogCentered { get; set; } = true;
        [Parameter]
        public bool DialogScrollable { get; set; } = true;
        [Parameter]
        public bool DialogSizeSmall { get; set; }
        [Parameter]
        public bool DialogSizeLarge { get; set; }
        [Parameter]
        public bool DialogSizeExtraLarge { get; set; }

        private string Title { get; set; }
        private RenderFragment Content { get; set; }
        private bool Visiable { get; set; }

        public void Show<T>(string title, Dictionary<string, object> parameterDict = null)
        {
            Show(title, typeof(T), parameterDict);
        }

        public void Show(string title, Type componentType, Dictionary<string, object> parameterDict = null)
        {
            Title = title;
            Content = BlazorUtils.GetRenderFragment(componentType, parameterDict);
            Visiable = true;
            InvokeAsync(StateHasChanged);
        }

        public void Close()
        {
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
