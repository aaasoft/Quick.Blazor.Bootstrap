using Quick.Blazor.Bootstrap;
using Quick.Localize;

namespace Test.AspNetCore9.Components.Layout;

public partial class SurveyPrompt : ComponentBase_WithGettextSupport
{
    public static string TextPleaseTakeOur => Locale<SurveyPrompt>.GetString("Please take our");
    public static string TextBriefSurvey => Locale<SurveyPrompt>.GetString("brief survey");
    public static string TextAndTellUsWhatYouThink => Locale<SurveyPrompt>.GetString("and tell us what you think.");
}
