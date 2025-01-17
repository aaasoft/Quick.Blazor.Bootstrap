using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Quick.Fields;

namespace Quick.Blazor.Bootstrap.QuickFields;

public partial class Controls : ComponentBase
{
    private ModalAlert modalAlert;
    private ToastStack toastStack;

    private FieldForGet[] Fields;

    [Parameter]
    public Action<FieldForGet, FieldForGet[]> OnFieldChangedAction { get; set; }

    public Dictionary<string, object> PrepareParameters(Action<FieldForGet, FieldForGet[]> onFieldChangedAction)
    {
        return new Dictionary<string, object>()
        {
            [nameof(OnFieldChangedAction)] = onFieldChangedAction
        };
    }

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
                    modalAlert.Show(field.Name, field.Description, null, null);
                    return;
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
