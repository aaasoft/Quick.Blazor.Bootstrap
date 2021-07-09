using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class NavMenuItem : ComponentBase
    {
        [Parameter]
        public EventCallback OnClick { get; set; }

        [Parameter]
        public string Key { get; set; }
        [Parameter]
        public RenderFragment Tab { get; set; }
        [Parameter]
        public RenderFragment ChildContent { get; set; }
        [Parameter]
        public bool Disabled { get; set; }
        [Parameter]
        public bool Closable { get; set; }

        private bool _isActive;
        internal bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnClick.InvokeAsync();
                }
            }
        }
        [CascadingParameter(Name = "IsEmpty")]
        private bool IsEmpty { get; set; }
        [CascadingParameter(Name = "Pane")]
        private NavMenuItem Pane { get; set; }
        [CascadingParameter]
        private NavMenu Parent { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (IsEmpty)
            {
                Parent?.AddTabPane(this);
            }
        }
    }
}
