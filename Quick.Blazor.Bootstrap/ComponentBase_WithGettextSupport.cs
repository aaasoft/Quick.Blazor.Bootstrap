using System;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Quick.Localize;

namespace Quick.Blazor.Bootstrap
{
    public abstract class ComponentBase_WithGettextSupport : ComponentBase, IDisposable
    {
        public ComponentBase_WithGettextSupport()
        {
            GettextResourceManager.CurrentCultureChanged += GettextResourceManager_CurrentCultureChanged;
        }

        private void GettextResourceManager_CurrentCultureChanged(object sender, CultureInfo e)
        {
            InvokeAsync(StateHasChanged);
        }

        public virtual void Dispose()
        {
            GettextResourceManager.CurrentCultureChanged -= GettextResourceManager_CurrentCultureChanged;
        }
    }
}