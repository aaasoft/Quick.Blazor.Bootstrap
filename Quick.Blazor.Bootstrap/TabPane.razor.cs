﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class TabPane : ComponentBase
    {
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
                }
            }
        }
        [CascadingParameter(Name = "IsEmpty")]
        private bool IsEmpty { get; set; }

        [CascadingParameter(Name = "IsTab")]
        private bool IsTab { get; set; }

        [CascadingParameter(Name = "IsPane")]
        private bool IsPane { get; set; }
        [CascadingParameter(Name = "Pane")]
        private TabPane Pane { get; set; }
        [CascadingParameter]
        private Tabs Parent { get; set; }

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
