using System.Reflection;
using GetText;

namespace Quick.Blazor.Bootstrap.Utils
{
    internal class LocaleUtils
    {
        public static ICatalog Catalog { get; } = Quick.Localize.GettextResourceManager.GetResourceManager(Assembly.GetExecutingAssembly()).GetCatalog();
    }
}