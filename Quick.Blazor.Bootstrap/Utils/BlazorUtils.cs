using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Utils
{
    public class BlazorUtils
    {
        public static RenderFragment GetRenderFragment<T>(Dictionary<string, object> parameterDict = null)
        {
            return GetRenderFragment(typeof(T), parameterDict);
        }

        public static RenderFragment GetRenderFragment(Type componentType, Dictionary<string, object> parameterDict = null)
        {
            return builder =>
            {
                builder.OpenComponent(0, componentType);
                if (parameterDict != null)
                {
                    var currentIndex = 1;
                    foreach (var key in parameterDict.Keys)
                    {
                        var pi = componentType.GetProperty(key);
                        if (pi == null)
                            continue;
                        builder.AddAttribute(currentIndex, key, parameterDict[key]);
                        currentIndex++;
                    }
                }
                builder.CloseComponent();
            };
        }
    }
}
