using System;
using Quick.Blazor.Bootstrap;
using Quick.Localize;

namespace TestWebApplication.Pages;

public partial class Index : ComponentBase_WithGettextSupport
{
    public static string TextWelcomeToUseQuickBlazorBootstrap => Locale.GetString("Welcome to use Quick.Blazor.Bootstrap.");
    public static string TextHowIsBlazorWorkingForYou => Locale.GetString("How is Blazor working for you?");
}
