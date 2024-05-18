using System.Reflection;
using GetText;
using Quick.Localize;

namespace Quick.Blazor.Bootstrap
{
    internal class Locale
    {
        private static GettextResourceManager _GettextResourceManager;
        private static ICatalog _Catalog;
        public static ICatalog Catalog
        {
            get
            {
                if (_GettextResourceManager == null)
                {
                    _GettextResourceManager = GettextResourceManager.GetResourceManager(Assembly.GetExecutingAssembly());
                    GettextResourceManager.CurrentCultureChanged += (sender, e) =>
                    {
                        _Catalog = _GettextResourceManager.GetCatalog();
                    };
                }
                if (_Catalog == null)
                    _Catalog = _GettextResourceManager.GetCatalog();
                return _Catalog;
            }
        }
    }
}