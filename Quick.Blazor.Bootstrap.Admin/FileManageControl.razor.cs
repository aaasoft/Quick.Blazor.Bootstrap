using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Blazor.Bootstrap.Utils;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class FileManageControl : ComponentBase_WithGettextSupport
    {
        private readonly UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        private IBlazorDownloadFileService BlazorDownloadFileService { get; set; }

        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ModalPrompt modalPrompt;
        private ModalWindow modalWindow;

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
                SelectedPathChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Parameter]
        public string SelectedPath
        {
            get
            {
                if (SelectedItem == null)
                    return null;
                if (SelectedItem is FileInfo)
                    return ((FileInfo)SelectedItem).FullName;
                if (SelectedItem is DirectoryInfo)
                    return ((DirectoryInfo)SelectedItem).FullName;
                return null;
            }
            set
            {
                if (File.Exists(value))
                {
                    SelectedItem = Files.FirstOrDefault(t => t.FullName == value);
                    if (SelectedItem == null)
                    {
                        var fileInfo = new FileInfo(value);
                        Files = new[] { fileInfo }.Concat(Files).ToArray();
                        SelectedItem = fileInfo;
                    }
                }
                if (Directory.Exists(value))
                {
                    SelectedItem = Dirs.FirstOrDefault(t => t.FullName == value);
                    if (SelectedItem == null)
                    {
                        var dirInfo = new DirectoryInfo(value);
                        Dirs = new[] { dirInfo }.Concat(Dirs).ToArray();
                        SelectedItem = dirInfo;
                    }
                }
            }
        }

        [Parameter]
        public string Dir { get; set; }
        [Parameter]
        public bool ListViewMode { get; set; } = true;
        [Parameter]
        public Action<IJSRuntime, string> DownloadFileAction { get; set; }
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
        [Parameter]
        public string OrderBy { get; set; } = nameof(FileInfo.Name);
        [Parameter]
        public bool OrderByAsc { get; set; } = true;

        private static string TextConfirm => Locale.GetString("Confirm");
        private static string TextConfirmDeleteFolder => Locale.GetString("Do you want to delete folder[{0}]?");
        private static string TextConfirmDeleteFile => Locale.GetString("Do you want to delete file[{0}]?");
        private static string TextInputNewName => Locale.GetString("Please input new name of [{0}]");
        private static string TextSuccess => Locale.GetString("Success");
        private static string TextCanceled => Locale.GetString("Canceled");
        private static string TextFailed => Locale.GetString("Failed");
        private static string TextFolderNotExist => Locale.GetString("Folder [{0}] not exist");
        private static string TextUp => Locale.GetString("Up");
        private static string TextNewFolder => Locale.GetString("New Folder");
        private static string TextNewFolderPrompt => Locale.GetString("Please input new folder name");
        private static string TextUpload => Locale.GetString("Upload");
        private static string TextUploadReadFileInfo => Locale.GetString("Reading upload file info...");
        private static string TextUploadFileExist => Locale.GetString("File [{0}] was exist.");
        private static string TextUploadFileUploading => Locale.GetString("Uploading file [{0}]...");
        private static string TextRefresh => Locale.GetString("Refresh");
        private static string TextDownload => Locale.GetString("Download");
        private static string TextVerify => Locale.GetString("Verify");
        private static string TextCompress => Locale.GetString("Compress");
        private static string TextDecompress => Locale.GetString("Decompress");

        private static string TextRename => Locale.GetString("Rename");
        private static string TextEdit => Locale.GetString("Edit");
        [Parameter]
        public Dictionary<string, Encoding> EncodingDict { get; set; }

        private static string TextDelete => Locale.GetString("Delete");
        private static string TextPath => Locale.GetString("Path");
        private static string TextGoto => Locale.GetString("Goto");
        private static string TextLastWriteTime => Locale.GetString("Last Write Time");
        private static string TextSize => Locale.GetString("Size");
        private static string TextName => Locale.GetString("Name");

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
                
        private bool isSelectedFile()
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
                    return storageUSC.GetString(fileInfo.Length, 0, true);
            }
            catch { }
            return string.Empty;
        }

        private string getLastWriteString(FileSystemInfo fileInfo)
        {
            try
            {
                if (fileInfo.Exists)
                    return fileInfo.LastWriteTime.ToString();
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
            if (string.IsNullOrEmpty(Dir))
            {
                refresh();
            }
            else
            {
                var preSelectedPath = SelectedPath;
                gotoPath(Dir);
                if (!string.IsNullOrEmpty(preSelectedPath))
                {
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
            CurrentDir = dir;
            Dir = CurrentPath = dir?.FullName;
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

        private void btnGoto_Click()
        {
            if (string.IsNullOrEmpty(CurrentPath))
                gotoDir(null);
            else
                gotoDir(new DirectoryInfo(CurrentPath));
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
                    var newDirPath = Path.Combine(CurrentPath, dir_name);
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
                         var newPath = Path.Combine(CurrentPath, name);
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
                        var newPath = Path.Combine(CurrentPath, name);
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
            var cancellationToken= cts.Token;
            modalLoading?.Show(TextDownload, file.Name, false, cts.Cancel);
            try
            {
                using (var commonTransferContext = new CommonTransferContext(progressInfo =>
                {
                    modalLoading.UpdateProgress(progressInfo.Percent, progressInfo.Message);
                }, file.Length))
                using (var fs = file.OpenRead())
                {
                    await commonTransferContext.TransferAsync(fs,async m=>
                    {
                        await BlazorDownloadFileService.AddBuffer(m, cancellationToken);
                    },cancellationToken);
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
                modalAlert?.Show(TextVerify, sb.ToString(), usePreTag: true);
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
            if(File.Exists(Path.Combine(baseFolder,zipFileName)))
            {
                for(var i=1;i<int.MaxValue;i++)
                {
                    zipFileName = $"{fsInfo.Name}({i}).zip";
                    if(!File.Exists(Path.Combine(baseFolder,zipFileName)))
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
                Files = new[] { new FileInfo(zipFileName) }.Concat(Files).ToArray();
                SelectedPath = zipFileName;
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
            var zipFileInfo = SelectedItem as FileInfo;
            if (zipFileInfo == null)
                return;
            var cts = new System.Threading.CancellationTokenSource();
            var cancellationToken = cts.Token;
            modalLoading?.Show($"{TextDecompress} - {zipFileInfo.Name}", null, false, cts.Cancel);
            long totalFileSize = 0;
            var baseFolder = zipFileInfo.DirectoryName;
            //开始解压
            try
            {
                using (var zipFileStream = zipFileInfo.OpenRead())
                using (var zipArchive = SharpCompress.Archives.ArchiveFactory.Open(zipFileStream))
                {
                    foreach (var zipEntry in zipArchive.Entries)
                    {
                        totalFileSize += zipEntry.Size;
                    }
                    using (var commonTransferContext = new CommonTransferContext(progressInfo =>
                    {
                        modalLoading.UpdateProgress(progressInfo.Percent, progressInfo.Message);
                    }, totalFileSize))
                    {
                        foreach (var zipEntry in zipArchive.Entries)
                        {
                            var zipEntryKey = zipEntry.Key;
                            if (zipEntry is SharpCompress.Archives.GZip.GZipArchiveEntry)
                                zipEntryKey = Path.GetFileNameWithoutExtension(zipFileInfo.Name);
                            modalLoading.UpdateContent(zipEntryKey);
                            var fileName = Path.Combine(baseFolder, zipEntryKey);
                            //如果是文件夹
                            if (zipEntry.IsDirectory)
                            {
                                if (!Directory.Exists(fileName))
                                    Directory.CreateDirectory(fileName);
                                continue;
                            }
                            var fileFolder = Path.GetDirectoryName(fileName);
                            if (!Directory.Exists(fileFolder))
                                Directory.CreateDirectory(fileFolder);
                            using (var zipEntryStream = zipEntry.OpenEntryStream())
                            using (var fileStream = File.OpenWrite(fileName))
                                await commonTransferContext.TransferAsync(zipEntryStream, fileStream);
                        }
                    }
                }
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

        private System.Threading.CancellationTokenSource uploadCts;

        private async Task onInputFileChanged(InputFileChangeEventArgs e)
        {
            IBrowserFile firstFile = null;
            try
            {
                //1MB缓存
                uploadCts = new System.Threading.CancellationTokenSource();
                var cancellationToken = uploadCts.Token;
                var files = e.GetMultipleFiles(int.MaxValue);
                var totalFileSize = files.Sum(t => t.Size);                

                using (var commonTransferContext = new CommonTransferContext(progressInfo =>
                {
                    modalLoading.UpdateProgress(progressInfo.Percent, progressInfo.Message);
                }, totalFileSize))
                {
                    foreach (var file in files)
                    {
                        if (firstFile == null)
                            firstFile = file;

                        modalLoading?.Show(TextUpload, TextUploadReadFileInfo, false, uploadCts.Cancel);
                        var tmpFile = Path.Combine(CurrentPath, file.Name);
                        if (File.Exists(tmpFile))
                            throw new IOException(string.Format(TextUploadFileExist, file.Name));
                        var fileSize = file.Size;
                        var fileInfoStr = $"{file.Name} ({storageUSC.GetString(fileSize, 0, true)}B)";
                        modalLoading?.Show(TextUpload, string.Format(TextUploadFileUploading, fileInfoStr), false, uploadCts.Cancel);
                        try
                        {
                            using (Stream stream = file.OpenReadStream(fileSize, cancellationToken))
                            using (var fileStream = File.OpenWrite(tmpFile))
                                await commonTransferContext.TransferAsync(stream, fileStream, cancellationToken,fileSize);
                            
                            if (cancellationToken.IsCancellationRequested)
                                throw new OperationCanceledException();
                        }
                        catch (OperationCanceledException)
                        {
                            modalAlert?.Show(TextUpload, TextCanceled);
                            File.Delete(tmpFile);
                            throw;
                        }
                        modalAlert?.Show(TextUpload, TextSuccess);
                    }
                }
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
                modalLoading?.Close();
                refresh();
                SelectedItem = Files?.FirstOrDefault(t => t.Name == firstFile?.Name);
            }
        }

        private void btnEdit_Click()
        {
            var file = SelectedItem as FileInfo;
            if (file == null)
                return;

            Action openTextEditWindowAction = () =>
            {
                modalWindow.Show<TextEditControl>($"{file.Name} - {TextEdit}", new Dictionary<string, object>()
                {
                    [nameof(TextEditControl.File)] = file.FullName,
                    [nameof(TextEditControl.IconSave)] = IconSave,
                    [nameof(TextEditControl.EncodingDict)] = EncodingDict
                });
            };
            //如果文件大小大于1MB，则弹出提示是否打开
            if (file.Length > 1 * 1024 * 1024)
            {
                modalAlert.Show(TextConfirm, Locale.GetString($"File [{0}] is to large,are you sure to open it?", file.Name), openTextEditWindowAction);
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
                modalAlert?.Show(TextConfirm, string.Format(TextConfirmDeleteFile, file.Name), () =>
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
                }, () => { });
            }
            else if (SelectedItem is DirectoryInfo)
            {
                var dir = (DirectoryInfo)SelectedItem;
                modalAlert?.Show(TextConfirm, string.Format(TextConfirmDeleteFolder, dir.Name), () =>
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
                 }, () => { });
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
                                query = CurrentDir.GetFiles()
                                    .Where(t => string.IsNullOrEmpty(Search) || t.Name.Contains(Search));
                            else
                                query = CurrentDir.GetFiles("*" + FileFilter)
                                    .Where(t => string.IsNullOrEmpty(Search) || t.Name.Contains(Search));
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
                    modalAlert?.Show(TextFailed, string.Format(TextFolderNotExist, CurrentDir.FullName));
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
