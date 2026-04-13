using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Quick.Blazor.Bootstrap.Admin.Core;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Localize;
using Quick.Utils;
using SharpCompress.Archives;
using System.IO.Compression;
using System.Text;
using Tewr.Blazor.FileReader;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class FileManageControl : ComponentBase_WithGettextSupport
    {
        private readonly UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        private IBlazorDownloadFileService BlazorDownloadFileService { get; set; }
        [Inject]
        private IFileReaderService fileReaderService { get; set; }

        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ModalPrompt modalPrompt;
        private ModalWindow modalWindow;
        private ElementReference inputFile;
        private ElementReference inputFolder;

        private string CurrentPath;
        private string Search;
        private DirectoryInfo CurrentDir;
        private DirectoryInfo[] Dirs;
        private FileInfo[] Files;

        public event EventHandler SelectedPathChanged;

        private object _SelectedItem;
        private object SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                _SelectedItem = value;
                if (value == null)
                    SelectedPath = null;
                else if (value is FileInfo)
                    SelectedPath = ((FileInfo)value).FullName;
                else if (value is DirectoryInfo)
                    SelectedPath = ((DirectoryInfo)value).FullName;
                else
                    SelectedPath = null;

                if (!string.IsNullOrEmpty(BaseDir) && !string.IsNullOrEmpty(SelectedPath) && SelectedPath.StartsWith(BaseDir))
                    SelectedPath = SelectedPath.Substring(BaseDir.Length + 1).Replace(Path.DirectorySeparatorChar, '/');

                SelectedPathChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Parameter]
        public string BaseDir { get; set; }
        [Parameter]
        public string SelectedPath { get; set; }
        [Parameter]
        public string Dir { get; set; }
        [Parameter]
        public bool ListViewMode { get; set; } = true;
        /// <summary>
        /// 下载文件动作
        /// </summary>
        public static Action<IJSRuntime, string> DownloadFileAction { get; set; }
        [Parameter]
        public Action<IJSRuntime> FileDoubleClickCustomAction { get; set; }
        [Parameter]
        public bool FileDoubleClickToDownload { get; set; } = true;

        [Parameter]
        public bool DisplayFolder { get; set; } = true;
        [Parameter]
        public bool DisplayFile { get; set; } = true;
        [Parameter]
        public bool DisplayAddress { get; set; } = true;
        [Parameter]
        public bool DisplaySerachBox { get; set; } = true;
        [Parameter]
        public bool DisplayCreateFolderButton { get; set; } = true;
        [Parameter]
        public bool DisplayRenameButton { get; set; } = true;
        [Parameter]
        public bool DisplayEditButton { get; set; } = true;
        [Parameter]
        public bool DisplayDeleteButton { get; set; } = true;
        [Parameter]
        public bool DisplayDownloadButton { get; set; } = true;
        [Parameter]
        public bool DisplayUploadButton { get; set; } = true;
        [Parameter]
        public bool DisplayVerifyButton { get; set; } = true;
        [Parameter]
        public bool DisplayCompressButton { get; set; } = true;
        [Parameter]
        public bool DisplayDecompressButton { get; set; } = true;

        [Parameter]
        public string FileFilter { get; set; }
        private string FileFilterForInputFile
        {
            get
            {
                if (string.IsNullOrEmpty(FileFilter))
                    return FileFilter;
                var array = FileFilter.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < array.Length; i++)
                {
                    var v = array[i];
                    if (v.StartsWith("*."))
                        array[i] = v.Substring(1);
                }
                return string.Join(',', array);
            }
        }

        [Parameter]
        public string OrderBy { get; set; } = nameof(FileInfo.Name);
        [Parameter]
        public bool OrderByAsc { get; set; } = true;

        public static string TextLoading => Locale.GetString("Loading");
        public static string TextConfirm => Locale.GetString("Confirm");
        public static string TextConfirmDeleteFolder => Locale.GetString("Do you want to delete folder[{0}]?");
        public static string TextConfirmDeleteFile => Locale.GetString("Do you want to delete file[{0}]?");
        public static string TextInputNewName => Locale.GetString("Please input new name of [{0}]");
        public static string TextSuccess => Locale.GetString("Success");
        public static string TextCanceled => Locale.GetString("Canceled");
        public static string TextFailed => Locale.GetString("Failed");
        public static string TextFolderNotExist => Locale.GetString("Folder [{0}] not exist");
        public static string TextUp => Locale.GetString("Up");
        public static string TextNewFolder => Locale.GetString("New Folder");
        public static string TextNewFolderPrompt => Locale.GetString("Please input new folder name");
        public static string TextCouldNotCreateFolderOutOfBaseDir => Locale.GetString("Could not create folder out of BaseDir");
        public static string TextUpload => Locale.GetString("Upload File");
        public static string TextUploadFolder => Locale.GetString("Upload Folder");
        public static string TextUploadReadFileInfo => Locale.GetString("Reading upload file info...");
        public static string TextUploadFileExistReplace => Locale.GetString("File [{0}] was exist,do you want to replace it?");
        public static string TextUploadFileUploading => Locale.GetString("Uploading file [{0}]...");
        public static string TextRefresh => Locale.GetString("Refresh");
        public static string TextDownload => Locale.GetString("Download");
        public static string TextVerify => Locale.GetString("Verify");
        public static string TextCompress => Locale.GetString("Compress");
        private static string TextDecompress => Locale.GetString("Decompress");

        private static string TextRename => Locale.GetString("Rename");
        private static string TextEdit => Locale.GetString("Edit");
        [Parameter]
        public Dictionary<string, Encoding> EncodingDict { get; set; }

        public static string TextDelete => Locale.GetString("Delete");
        public static string TextPath => Locale.GetString("Path");
        public static string TextLastWriteTime => Locale.GetString("Last Write Time");
        public static string TextSize => Locale.GetString("Size");
        public static string TextName => Locale.GetString("Name");

        [Parameter]
        public RenderFragment ToolbarAddonButtons { get; set; }

        [Parameter]
        public string IconFolder { get; set; } = "fa fa-folder m-1";
        [Parameter]
        public string IconFile { get; set; } = "fa fa-file-o m-1";
        [Parameter]
        public string IconZipFile { get; set; } = "fa fa-file-zip-o m-1";
        [Parameter]
        public string IconUp { get; set; } = "fa fa-arrow-up";
        [Parameter]
        public string IconNewFolder { get; set; } = "fa fa-plus-square";
        [Parameter]
        public string IconUpload { get; set; } = "fa fa-upload";
        [Parameter]
        public string IconUploadFolder { get; set; } = "fa fa-cloud-upload";
        [Parameter]
        public string IconRefresh { get; set; } = "fa fa-refresh";
        [Parameter]
        public string IconDownload { get; set; } = "fa fa-download";
        [Parameter]
        public string IconVerify { get; set; } = "fa fa-check-circle-o";
        [Parameter]
        public string IconCompress { get; set; } = "fa fa-inbox";
        [Parameter]
        public string IconDecompress { get; set; } = "fa fa-dropbox";
        [Parameter]
        public string IconRename { get; set; } = "fa fa-i-cursor";
        [Parameter]
        public string IconEdit { get; set; } = "fa fa-edit";
        [Parameter]
        public string IconSave { get; set; } = "fa fa-save";
        [Parameter]
        public string IconDelete { get; set; } = "fa fa-trash";
        [Parameter]
        public string IconDisplayList { get; set; } = "fa fa-list";
        [Parameter]
        public string IconDisplayIcon { get; set; } = "fa fa-th-large";
        [Parameter]
        public string IconGoto { get; set; } = "fa fa-arrow-right";
        [Parameter]
        public string IconSearch { get; set; } = "fa fa-search";

        public bool IsSelectedFile()
        {
            return SelectedItem != null && SelectedItem is FileInfo;
        }

        private bool isSelectedZipFile()
        {
            return isSelectedZipFile(SelectedItem as FileInfo);
        }

        private string[] compressFileExtensions = new[] { ".zip", ".7z", ".rar", ".tar", ".gz", ".tgz" };

        private bool isSelectedZipFile(FileInfo fileInfo)
        {
            if (fileInfo == null)
                return false;
            var fileExtension = Path.GetExtension(fileInfo.Name);
            return compressFileExtensions.Contains(fileExtension);
        }

        private string getFileLengthString(FileInfo fileInfo)
        {
            try
            {
                if (fileInfo.Exists)
                    return storageUSC.GetString(fileInfo.Length, 2, true);
            }
            catch { }
            return string.Empty;
        }

        private string getLastWriteString(FileSystemInfo fileInfo)
        {
            try
            {
                if (fileInfo.Exists)
                    return fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch { }
            return string.Empty;
        }

        private string getOrderByButtonIconClass(string field)
        {
            if (field != OrderBy)
                return null;
            if (OrderByAsc)
                return "fa fa-caret-up";
            return "fa fa-caret-down";
        }

        private void changeOrderByAscOrDesc(string orderBy)
        {
            var isOrderByChanged = OrderBy != orderBy;
            if (isOrderByChanged)
            {
                OrderBy = orderBy;
                OrderByAsc = true;
            }
            else
            {
                OrderByAsc = !OrderByAsc;
            }
            refresh();
        }

        protected override void OnParametersSet()
        {
            var preSelectedPath = SelectedPath;
            if (!string.IsNullOrEmpty(BaseDir))
            {
                BaseDir = BaseDir.Replace('/', Path.DirectorySeparatorChar);
                while (BaseDir != Path.DirectorySeparatorChar.ToString() && BaseDir.EndsWith(Path.DirectorySeparatorChar))
                    BaseDir = BaseDir.Substring(0, BaseDir.Length - 1);
                if (string.IsNullOrEmpty(Dir))
                    gotoPath(BaseDir);
                else
                    if (!Path.IsPathFullyQualified(Dir))
                        Dir = getFullPathFromBaseDir(Dir);
            }
            if (string.IsNullOrEmpty(Dir))
            {
                refresh();
            }
            else
            {
                gotoPath(Dir);
                if (!string.IsNullOrEmpty(preSelectedPath))
                {
                    if (!string.IsNullOrEmpty(BaseDir))
                        preSelectedPath = getFullPathFromBaseDir(preSelectedPath);

                    if (Dirs != null)
                    {
                        foreach (var item in Dirs)
                        {
                            if (item.FullName == preSelectedPath)
                            {
                                SelectedItem = item;
                                break;
                            }
                        }
                    }
                    if (Files != null)
                    {
                        foreach (var item in Files)
                        {
                            if (item.FullName == preSelectedPath)
                            {
                                SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void gotoPath(string path)
        {
            gotoDir(new DirectoryInfo(path));
        }

        private void gotoDir(DirectoryInfo dir)
        {
            CurrentPath = dir?.FullName;
            if (!string.IsNullOrEmpty(BaseDir))
            {
                if (CurrentPath != null)
                {
                    CurrentPath = CurrentPath.Replace(Path.DirectorySeparatorChar, '/');
                    if (!isPathUnderBaseDir(CurrentPath))
                    {
                        gotoPath(BaseDir);
                        return;
                    }
                    CurrentPath = CurrentPath.Substring(BaseDir.Length).Replace(Path.DirectorySeparatorChar, '/');
                }
                if (string.IsNullOrEmpty(CurrentPath))
                    CurrentPath = "/";
                if (dir == null)
                    dir = new DirectoryInfo(BaseDir);
            }
            CurrentDir = dir;
            Dir = dir?.FullName;
            refresh();
        }

        private async void txtCurrentPath_KeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Task.Delay(100);
                btnGoto_Click();
                await InvokeAsync(StateHasChanged);
            }
        }

        private string getFullPathFromBaseDir(string path)
        {
            path = path.Replace('/', Path.DirectorySeparatorChar);
            if (!path.StartsWith(Path.DirectorySeparatorChar))
                path = Path.DirectorySeparatorChar + path;

            return BaseDir + path;
        }

        private bool isPathUnderBaseDir(string path)
        {
            path = path.Replace('/', Path.DirectorySeparatorChar);
            return path.StartsWith(BaseDir);
        }

        private void btnGoto_Click()
        {
            if (string.IsNullOrEmpty(CurrentPath))
            {
                gotoDir(null);
            }
            else
            {
                var fullCurrentPath = CurrentPath;
                if (!string.IsNullOrEmpty(BaseDir))
                    fullCurrentPath = getFullPathFromBaseDir(fullCurrentPath);
                gotoDir(new DirectoryInfo(fullCurrentPath));
            }
        }

        private void btnGotoUpper_Click()
        {
            if (CurrentDir == null)
                return;
            var dir = CurrentDir.Parent;
            gotoDir(dir);
        }

        private void btnCreateFolder_Click()
        {
            modalPrompt?.Show(TextNewFolderPrompt, TextNewFolder, dir_name =>
            {
                try
                {
                    var newDirPath = Path.Combine(Dir, dir_name);
                    if (!string.IsNullOrEmpty(BaseDir))
                        if (!isPathUnderBaseDir(newDirPath))
                            throw new IOException(TextCouldNotCreateFolderOutOfBaseDir);
                    Directory.CreateDirectory(newDirPath);
                    modalAlert?.Show(TextNewFolder, TextSuccess);
                    refresh();
                    InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    modalAlert?.Show(TextNewFolder, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
                }
            }, () => { });
        }

        private void btnRename_Click()
        {
            if (SelectedItem == null)
                return;
            if (SelectedItem is DirectoryInfo)
            {
                var dir = (DirectoryInfo)SelectedItem;
                modalPrompt?.Show(string.Format(TextInputNewName, dir.Name), dir.Name, name =>
                 {
                     try
                     {
                         var newPath = Path.Combine(Dir, name);
                         dir.MoveTo(newPath);
                         modalAlert?.Show(TextRename, TextSuccess);
                         refresh();
                         InvokeAsync(StateHasChanged);
                     }
                     catch (Exception ex)
                     {
                         modalAlert?.Show(TextRename, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
                     }
                 }, () => { });
            }
            else if (SelectedItem is FileInfo)
            {
                var file = (FileInfo)SelectedItem;
                modalPrompt?.Show(string.Format(TextInputNewName, file.Name), file.Name, name =>
                {
                    try
                    {
                        var newPath = Path.Combine(Dir, name);
                        file.MoveTo(newPath);
                        modalAlert?.Show(TextRename, TextSuccess);
                        refresh();
                        InvokeAsync(StateHasChanged);
                    }
                    catch (Exception ex)
                    {
                        modalAlert?.Show(TextRename, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
                    }
                }, () => { });
            }
        }

        private void onFileDoubleClick()
        {
            if (FileDoubleClickCustomAction == null)
            {
                if (FileDoubleClickToDownload)
                    btnDownload_Click();
            }
            else
            {
                FileDoubleClickCustomAction(JSRuntime);
            }
        }

        private async void btnDownload_Click()
        {
            var file = SelectedItem as FileInfo;
            if (file == null)
                return;

            if (DownloadFileAction != null)
            {
                DownloadFileAction.Invoke(JSRuntime, file.FullName);
                return;
            }

            var cts = new System.Threading.CancellationTokenSource();
            var cancellationToken = cts.Token;
            modalLoading?.Show(TextDownload, file.Name, false, cts.Cancel);
            try
            {
                using (var commonTransferContext = new CommonTransferContext(progressInfo =>
                {
                    modalLoading.UpdateProgress(progressInfo.Percent, progressInfo.Message);
                }, file.Length))
                using (var fs = file.OpenRead())
                {
                    await commonTransferContext.TransferAsync(fs, async m =>
                    {
                        await BlazorDownloadFileService.AddBuffer(m, cancellationToken);
                    }, cancellationToken);
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    modalAlert?.Show(TextDownload, TextCanceled);
                    return;
                }
                var result = await BlazorDownloadFileService.DownloadBinaryBuffers(file.Name, cancellationToken);
                if (!result.Succeeded)
                {
                    modalAlert?.Show(TextDownload, TextFailed + Environment.NewLine + result.ErrorName + Environment.NewLine + result.ErrorMessage);
                }
            }
            catch (TaskCanceledException)
            {
                modalAlert?.Show(TextDownload, TextCanceled);
            }
            catch (Exception ex)
            {
                modalAlert?.Show(TextDownload, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
            }
            finally
            {
                await BlazorDownloadFileService.ClearBuffers();
                modalLoading?.Close();
            }
        }

        private async void btnVerify_Click()
        {
            var fileInfo = (FileInfo)SelectedItem;
            var cts = new System.Threading.CancellationTokenSource();
            var cancellationToken = cts.Token;
            modalLoading?.Show(TextVerify, fileInfo.Name, false, cts.Cancel);
            //开始校验
            try
            {
                var md5 = System.Security.Cryptography.MD5.Create();
                var sha1 = System.Security.Cryptography.SHA1.Create();
                var sha256 = System.Security.Cryptography.SHA256.Create();

                using (var commonTransferContext = new CommonTransferContext(progressInfo =>
                {
                    modalLoading.UpdateProgress(progressInfo.Percent, progressInfo.Message);
                }, fileInfo.Length))
                using (var fs = fileInfo.OpenRead())
                {
                    await commonTransferContext.TransferAsync(fs, m =>
                    {
                        md5.TransformBlock(m.Array, m.Offset, m.Count, null, 0);
                        sha1.TransformBlock(m.Array, m.Offset, m.Count, null, 0);
                        sha256.TransformBlock(m.Array, m.Offset, m.Count, null, 0);
                        return Task.CompletedTask;
                    }, cancellationToken);
                }
                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                var sb = new StringBuilder();
                sb.AppendLine("Name: " + fileInfo.Name);
                sb.AppendLine("Size: " + fileInfo.Length);
                sb.AppendLine("MD5: " + Convert.ToHexString(md5.Hash).ToLower());
                sb.AppendLine("SHA1: " + Convert.ToHexString(sha1.Hash).ToLower());
                sb.AppendLine("SHA256: " + Convert.ToHexString(sha256.Hash).ToLower());
                modalAlert?.Show(TextVerify, sb.ToString(), new() { UsePreTag = true });
                await InvokeAsync(StateHasChanged);
            }
            catch (TaskCanceledException)
            {
                modalAlert?.Show(TextVerify, TextCanceled);
            }
            catch (Exception ex)
            {
                modalAlert?.Show(TextVerify, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
            }
            finally
            {
                modalLoading?.Close();
            }
        }

        private async void btnCompress_Click()
        {
            var fsInfo = (FileSystemInfo)SelectedItem;
            var cts = new System.Threading.CancellationTokenSource();
            var cancellationToken = cts.Token;
            modalLoading?.Show($"{TextCompress} - {fsInfo.Name}", null, false, cts.Cancel);
            var folderList = new List<DirectoryInfo>();
            var fileList = new List<FileInfo>();
            long totalFileSize = 0;
            //统计要压缩的文件列表信息
            string baseFolder = null;
            if (SelectedItem is FileInfo)
            {
                var fileInfo = (FileInfo)SelectedItem;
                baseFolder = fileInfo.DirectoryName;
                fileList.Add(fileInfo);
                totalFileSize = fileInfo.Length;
            }
            else if (SelectedItem is DirectoryInfo)
            {
                var dirInfo = (DirectoryInfo)SelectedItem;
                baseFolder = dirInfo.Parent.FullName;
                fileList.AddRange(dirInfo.GetFiles("*", SearchOption.AllDirectories));
                folderList.Add(dirInfo);
                folderList.AddRange(dirInfo.GetDirectories("*", SearchOption.AllDirectories));
                totalFileSize = fileList.Sum(t => t.Length);
            }
            //文件名
            string zipFileName = $"{fsInfo.Name}.zip";
            if (File.Exists(Path.Combine(baseFolder, zipFileName)))
            {
                for (var i = 1; i < int.MaxValue; i++)
                {
                    zipFileName = $"{fsInfo.Name}({i}).zip";
                    if (!File.Exists(Path.Combine(baseFolder, zipFileName)))
                        break;
                }
            }
            zipFileName = Path.Combine(baseFolder, zipFileName);
            //开始压缩
            try
            {
                using (var zipFileStream = File.Create(zipFileName))
                using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, true))
                {
                    //添加文件夹
                    foreach (var folder in folderList)
                    {
                        var entryName = folder.FullName.Substring(baseFolder.Length + 1) + Path.DirectorySeparatorChar;
                        entryName = PathUtils.UseUnixDirectorySeparatorChar(entryName);
                        zipArchive.CreateEntry(entryName);
                    }
                    //添加文件
                    using (var commonTransferContext = new CommonTransferContext(progressInfo =>
                    {
                        modalLoading.UpdateProgress(progressInfo.Percent, progressInfo.Message);
                    }, totalFileSize))
                    {
                        foreach (var file in fileList)
                        {
                            var entryName = file.FullName.Substring(baseFolder.Length + 1);
                            entryName = PathUtils.UseUnixDirectorySeparatorChar(entryName);
                            modalLoading.UpdateContent(entryName);
                            var zipEntry = zipArchive.CreateEntry(entryName);
                            using (var fs = file.OpenRead())
                            using (var zs = zipEntry.Open())
                                await commonTransferContext.TransferAsync(fs, zs, cancellationToken);
                        }
                    }
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    try { File.Delete(zipFileName); } catch { }
                    modalAlert?.Show(TextCompress, TextCanceled);
                    return;
                }
                var fileInfo = new FileInfo(zipFileName);
                Files = new[] { fileInfo }.Concat(Files).ToArray();
                SelectedItem = fileInfo;
                await InvokeAsync(StateHasChanged);
            }
            catch (TaskCanceledException)
            {
                try { File.Delete(zipFileName); } catch { }
                modalAlert?.Show(TextCompress, TextCanceled);
            }
            catch (Exception ex)
            {
                try { File.Delete(zipFileName); } catch { }
                modalAlert?.Show(TextCompress, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
            }
            finally
            {
                modalLoading?.Close();
            }
        }

        private async void btnDecompress_Click()
        {
            var archiveFileInfo = SelectedItem as FileInfo;
            if (archiveFileInfo == null)
                return;
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            modalLoading?.Show($"{TextDecompress} - {archiveFileInfo.Name}", TextLoading, false, cts.Cancel);
            long totalFileSize = 0;
            var baseFolder = archiveFileInfo.DirectoryName;
            //开始解压
            try
            {
                await Task.Run(()=>
                {
                    using (var archiveFileStream = archiveFileInfo.OpenRead())
                    using (var archive = ArchiveFactory.OpenArchive(archiveFileStream))
                    {
                        totalFileSize = archive.GetEntriesTotalSize();
                        using (var commonTransferContext = new CommonTransferContext(progressInfo =>
                        {
                            modalLoading.UpdateProgress(progressInfo.Percent, progressInfo.Message);
                        }, totalFileSize))
                        {
                            archive.EntriesForEach(entry =>
                            {
                                var metaEntry = entry.Entry;
                                var entryKey = metaEntry.Key;
                                if (string.IsNullOrEmpty(entryKey))
                                    entryKey = Path.GetFileNameWithoutExtension(archiveFileInfo.Name);
                                modalLoading.UpdateContent(entryKey);
                                var fileName = Path.Combine(baseFolder, entryKey);
                                //如果是文件夹
                                if (metaEntry.IsDirectory)
                                {
                                    if (!Directory.Exists(fileName))
                                        Directory.CreateDirectory(fileName);
                                    return;
                                }
                                var fileFolder = Path.GetDirectoryName(fileName);
                                if (!Directory.Exists(fileFolder))
                                    Directory.CreateDirectory(fileFolder);
                                using (var zipEntryStream = entry.OpenEntryStream())
                                using (var fileStream = File.OpenWrite(fileName))
                                    commonTransferContext.TransferAsync(zipEntryStream, fileStream).Wait();
                            });
                        }
                    }
                });
                if (cancellationToken.IsCancellationRequested)
                {
                    modalAlert?.Show(TextDecompress, TextCanceled);
                    return;
                }
                refresh();
                await InvokeAsync(StateHasChanged);
            }
            catch (TaskCanceledException)
            {
                modalAlert?.Show(TextDecompress, TextCanceled);
            }
            catch (Exception ex)
            {
                modalAlert?.Show(TextDecompress, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
            }
            finally
            {
                modalLoading?.Close();
            }
        }

        private CancellationTokenSource uploadCts;

        private async Task onInputFileChanged(ElementReference inputFile)
        {
            var fileReaderRef = fileReaderService.CreateReference(inputFile);
            FileUploadHelper.UploadFileInfo currentFileInfo = default;
            try
            {
                uploadCts = new CancellationTokenSource();
                var cancellationToken = uploadCts.Token;

                modalLoading?.Show(TextUpload, TextUploadReadFileInfo, false, uploadCts.Cancel);
                var fileReferences = (await fileReaderRef.EnumerateFilesAsync()).ToArray();
                var currentFileIndex = 0;
                await FileUploadHelper.UploadFilesAsync(
                    fileReferences,
                    async fileInfo =>
                    {
                        currentFileInfo = fileInfo;
                        currentFileIndex++;
                        var message = $"{fileInfo.Name} ({fileInfo.SizeString})";
                        if (fileReferences.Length > 1)
                            message = $"{currentFileIndex}/{fileReferences.Length} {message}";
                        modalLoading?.Show(TextUpload, string.Format(TextUploadFileUploading, message), false, uploadCts.Cancel);
                        var file = Path.Combine(CurrentDir.FullName, fileInfo.Name);
                        if (File.Exists(file))
                        {
                            bool? isConfirm = null;
                            var confirmCts = new CancellationTokenSource();
                            modalAlert.Show(TextConfirm, string.Format(TextUploadFileExistReplace, file), new()
                            {
                                OkCallback = () =>
                                {
                                    isConfirm = true;
                                    confirmCts.Cancel();
                                },
                                CancelCallback = () =>
                                {
                                    isConfirm = false;
                                    confirmCts.Cancel();
                                },
                                CloseCallback = () =>
                                {
                                    isConfirm = false;
                                    confirmCts.Cancel();
                                }
                            });
                            try { await Task.Delay(-1, confirmCts.Token); }
                            catch { }
                            if (isConfirm.Value)
                            {
                                File.Delete(file);
                            }
                            else
                            {
                                uploadCts.Cancel();
                                throw new OperationCanceledException();
                            }
                            return file;
                        }
                        return file;
                    },
                    progressInfo => modalLoading.UpdateProgress(progressInfo.Percent, progressInfo.Message),
                    cancellationToken);
                modalAlert?.Show(TextUpload, TextSuccess);
            }
            catch (OperationCanceledException)
            {
                modalAlert?.Show(TextUpload, TextCanceled);
            }
            catch (Exception ex)
            {
                modalAlert?.Show(TextUpload, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
            }
            finally
            {
                await fileReaderRef.ClearValue();
                modalLoading?.Close();
                refresh();
                SelectedItem = Files?.FirstOrDefault(t => t.Name == currentFileInfo.Name);
            }
        }

        private void btnEdit_Click()
        {
            var file = SelectedItem as FileInfo;
            if (file == null)
                return;

            Action openTextEditWindowAction = () =>
            {
                modalWindow.Show($"{file.Name} - {TextEdit}", new DialogParameters<TextEditControl>()
                {
                    { x => x.File, file.FullName },
                    { x => x.IconSave, IconSave },
                    { x => x.EncodingDict, EncodingDict }
                });
            };
            //如果文件大小大于1MB，则弹出提示是否打开
            if (file.Length > 1 * 1024 * 1024)
            {
                var fileInfoStr = $"{file.Name} ({storageUSC.GetString(file.Length, 2, true)}B)";
                modalAlert.Show(TextConfirm, Locale.GetString("File [{0}] is too large,are you sure to edit it with text editor?", fileInfoStr), new() { OkCallback = openTextEditWindowAction });
            }
            else
            {
                openTextEditWindowAction();
            }
        }

        private void btnDelete_Click()
        {
            if (SelectedItem == null)
                return;
            if (SelectedItem is FileInfo)
            {
                var file = (FileInfo)SelectedItem;
                modalAlert?.Show(TextConfirm, string.Format(TextConfirmDeleteFile, file.Name), new()
                {
                    OkCallback = () =>
                    {
                        modalLoading?.Show(TextConfirm, file.Name, true, null);
                        try
                        {
                            file.Delete();
                            modalAlert?.Show(TextDelete, TextSuccess);
                        }
                        catch (Exception ex)
                        {
                            modalAlert?.Show(TextDelete, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
                        }
                        refresh();
                        modalLoading?.Close();
                        InvokeAsync(StateHasChanged);
                    },
                    ShowCancelButton = true
                });
            }
            else if (SelectedItem is DirectoryInfo)
            {
                var dir = (DirectoryInfo)SelectedItem;
                modalAlert?.Show(TextConfirm, string.Format(TextConfirmDeleteFolder, dir.Name), new()
                {
                    OkCallback = () =>
                    {
                        modalLoading.Show(TextDelete, dir.Name, true, null);
                        try
                        {
                            dir.Delete(true);
                            modalAlert?.Show(TextDelete, TextSuccess);
                        }
                        catch (Exception ex)
                        {
                            modalAlert?.Show(TextDelete, TextFailed + Environment.NewLine + ExceptionUtils.GetExceptionMessage(ex));
                        }
                        refresh();
                        modalLoading.Close();
                        InvokeAsync(StateHasChanged);
                    },
                    ShowCancelButton = true
                });
            }
        }

        private IEnumerable<DirectoryInfo> orderDirs(IEnumerable<DirectoryInfo> query)
        {
            switch (OrderBy)
            {
                case nameof(DirectoryInfo.LastWriteTime):
                    return OrderByAsc ? query.OrderBy(t => t.LastWriteTime) : query.OrderByDescending(t => t.LastWriteTime);
                case nameof(DirectoryInfo.Name):
                default:
                    return OrderByAsc ? query.OrderBy(t => t.Name) : query.OrderByDescending(t => t.Name);
            }
        }

        private IEnumerable<FileInfo> orderFiles(IEnumerable<FileInfo> query)
        {
            switch (OrderBy)
            {
                case nameof(FileInfo.LastWriteTime):
                    return OrderByAsc ? query.OrderBy(t => t.LastWriteTime) : query.OrderByDescending(t => t.LastWriteTime);
                case nameof(FileInfo.Length):
                    return OrderByAsc ? query.OrderBy(t => t.Length) : query.OrderByDescending(t => t.Length);
                case nameof(FileInfo.Name):
                default:
                    return OrderByAsc ? query.OrderBy(t => t.Name) : query.OrderByDescending(t => t.Name);
            }
        }

        private void refresh()
        {
            SelectedItem = null;
            Dirs = null;
            Files = null;

            if (CurrentDir == null)
            {
                if (DisplayFolder)
                {
                    var query = DriveInfo.GetDrives().Where(t => t.IsReady)
                        .Select(t => t.RootDirectory)
                        .Where(t => string.IsNullOrEmpty(Search) || t.Name.Contains(Search));
                    query = orderDirs(query);
                    Dirs = query.ToArray();
                }
            }
            else
            {
                if (CurrentDir.Exists)
                {
                    try
                    {
                        if (DisplayFolder)
                        {
                            var query = CurrentDir.GetDirectories()
                                .Where(t => string.IsNullOrEmpty(Search) || t.Name.Contains(Search));
                            query = orderDirs(query);
                            Dirs = query.ToArray();
                        }
                        if (DisplayFile)
                        {
                            IEnumerable<FileInfo> query = null;
                            if (string.IsNullOrEmpty(FileFilter))
                            {
                                query = CurrentDir.GetFiles();
                            }
                            else
                            {
                                var fileFilters = FileFilter.Split(new char[] { '|', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                List<FileInfo> fileList = new List<FileInfo>();
                                foreach (var fileFilter in fileFilters)
                                {
                                    fileList.AddRange(CurrentDir.GetFiles(fileFilter));
                                }
                                query = fileList;
                            }
                            query = query.Where(t => string.IsNullOrEmpty(Search) || t.Name.Contains(Search));
                            query = orderFiles(query);
                            Files = query.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        modalAlert?.Show(TextFailed, ExceptionUtils.GetExceptionMessage(ex));
                    }
                }
                else
                {
                    modalAlert?.Show(TextFailed, string.Format(TextFolderNotExist, CurrentPath));
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            uploadCts?.Cancel();
            uploadCts = null;
        }
    }
}
