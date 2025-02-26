using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Quick.Fields;

namespace Quick.Blazor.Bootstrap.QuickFields;

public partial class Controls : ComponentBase
{
    private ModalAlert modalAlert;
    private ToastStack toastStack;

    public FieldForGet[] Fields { get; private set; }

    [Parameter]
    public Action<FieldForGet, FieldForGet[]> OnFieldChangedAction { get; set; }

    public void SetFields(FieldForGet[] fields)
    {
        travelFields(fields, field =>
        {
            if (field.PostOnChanged.HasValue && field.PostOnChanged.Value)
                field.PropertyChanged -= OnFieldValueChanged;
        });
        Fields = fields;
        travelFields(Fields, field =>
        {
            switch (field.Type)
            {
                case FieldType.MessageBox:
                    Action okAction = null;
                    Action cancelAction = null;
                    var usePreTag = field.MessageBox_UsePreTag.HasValue && field.MessageBox_UsePreTag.Value;
                    if (field.PostOnChanged.HasValue && field.PostOnChanged.Value)
                    {
                        var canCancel = field.MessageBox_CanCancel.HasValue && field.MessageBox_CanCancel.Value;
                        okAction = () => field.Value = "OK";
                        if (canCancel)
                            cancelAction = () => field.Value = "CANCEL";
                    }
                    modalAlert.Show(field.Name, field.Description, okAction, cancelAction, usePreTag);
                    break;
                case FieldType.Toast:
                    toastStack.AddToast(field.Name, field.Description, BackgroundTheme.info);
                    return;
                case FieldType.Button:
                    field.PostOnChanged = true;
                    break;
            }
            if (field.PostOnChanged.HasValue && field.PostOnChanged.Value)
                field.PropertyChanged += OnFieldValueChanged;
        });
        InvokeAsync(StateHasChanged);
    }

    private void OnFieldValueChanged(object sender, PropertyChangedEventArgs e)
    {
        var field = (FieldForGet)sender;
        OnFieldChanged(field);
    }

    private void OnFieldChanged(FieldForGet field)
    {
        OnFieldChangedAction?.Invoke(field, Fields);
    }

    public void Dispose()
    {
        if (Fields != null)
            foreach (var field in Fields)
                if (field.PostOnChanged.HasValue && field.PostOnChanged.Value)
                    field.PropertyChanged -= OnFieldValueChanged;
    }

    private void travelFields(FieldForGet[] fields, Action<FieldForGet> action)
    {
        if (fields == null)
            return;
        foreach (var field in fields)
        {
            action.Invoke(field);
            travelFields(field.Children, action);
        }
    }
}
