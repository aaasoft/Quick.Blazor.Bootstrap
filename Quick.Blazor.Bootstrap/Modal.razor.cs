using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class Modal
    {
        [Parameter]
        public bool Visiable { get; set; }
        [Parameter]
        public RenderFragment TitleContent { get; set; }
        [Parameter]
        public RenderFragment HeaderContent { get; set; }
        [Parameter]
        public RenderFragment BodyContent { get; set; }
        [Parameter]
        public RenderFragment FooterContent { get; set; }
        [Parameter]
        public bool DialogCentered { get; set; }
        [Parameter]
        public bool DialogScrollable { get; set; }
        [Parameter]
        public bool DialogBackdropVisiable { get; set; }
    }
}
