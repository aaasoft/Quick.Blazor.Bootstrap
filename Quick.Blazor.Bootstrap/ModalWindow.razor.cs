using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;
using System;
using System.Collections.Generic;

namespace Quick.Blazor.Bootstrap;

public partial class ModalWindow : ComponentBase
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
    [Parameter]
    public string ModalHeaderCls { get; set; }
    [Parameter]
    public string ModalBodyCls { get; set; }
    [Parameter]
    public string ModalFooterCls { get; set; }
    private string Title { get; set; }
    private RenderFragment Content { get; set; }
    private bool Visiable { get; set; }

    public void Show<T>(string title, Dictionary<string, object> parameterDict = null)
    {
        Show(title, typeof(T), parameterDict);
    }

    public void Show(string title, Type componentType, Dictionary<string, object> parameterDict = null)
    {
        if (parameterDict == null)
            parameterDict = new();
        parameterDict[nameof(ModalWindow)] = this;
        Show(title, BlazorUtils.GetRenderFragment(componentType, parameterDict));
        InvokeAsync(StateHasChanged);
    }

    public void Show(string title, RenderFragment content)
    {
        Title = title;
        Content = content;
        Visiable = true;
        InvokeAsync(StateHasChanged);
    }

    public void UpdateTitle(string title)
    {
        Title = title;
        InvokeAsync(StateHasChanged);
    }

    public void UpdateContent(RenderFragment content)
    {
        Content = content;
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
