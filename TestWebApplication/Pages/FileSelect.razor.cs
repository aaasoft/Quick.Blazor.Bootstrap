using System;
using System.IO;
using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin;
using Quick.Localize;

namespace TestWebApplication.Pages;

public partial class FileSelect : ComponentBase_WithGettextSupport
{
    private ModalWindow modalWindow;
    private string filePath;

    private static string TextZipFile => Locale.GetString("Zip File");
    private static string TextSelectFile => Locale.GetString("Select file");

    private void selectFile()
    {
        string dir= null;
        if(!string.IsNullOrEmpty(filePath))
            dir = Path.GetDirectoryName(filePath);
        modalWindow.Show<FileSelectControl>(TextSelectFile, new System.Collections.Generic.Dictionary<string, object>()
        {
            [nameof(FileSelectControl.FileFilter)] = "*.zip",
            [nameof(FileSelectControl.Dir)] = dir,
            [nameof(FileSelectControl.SelectedPath)] = filePath,
            [nameof(FileSelectControl.SelectAction)] = new Action<string>(e =>
            {
                filePath = e;
                modalWindow.Close();
                InvokeAsync(StateHasChanged);
            })
        });
    }
}
