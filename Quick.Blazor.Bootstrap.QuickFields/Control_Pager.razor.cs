using System;
using Microsoft.AspNetCore.Components;
using Quick.Fields;
using Quick.Localize;

namespace Quick.Blazor.Bootstrap.QuickFields;

public partial class Control_Pager : ComponentBase_WithGettextSupport
{
    private static string TextFirst => Locale.GetString("First");
    private static string TextPrev => Locale.GetString("Prev");
    private static string TextNext => Locale.GetString("Next");
    private static string TextLast => Locale.GetString("Last");
    private static string TextRecords => Locale.GetString("Records:");
    private static string TextPages => Locale.GetString("Pages:");

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
        get { return int.Parse(Field.Value); }
        set { Field.Value = value.ToString(); }
    }
}
