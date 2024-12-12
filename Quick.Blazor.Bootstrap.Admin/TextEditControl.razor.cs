using Microsoft.AspNetCore.Components;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class TextEditControl : ComponentBase_WithGettextSupport
    {
        private ModalAlert modalAlert;
        [Parameter]
        public RenderFragment ToolbarAddonControls { get; set; }
        [Parameter]
        public int Rows { get; set; } = 30;
        [Parameter]
        public string File { get; set; }
        [Parameter]
        public string FileEncoding { get; set; } = "UTF-8";

        public string FileEncodingForBinding
        {
            get { return FileEncoding; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    FileEncoding = value;
                _ = loadFileContent();
            }
        }

        [Parameter]
        public Dictionary<string, Encoding> EncodingDict { get; set; }

        private static string TextSuccess => Locale.GetString("Success");
        private static string TextFailed => Locale.GetString("Failed");
        private string TextRows => Locale.GetString("Rows");
        private static string TextEncoding => Locale.GetString("Encoding");
        private static string TextTheme => Locale.GetString("Theme");

        [Parameter]
        public string IconSave { get; set; } = "fa fa-save";
        private TextEditControlTheme Theme { get; set; } = TextEditControlTheme.Dark;
        private Exception OpenException;
        private string Content { get; set; }
        
        private string GetThemeClass(TextEditControlTheme theme)
        {
            switch (theme)
            {
                case TextEditControlTheme.Light:
                    return "form-control bg-light text-dark";
                case TextEditControlTheme.Dark:
                    return "form-control bg-dark text-light";
                case TextEditControlTheme.Default:
                default:
                    return "form-control";
            }
        }

        private async Task loadFileContent()
        {
            try
            {
                Content = await System.IO.File.ReadAllTextAsync(File, EncodingDict[FileEncoding]);
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Content = ex.Message;
                OpenException = ex;
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (EncodingDict == null)
            {
                EncodingDict = new Dictionary<string, Encoding>()
                {
                    ["UTF-8"] = new UTF8Encoding(false),
                    ["UTF-8 BOM"] = new UTF8Encoding(true),
                    ["ASCII"] = Encoding.ASCII,
                    ["Latin1"] = Encoding.Latin1,
                    ["Unicode"] = Encoding.Unicode
                };
            }
            await loadFileContent();
        }

        private void Save()
        {
            try
            {
                System.IO.File.WriteAllText(File, Content, EncodingDict[FileEncoding]);
                modalAlert.Show(TextSuccess, File);
            }
            catch (Exception ex)
            {
                modalAlert.Show(TextFailed, ex.Message);
            }
        }
    }
}
