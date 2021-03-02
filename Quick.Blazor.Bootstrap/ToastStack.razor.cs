using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class ToastStack
    {
        private Dictionary<string, ToastModel> dict = new Dictionary<string, ToastModel>();

        [Parameter]
        public bool AutoClose { get; set; } = true;
        [Parameter]
        public int AutoCloseTime { get; set; } = 5000;

        public class ToastModel
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public string Time { get; set; }
            public BackgroundTheme ToastBg { get; set; }
        }

        public string AddToast(string title, string content, BackgroundTheme toastBg = BackgroundTheme.info)
        {
            var id = Guid.NewGuid().ToString("N");
            var model = new ToastModel()
            {
                Title = title,
                Content = content,
                Time = DateTime.Now.ToShortTimeString(),
                ToastBg = toastBg
            };
            lock (dict)
                dict.Add(id, model);
            //如果配置了自动关闭
            if (AutoClose)
            {
                Task.Delay(AutoCloseTime).ContinueWith(t =>
                {
                    CloseToast(id);
                });
            }
            InvokeAsync(StateHasChanged);
            return id;
        }

        public void CloseToast(string id)
        {
            lock (dict)
                if (dict.ContainsKey(id))
                    dict.Remove(id);
            InvokeAsync(StateHasChanged);
        }
    }
}
