using System;
using Quick.Blazor.Bootstrap;

namespace TestWebApplication.Shared;

public partial class MainLayout : LayoutComponentBase_WithGettextSupport
{
    public static string TextHome => Locale.Catalog.GetString("Home");
    public static string TextTabs => Locale.Catalog.GetString("Tabs");
    public static string TextPagination => Locale.Catalog.GetString("Pagination");
    public static string TextTree => Locale.Catalog.GetString("Tree");
    public static string TextAccordion => Locale.Catalog.GetString("Accordion");
    public static string TextFileManage => Locale.Catalog.GetString("FileManage");
    public static string TextProcessManage => Locale.Catalog.GetString("ProcessManage");
    public static string TextTerminal => Locale.Catalog.GetString("Terminal");
    public static string TextLogView => Locale.Catalog.GetString("LogView");
    public static string TextReverseProxy => Locale.Catalog.GetString("ReverseProxy");
    
}
