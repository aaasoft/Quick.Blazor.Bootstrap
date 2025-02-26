using System.Text;
using Quick.Fields;

namespace Quick.Blazor.Bootstrap.QuickFields;

internal static class ControlUtils
{
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
        return sb.ToString();
    }
}
