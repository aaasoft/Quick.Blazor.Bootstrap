using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class Progress : ComponentBase
    {
        [Parameter]
        public bool Visiable { get; set; }
        [Parameter]
        public double Maximum { get; set; } = 100;
        [Parameter]
        public double Minimum { get; set; } = 0;
        [Parameter]
        public double Value { get; set; } = 0;
        [Parameter]
        public string Text { get; set; }
        [Parameter]
        public string StyleHeight { get; set; }
        [Parameter]
        public bool Striped { get; set; }
        [Parameter]
        public bool Animated { get; set; }
        [Parameter]
        public BackgroundTheme? Background { get; set; }

        private string GetClass()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("progress-bar");
            if (Striped)
                sb.Append(" progress-bar-striped");
            if (Animated)
                sb.Append(" progress-bar-animated");
            if (Background.HasValue)
                sb.Append($" bg-{Background.Value}");
            
            return sb.ToString();
        }

        private string GetStyle()
        {
            StringBuilder sb = new StringBuilder();
            //width
            sb.Append($"width:{(Value - Minimum) * 100 / (Maximum - Minimum)}%;");
            //height
            if (!string.IsNullOrEmpty(StyleHeight))
                sb.Append($"height:{StyleHeight}%;");
            //output
            return sb.ToString();
        }
    }
}
