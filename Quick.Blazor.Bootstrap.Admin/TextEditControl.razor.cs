using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class TextEditControl : ComponentBase
    {
        private ModalAlert modalAlert;
        [Parameter]
        public int Rows { get; set; } = 20;
        [Parameter]
        public string File { get; set; }
        private string _FileEncoding = "UTF-8";
        private Encoding FileEncodingObj = Encoding.UTF8;
        [Parameter]
        public string FileEncoding
        {
            get { return _FileEncoding; }
            set
            {
                _FileEncoding = value;
                FileEncodingObj = Encoding.GetEncoding(value);
                _ = loadFileContent();
            }
        }

        public string[] AllEncodings = new[] { "UTF-8", "GB18030", "Unicode" };

        [Parameter]
        public string TextSuccess { get; set; } = "Success";
        [Parameter]
        public string TextFailed { get; set; } = "Failed";
        [Parameter]
        public string TextRows { get; set; } = "Rows";
        [Parameter]
        public string TextEncoding { get; set; } = "Encoding";
        [Parameter]
        public RenderFragment IconSave { get; set; }

        private Exception OpenException;
        private string Content { get; set; }

        private async Task loadFileContent()
        {
            try
            {
                Content = await System.IO.File.ReadAllTextAsync(File, FileEncodingObj);
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
            await loadFileContent();
        }

        private void Save()
        {
            try
            {
                System.IO.File.WriteAllText(File, Content, FileEncodingObj);
                modalAlert.Show(TextSuccess, File);
            }
            catch (Exception ex)
            {
                modalAlert.Show(TextFailed, ex.Message);
            }
        }
    }
}
