using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.ReverseProxy.Model;
using System;
using System.Collections.Generic;

namespace Quick.Blazor.Bootstrap.ReverseProxy.Controls
{
    public partial class ReverseProxyRuleCreateControl : ComponentBase
    {
        private CreateReverseProxyRuleModel createModel = new CreateReverseProxyRuleModel();

        [Parameter]
        public ReverseProxyRule Model { get; set; }
        [Parameter]
        public Action<CreateReverseProxyRuleModel> OkAction { get; set; }
        [Parameter]
        public string TextName { get; set; }
        [Parameter]
        public string TextPath { get; set; }
        [Parameter]
        public string TextUrl { get; set; }
        [Parameter]
        public string TextOK { get; set; }

        private void Ok()
        {
            OkAction?.Invoke(createModel);
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (Model != null)
            {
                createModel.Name = Model.Name;
                createModel.Path = Model.Path;
                createModel.Url = Model.Url;
            }
        }
    }
}
