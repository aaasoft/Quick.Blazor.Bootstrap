using System;
using Quick.Blazor.Bootstrap;

namespace TestWebApplication.Pages;

public partial class Index : ComponentBase_WithGettextSupport
{
    public static string TextWelcomeToUseQuickBlazorBootstrap => Locale.Catalog.GetString("Welcome to use Quick.Blazor.Bootstrap.");
    public static string TextHowIsBlazorWorkingForYou => Locale.Catalog.GetString("How is Blazor working for you?");
}
