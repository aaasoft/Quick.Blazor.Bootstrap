using System.Globalization;
using Microsoft.AspNetCore.Components;
using Quick.Localize;

namespace Test.AspNetCore9.Components.Layout;

public partial class TopRow : ComponentBase
{
    public Dictionary<string, string> _LanguageDict = new Dictionary<string, string>();
    public Dictionary<string, string> LanguageDict => _LanguageDict;
    private string _Language;
    public string Language
    {
        get
        {
            return _Language;
        }
        set
        {
            _Language = value;
            GettextResourceManager.ChangeCurrentCulture(CultureInfo.GetCultureInfo(_Language));
        }
    }
    private string getLanguageDisplayName(CultureInfo culture) => culture.NativeName;
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _Language = GettextResourceManager.CurrentCulture.IetfLanguageTag;
        refreshLanguageDict();
    }

    private void refreshLanguageDict()
    {
        _LanguageDict["en-US"] = getLanguageDisplayName(CultureInfo.GetCultureInfo("en-US"));
        _LanguageDict["zh-CN"] = getLanguageDisplayName(CultureInfo.GetCultureInfo("zh-CN"));
        if (!_LanguageDict.ContainsKey(Language))
        {
            _LanguageDict[Language] = getLanguageDisplayName(GettextResourceManager.CurrentCulture);
        }
    }
}
