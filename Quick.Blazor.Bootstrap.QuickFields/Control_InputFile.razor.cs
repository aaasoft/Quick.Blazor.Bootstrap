using System;
using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Admin;
using Quick.Fields;

namespace Quick.Blazor.Bootstrap.QuickFields;

public partial class Control_InputFile : ComponentBase
{
    private ModalWindow modalWindow;

    [Parameter]
    public FieldForGet Field { get; set; }

    private void selectFile()
    {
        modalWindow.Show<FileSelectControl>(Field.Name, new System.Collections.Generic.Dictionary<string, object>()
        {
            [nameof(FileSelectControl.FileFilter)] = Field.InputFile_FileFilter,
            [nameof(FileSelectControl.SelectedPath)] = Field.Value,
            [nameof(FileSelectControl.SelectAction)] = new Action<string>(e => Field.Value = e)
        });
    }
}
