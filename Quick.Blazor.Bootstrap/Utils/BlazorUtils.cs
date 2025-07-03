using Microsoft.AspNetCore.Components;

namespace Quick.Blazor.Bootstrap.Utils
{
    public class BlazorUtils
    {
        public static RenderFragment GetRenderFragment<T>(Dictionary<string, object> parameterDict = null)
        {
            return GetRenderFragment(typeof(T), parameterDict);
        }

        public static RenderFragment GetRenderFragment(Type componentType, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            return builder =>
            {
                builder.OpenComponent(0, componentType);
                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        var key = item.Key;
                        var value = item.Value;
                        var pi = componentType.GetProperty(key);
                        if (pi == null)
                            continue;
                        builder.AddAttribute(0, key, value);
                    }
                }
                builder.CloseComponent();
            };
        }
    }
}
