using System.Reflection;
using GetText;

namespace Quick.Blazor.Bootstrap
{
    internal class Locale
    {
        public static ICatalog Catalog { get; } = Quick.Localize.GettextResourceManager.GetResourceManager(Assembly.GetExecutingAssembly()).GetCatalog();
    }
}