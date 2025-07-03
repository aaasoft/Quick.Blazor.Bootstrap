using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
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
        string dir = null;
        if (!string.IsNullOrEmpty(selectedPath))
            dir = Path.GetDirectoryName(selectedPath);
        modalWindow.Show(Field.Name, new DialogParameters<FileSelectControl>
        {
            {x=>x.FileFilter, Field.InputFile_FileFilter},
            {x=>x.Dir, dir},
            {x=>x.SelectedPath, selectedPath},
            {x=>x.SelectAction, new Action<string>(e =>
                {
                    Field.Value = e;
                    modalWindow.Close();
                    InvokeAsync(StateHasChanged);
                })
            }
        });
    }
}
