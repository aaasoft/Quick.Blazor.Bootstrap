using System;
using Quick.Blazor.Bootstrap;
using Quick.Localize;

namespace Test.AspNetCore9.Components.Layout;

public partial class MainLayout : LayoutComponentBase_WithGettextSupport
{
    public static string TextHome => Locale.GetString("Home");
    public static string TextTabs => Locale.GetString("Tabs");
    public static string TextPagination => Locale.GetString("Pagination");
    public static string TextTree => Locale.GetString("Tree");
    public static string TextAccordion => Locale.GetString("Accordion");
    public static string TextFileManage => Locale.GetString("FileManage");
    public static string TextFileSelect => Locale.GetString("FileSelect");
    public static string TextProcessManage => Locale.GetString("ProcessManage");
    public static string TextTerminal => Locale.GetString("Terminal");
    public static string TextLogView => Locale.GetString("LogView");
    public static string TextReverseProxy => Locale.GetString("ReverseProxy");
    
}
