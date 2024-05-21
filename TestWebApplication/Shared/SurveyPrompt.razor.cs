using System;
using Quick.Blazor.Bootstrap;

namespace TestWebApplication.Shared;

public partial class SurveyPrompt:ComponentBase_WithGettextSupport
{
    public static string TextPleaseTakeOur=> Locale.Catalog.GetString("Please take our");
    public static string TextBriefSurvey=> Locale.Catalog.GetString("brief survey");
    public static string TextAndTellUsWhatYouThink => Locale.Catalog.GetString("and tell us what you think.");
}
