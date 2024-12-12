using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Quick.Localize;
using System;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class FileSelectControl : ComponentBase_WithGettextSupport
    {
        private static string TextSelect => Locale.GetString("Select");

        [Parameter]
        public string Dir { get; set; }
        [Parameter]
        public string FileFilter { get; set; }
        [Parameter]
        public string SelectedPath { get; set; }
        [Parameter]
        public Action<string> SelectAction { get; set; }
        [Parameter]
        public Action<string> FileDoubleClickCustomAction { get; set; }

        private FileManageControl fileManageControl;


        private void Select()
        {
            SelectAction?.Invoke(fileManageControl.SelectedPath);
        }

        private void onFileDoubleClick(IJSRuntime jsRuntime)
        {
            if (FileDoubleClickCustomAction == null)
                Select();
            else
                FileDoubleClickCustomAction?.Invoke(fileManageControl.SelectedPath);
        }
    }
}
