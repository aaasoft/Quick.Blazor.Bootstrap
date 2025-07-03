using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;

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
    private Action OnCloseAction { get; set; }

    public void Show<T>(string title, Dictionary<string, object> parameterDict = null, Action onCloseAction = null)
    {
        Show<T>(title, parameterDict, onCloseAction);
    }

    public void Show<T>(string title, DialogParameters parameters = null, Action onCloseAction = null)
    {
        Show<T>(title, parameters, onCloseAction);
    }

    public void Show<T>(string title, IEnumerable<KeyValuePair<string, object>> parameters = null, Action onCloseAction = null)
    {
        Show(title, typeof(T), parameters, onCloseAction);
    }

    public void Show(string title, Type componentType, IEnumerable<KeyValuePair<string, object>> parameters = null, Action onCloseAction = null)
    {
        Show(title, BlazorUtils.GetRenderFragment(componentType, parameters));
        InvokeAsync(StateHasChanged);
    }

    public void Show(string title, RenderFragment content, Action onCloseAction = null)
    {
        Title = title;
        Content = content;
        Visiable = true;
        OnCloseAction = onCloseAction;
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
        OnCloseAction?.Invoke();
    }

    public void SwitchDialogSizeExtraLarge()
    {
        DialogSizeExtraLarge = !DialogSizeExtraLarge;
    }
}
