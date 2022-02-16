using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class Accordion
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
        public string NavTabsExtraClass { get; set; }
        [Parameter]
        public string TabContentExtraClass { get; set; }

        private bool _AllowMultiplePaneActived = false;
        [Parameter]
        public bool AllowMultiplePanelActived
        {
            get { return _AllowMultiplePaneActived; }
            set
            {
                _AllowMultiplePaneActived = value;
                if (!value)
                    foreach (var pane in _panes)
                        pane.IsActive = false;
            }
        }

        internal List<AccordionPane> _panes = new List<AccordionPane>();

        private string _activeKey = null;
        private AccordionPane _activePane = null;

        internal void AddTabPane(AccordionPane tabPane)
        {
            if (string.IsNullOrEmpty(tabPane.Key))
            {
                throw new ArgumentNullException(nameof(tabPane), "Key is null");
            }

            if (_panes.Select(p => p.Key).Contains(tabPane.Key))
            {
                throw new ArgumentException("An AccordionPane with the same key already exists");
            }
            _panes.Add(tabPane);
            if (AllowMultiplePanelActived)
                tabPane.IsActive = true;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (!firstRender || _panes.Count == 0)
                return;

            AccordionPane panel = null;
            if (!string.IsNullOrEmpty(ActiveKey))
                panel = _panes.FirstOrDefault(t => t.Key == ActiveKey);
            if (panel == null)
                panel = _panes.First();
            ActivatePane(panel);
        }

        private void ActivatePane(AccordionPane tabPane)
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

        internal void HandleTabClick(AccordionPane tabPane)
        {
            if (AllowMultiplePanelActived)
            {
                tabPane.IsActive = !tabPane.IsActive;
            }
            else
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
}
