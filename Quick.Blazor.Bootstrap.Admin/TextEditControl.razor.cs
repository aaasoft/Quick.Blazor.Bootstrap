using GaelJ.BlazorCodeMirror6.Models;
using Microsoft.AspNetCore.Components;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class TextEditControl : ComponentBase_WithGettextSupport
    {
        private ModalAlert modalAlert;
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
        private static string TextHeidht => Locale.GetString("Height");
        private static string TextEncoding => Locale.GetString("Encoding");
        private static string TextTheme => Locale.GetString("Theme");
        private static string TextLanguage => Locale.GetString("Language");

        [Parameter]
        public string IconSave { get; set; } = "fa fa-save";

        private CodeMirrorLanguage _Language = CodeMirrorLanguage.PlainText;
        public CodeMirrorLanguage Language
        {
            get { return _Language; }
            set { _Language = value; }
        }
        private ThemeMirrorTheme Theme = ThemeMirrorTheme.OneDark;

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
        private CodeMirrorLanguage detectLanguage()
        {
            var fileName = Path.GetFileName(File);
            switch (fileName)
            {
                case "CMakeLists.txt":
                    return CodeMirrorLanguage.CMake;
                case "Dockerfile":
                    return CodeMirrorLanguage.Dockerfile;
            }
            return Path.GetExtension(fileName).ToLower() switch
            {
                ".cs" => CodeMirrorLanguage.Csharp,
                ".c" => CodeMirrorLanguage.C,
                ".cpp" => CodeMirrorLanguage.Cpp,
                ".java" => CodeMirrorLanguage.Java,
                ".php" => CodeMirrorLanguage.Php,
                ".js" => CodeMirrorLanguage.Javascript,
                ".css" => CodeMirrorLanguage.Css,
                ".ts" => CodeMirrorLanguage.TypeScript,
                ".sql" => CodeMirrorLanguage.Sql,
                ".rs" => CodeMirrorLanguage.Rust,
                ".lua" => CodeMirrorLanguage.Lua,
                ".csv" => CodeMirrorLanguage.Csv,
                ".fs" => CodeMirrorLanguage.Fsharp,
                ".go" => CodeMirrorLanguage.Go,
                ".html" => CodeMirrorLanguage.Html,
                ".yaml" => CodeMirrorLanguage.Yaml,
                ".vue" => CodeMirrorLanguage.Vue,
                ".vbs" => CodeMirrorLanguage.VbScript,
                ".json" => CodeMirrorLanguage.Json,
                ".md" => CodeMirrorLanguage.Markdown,
                ".ps1" => CodeMirrorLanguage.PowerShell,
                ".py" => CodeMirrorLanguage.Python,
                ".sass" => CodeMirrorLanguage.Sass,
                ".sh" => CodeMirrorLanguage.Shell,
                ".xml" => CodeMirrorLanguage.Xml,
                _ => CodeMirrorLanguage.PlainText
            };
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
            Language = detectLanguage();
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
