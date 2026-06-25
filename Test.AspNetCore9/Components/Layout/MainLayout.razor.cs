using System;
using Quick.Blazor.Bootstrap;
using Quick.Localize;

namespace Test.AspNetCore9.Components.Layout;

public partial class MainLayout : LayoutComponentBase_WithGettextSupport
{
    public static string TextHome => Locale<MainLayout>.GetString("Home");
    public static string TextTabs => Locale<MainLayout>.GetString("Tabs");
    public static string TextPagination => Locale<MainLayout>.GetString("Pagination");
    public static string TextTree => Locale<MainLayout>.GetString("Tree");
    public static string TextAccordion => Locale<MainLayout>.GetString("Accordion");
    public static string TextFileManage => Locale<MainLayout>.GetString("FileManage");
    public static string TextFileSelect => Locale<MainLayout>.GetString("FileSelect");
    public static string TextProcessManage => Locale<MainLayout>.GetString("ProcessManage");
    public static string TextTerminal => Locale<MainLayout>.GetString("Terminal");
    public static string TextLogView => Locale<MainLayout>.GetString("LogView");
    public static string TextReverseProxy => Locale<MainLayout>.GetString("ReverseProxy");
    
}
