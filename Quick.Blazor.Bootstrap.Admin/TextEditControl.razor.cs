using Microsoft.AspNetCore.Components;
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
        public int Rows { get; set; } = 20;
        [Parameter]
        public string File { get; set; }
        private string _FileEncoding = "UTF-8";
        [Parameter]
        public string FileEncoding
        {
            get { return _FileEncoding; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _FileEncoding = value;
                _ = loadFileContent();
            }
        }

        [Parameter]
        public Dictionary<string, Encoding> EncodingDict { get; set; }

        private string TextSuccess => Locale.Catalog.GetString("Success");
        private string TextFailed => Locale.Catalog.GetString("Failed");
        private string TextRows => Locale.Catalog.GetString("Rows");
        private string TextEncoding => Locale.Catalog.GetString("Encoding");
        [Parameter]
        public string IconSave { get; set; } = "fa fa-save";

        private Exception OpenException;
        private string Content { get; set; }

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
