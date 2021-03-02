﻿using Microsoft.AspNetCore.Components;
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
        private string Title { get; set; }
        private RenderFragment Content { get; set; }
        private Action OkCallback { get; set; }
        private Action CancelCallback { get; set; }
        private bool Visiable { get; set; }

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
    }
}
