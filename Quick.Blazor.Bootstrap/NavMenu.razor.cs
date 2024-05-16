using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class NavMenu : ComponentBase
    {
        [Parameter]
        public EventCallback OnTitleClick { get; set; }
        [Parameter]
        public RenderFragment Title { get; set; }
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
        public string NavTabsExtraClass { get; set; }
        [Parameter]
        public string TabContentExtraClass { get; set; }

        /// <summary>
        /// 改变激活的标识
        /// </summary>
        /// <param name="activeKey"></param>
        public void ChangeActiveKey(string activeKey)
        {
            var pane = _panes.FirstOrDefault(t => t.Key == activeKey);
            if (pane == null)
                return;
            ActivatePane(pane);
        }

        internal List<NavMenuItem> _panes = new List<NavMenuItem>();

        private string _activeKey = null;
        private NavMenuItem _activePane = null;

        private bool collapseNavMenu = true;

        private string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        internal void AddTabPane(NavMenuItem tabPane)
        {
            if (string.IsNullOrEmpty(tabPane.Key))
            {
                throw new ArgumentNullException(nameof(tabPane), LocaleUtils.Catalog.GetString("NavMenuItem's Key is null"));
            }

            if (_panes.Select(p => p.Key).Contains(tabPane.Key))
            {
                throw new ArgumentException(LocaleUtils.Catalog.GetString("An NavMenuItem with the same key already exists"));
            }
            _panes.Add(tabPane);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (!firstRender || _panes.Count == 0)
                return;

            NavMenuItem panel = null;
            if (!string.IsNullOrEmpty(ActiveKey))
                panel = _panes.FirstOrDefault(t => t.Key == ActiveKey);
            if (panel == null)
                panel = _panes.First();
            ActivatePane(panel);
        }

        private void ActivatePane(NavMenuItem tabPane)
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

        internal void HandleTabClick(NavMenuItem tabPane)
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
