using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin;
using Quick.Localize;

namespace Test.AspNetCore9.Components.Pages;

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
        modalWindow.Show(TextSelectFile, new DialogParameters<FileSelectControl>()
        {
            {x=>x.FileFilter,"*.zip"},
            { x=>x.BaseDir,"C:\\Code"},
            { x=>x.Dir,dir},
            {x=>x.SelectedPath,filePath},
            {x=>x.SelectAction,e =>
                {
                    filePath = e;
                    modalWindow.Close();
                    InvokeAsync(StateHasChanged);
                }
            },
        });
    }
}
