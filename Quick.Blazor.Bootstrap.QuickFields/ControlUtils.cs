using System.Text;
using Quick.Fields;

namespace Quick.Blazor.Bootstrap.QuickFields;

internal static class ControlUtils
{
    private static void appendMargin(StringBuilder sb, FieldForGet field)
    {
        if (field.Margin.HasValue)
            sb.Append(" m-" + field.Margin.Value);
        if (field.MarginLeft.HasValue)
            sb.Append(" ml-" + field.MarginLeft.Value);
        if (field.MarginTop.HasValue)
            sb.Append(" mt-" + field.MarginTop.Value);
        if (field.MarginRight.HasValue)
            sb.Append(" mr-" + field.MarginRight.Value);
        if (field.MarginBottom.HasValue)
            sb.Append(" mb-" + field.MarginBottom.Value);
    }

    private static void appendPadding(StringBuilder sb, FieldForGet field)
    {
        if (field.Padding.HasValue)
            sb.Append(" p-" + field.Padding.Value);
        if (field.PaddingLeft.HasValue)
            sb.Append(" pl-" + field.PaddingLeft.Value);
        if (field.PaddingTop.HasValue)
            sb.Append(" pt-" + field.PaddingTop.Value);
        if (field.PaddingRight.HasValue)
            sb.Append(" pr-" + field.PaddingRight.Value);
        if (field.PaddingBottom.HasValue)
            sb.Append(" pb-" + field.PaddingBottom.Value);
    }

    public static string GetCommonClass(FieldForGet field)
    {
        if (!string.IsNullOrEmpty(field.Html_Class))
            return field.Html_Class;
        var sb = new StringBuilder();
        appendMargin(sb, field);
        appendPadding(sb, field);
        return sb.ToString();
    }

    public static string GetButtonClass(FieldForGet field)
    {
        if (!string.IsNullOrEmpty(field.Html_Class))
            return field.Html_Class;
        var sb = new StringBuilder();
        sb.Append("btn");
        //显示主题
        var fieldTheme = FieldTheme.Secondary;
        if (field.Theme.HasValue)
            fieldTheme = field.Theme.Value;
        sb.Append(" btn-");
        if (field.InputButton_IsOutline.HasValue && field.InputButton_IsOutline.Value)
            sb.Append("outline-");
        sb.Append(fieldTheme.ToString().ToLower());
        //显示尺寸
        if (field.Input_IsSmall.HasValue && field.Input_IsSmall.Value)
            sb.Append(" btn-sm");
        if (field.Input_IsLarge.HasValue && field.Input_IsLarge.Value)
            sb.Append(" btn-lg");
        appendMargin(sb, field);
        appendPadding(sb, field);
        return sb.ToString();
    }

    public static string GetInputClass(FieldForGet field)
    {
        if (!string.IsNullOrEmpty(field.Html_Class))
            return field.Html_Class;
        var sb = new StringBuilder();
        sb.Append("form-control");
        if (field.Input_IsSmall.HasValue && field.Input_IsSmall.Value)
            sb.Append(" form-control-sm");
        if (field.Input_IsLarge.HasValue && field.Input_IsLarge.Value)
            sb.Append(" form-control-lg");
        if (!string.IsNullOrEmpty(field.Input_ValidationMessage))
            sb.Append(" invalid");
        appendMargin(sb, field);
        appendPadding(sb, field);
        return sb.ToString();
    }

    public static string GetAlertClass(FieldForGet field)
    {
        if (!string.IsNullOrEmpty(field.Html_Class))
            return field.Html_Class;
        var sb = new StringBuilder();
        sb.Append("alert");
        if (field.Theme.HasValue)
        {
            sb.Append(" alert-");
            sb.Append(field.Theme.Value.ToString().ToLower());
        }
        appendMargin(sb, field);
        appendPadding(sb, field);
        return sb.ToString();
    }

    public static string GetTableClass(FieldForGet field)
    {
        if (!string.IsNullOrEmpty(field.Html_Class))
            return field.Html_Class;
        var sb = new StringBuilder();
        sb.Append("table");
        if (field.Theme.HasValue)
        {
            sb.Append(" table-");
            sb.Append(field.Theme.Value.ToString().ToLower());
        }
        if (field.Input_IsSmall.HasValue && field.Input_IsSmall.Value)
            sb.Append(" table-sm");
        if (field.Input_IsLarge.HasValue && field.Input_IsLarge.Value)
            sb.Append(" table-lg");
        if (field.ContainerTable_Striped.HasValue && field.ContainerTable_Striped.Value)
            sb.Append(" table-striped");
        if (field.ContainerTable_Bordered.HasValue)
            if (field.ContainerTable_Bordered.Value)
                sb.Append(" table-bordered");
            else
                sb.Append(" table-borderless");
        if (field.ContainerTable_Hoverable.HasValue && field.ContainerTable_Hoverable.Value)
            sb.Append(" table-hover");
        appendMargin(sb, field);
        appendPadding(sb, field);
        return sb.ToString();
    }

    public static string GetTableHeadClass(FieldForGet field)
    {
        if (!string.IsNullOrEmpty(field.Html_Class))
            return field.Html_Class;
        var sb = new StringBuilder();
        if (field.Theme.HasValue)
        {
            sb.Append("thead-");
            sb.Append(field.Theme.Value.ToString().ToLower());
        }
        appendMargin(sb, field);
        appendPadding(sb, field);
        return sb.ToString();
    }
}
