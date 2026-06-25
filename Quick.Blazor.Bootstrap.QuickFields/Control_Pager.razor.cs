using Microsoft.AspNetCore.Components;
using Quick.Fields;
using Quick.Localize;

namespace Quick.Blazor.Bootstrap.QuickFields;

public partial class Control_Pager : ComponentBase_WithGettextSupport
{
    private static string TextFirst => Locale<Control_Pager>.GetString("First");
    private static string TextPrev => Locale<Control_Pager>.GetString("Prev");
    private static string TextNext => Locale<Control_Pager>.GetString("Next");
    private static string TextLast => Locale<Control_Pager>.GetString("Last");
    private static string TextRecords => Locale<Control_Pager>.GetString("Records:");
    private static string TextPages => Locale<Control_Pager>.GetString("Pages:");

    [Parameter]
    public FieldForGet Field { get; set; }

    public int PageSize
    {
        get { return Field.Pager_PageSize ?? 0; }
        set
        {
            Field.Pager_PageSize = value;
        }
    }

    public int RecordCount
    {
        get { return Field.Pager_RecordCount ?? 0; }
        set
        {
            Field.Pager_RecordCount = value;
        }
    }

    private int Offset
    {
        get
        {
            if (!string.IsNullOrEmpty(Field.Value)
                && int.TryParse(Field.Value, out var ret))
                return ret;
            return 0;
        }
        set { Field.Value = value.ToString(); }
    }
}
