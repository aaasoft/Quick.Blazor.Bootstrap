using System;
using System.IO;
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
        var selectedPath = Field.Value;
        string dir= null;
        if(!string.IsNullOrEmpty(selectedPath))
            dir = Path.GetDirectoryName(selectedPath);
        modalWindow.Show<FileSelectControl>(Field.Name, new System.Collections.Generic.Dictionary<string, object>()
        {
            [nameof(FileSelectControl.FileFilter)] = Field.InputFile_FileFilter,
            [nameof(FileSelectControl.Dir)] = dir,
            [nameof(FileSelectControl.SelectedPath)] = selectedPath,
            [nameof(FileSelectControl.SelectAction)] = new Action<string>(e =>
            {
                Field.Value = e;
                modalWindow.Close();
                InvokeAsync(StateHasChanged);
            })
        });
    }
}
