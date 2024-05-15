using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class LogViewControl : ComponentBase
    {
        [Parameter]
        public int Rows { get; set; } = 20;
        [Parameter]
        public int MaxLines { get; set; } = 1000;
        [Parameter]
        public string Content { get; set; }
        [Parameter]
        public string TextAreaClass { get; set; } = "form-control bg-dark m-0 text-light";

        private Queue<string> logQueue = new Queue<string>();

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private string elementId = "textArea_" + Guid.NewGuid().ToString("N");

        private void scrollToBottom()
        {
            JSRuntime.InvokeVoidAsync("eval",
    @$"this.setTimeout(function () {{
var el = document.getElementById('{elementId}');
    el.scrollTop = el.scrollHeight;
}},100);"
                );
        }

        public void Clear()
        {
            lock (logQueue)
                logQueue.Clear();
            Content = null;
        }

        public void AddLine(string line)
        {
            lock (logQueue)
            {
                while (line.EndsWith("\r"))
                    line = line.Substring(0, line.Length - 1);
                logQueue.Enqueue(line);
                while (logQueue.Count > MaxLines)
                    logQueue.Dequeue();
                Content = string.Join(Environment.NewLine, logQueue);
            }
            InvokeAsync(StateHasChanged);
            scrollToBottom();
        }

        public void SetContent(string content)
        {
            Content = content;
            InvokeAsync(StateHasChanged);
            scrollToBottom();
        }
    }
}
