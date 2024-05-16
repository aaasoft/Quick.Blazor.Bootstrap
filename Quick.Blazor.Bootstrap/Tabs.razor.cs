using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class Tabs : ComponentBase
    {
        [Parameter]
        public string ActiveKey { get; set; }
        [Parameter]
        public RenderFragment ChildContent { get; set; }
        [Parameter]
        public EventCallback<string> OnTabClick { get; set; }
        [Parameter]
        public EventCallback<string> ActiveKeyChanged { get; set; }
        [Parameter]
        public EventCallback<string> OnChange { get; set; }
        [Parameter]
        public bool IsNavPills { get; set; } = false;
        [Parameter]
        public string NavTabsExtraClass { get; set; }
        [Parameter]
        public string TabContentExtraClass { get; set; }

        internal List<TabPane> _panes = new List<TabPane>();

        private string _activeKey = null;
        private TabPane _activePane = null;

        internal void AddTabPane(TabPane tabPane)
        {
            if (string.IsNullOrEmpty(tabPane.Key))
            {
                throw new ArgumentNullException(nameof(tabPane), Locale.Catalog.GetString("TabPane's Key is null"));
            }

            if (_panes.Select(p => p.Key).Contains(tabPane.Key))
            {
                throw new ArgumentException(Locale.Catalog.GetString("An TabPane with the same key already exists"));
            }
            _panes.Add(tabPane);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (!firstRender || _panes.Count == 0)
                return;

            TabPane panel = null;
            if (!string.IsNullOrEmpty(ActiveKey))
                panel = _panes.FirstOrDefault(t => t.Key == ActiveKey);
            if (panel == null)
                panel = _panes.First();
            ActivatePane(panel);
        }

        private void ActivatePane(TabPane tabPane)
        {
            if (!tabPane.Disabled && _panes.Contains(tabPane))
            {
                if (_activePane != null)
                {
                    _activePane.IsActive = false;
                }
                tabPane.IsActive = true;
                _activePane = tabPane;
                if (_activeKey != _activePane.Key)
                {
                    if (!string.IsNullOrEmpty(_activeKey))
                    {
                        if (ActiveKeyChanged.HasDelegate)
                        {
                            ActiveKeyChanged.InvokeAsync(_activePane.Key);
                        }

                        if (OnChange.HasDelegate)
                        {
                            OnChange.InvokeAsync(_activePane.Key);
                        }
                    }

                    _activeKey = _activePane.Key;
                }
                StateHasChanged();
            }
        }

        internal void HandleTabClick(TabPane tabPane)
        {
            if (tabPane.IsActive)
                return;

            if (OnTabClick.HasDelegate)
            {
                OnTabClick.InvokeAsync(tabPane.Key);
            }

            ActivatePane(tabPane);
        }
    }
}
