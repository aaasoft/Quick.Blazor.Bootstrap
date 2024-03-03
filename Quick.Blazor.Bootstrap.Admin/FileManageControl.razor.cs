using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Quick.Blazor.Bootstrap.Admin.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Admin
{
    public partial class FileManageControl : IDisposable
    {
        private readonly UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;
        [Inject]
        private IBlazorDownloadFileService BlazorDownloadFileService { get; set; }

        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ModalPrompt modalPrompt;
        private ModalWindow modalWindow;

        private string CurrentPath;
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
                    SelectedItem = new FileInfo(value);
                if (Directory.Exists(value))
                    SelectedItem = new DirectoryInfo(value);
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
        public string FileFilter { get; set; }

        [Parameter]
        public string TextConfirm { get; set; } = "Confirm";
        [Parameter]
        public string TextConfirmDeleteFolder { get; set; } = "Do you want to delete folder[{0}]?";
        [Parameter]
        public string TextConfirmDeleteFile { get; set; } = "Do you want to delete file[{0}]?";
        [Parameter]
        public string TextInputNewName { get; set; } = "Please input new name of [{0}]?";
        [Parameter]
        public string TextSuccess { get; set; } = "Success";
        [Parameter]
        public string TextCanceled { get; set; } = "Canceled";
        [Parameter]
        public string TextFailed { get; set; } = "Failed";
        [Parameter]
        public string TextFolderNotExist { get; set; } = "Folder [{0}] not exist";
        [Parameter]
        public string TextUp { get; set; } = "Up";
        [Parameter]
        public string TextNewFolder { get; set; } = "New Folder";
        [Parameter]
        public string TextTransferSpeed { get; set; } = "Transfer Speed";
        [Parameter]
        public string TextRemainingTime { get; set; } = "Remaining Time";
        [Parameter]
        public string TextNewFolderPrompt { get; set; } = "Please input new folder name";
        [Parameter]
        public string TextUpload { get; set; } = "Upload";
        [Parameter]
        public string TextUploadReadFileInfo { get; set; } = "Reading upload file info...";
        [Parameter]
        public string TextUploadFileExist { get; set; } = "File [{0}] was exist.";
        [Parameter]
        public string TextUploadFileUploading { get; set; } = "Uploading file [{0}]...";

        [Parameter]
        public string TextRefresh { get; set; } = "Refresh";
        [Parameter]
        public string TextDownload { get; set; } = "Download";
        [Parameter]
        public string TextRename { get; set; } = "Rename";
        [Parameter]
        public string TextEdit { get; set; } = "Edit";
        [Parameter]
        public string TextRows { get; set; } = "Rows";
        [Parameter]
        public string TextEncoding { get; set; } = "Encoding";
        [Parameter]
        public string TextDelete { get; set; } = "Delete";
        [Parameter]
        public string TextPath { get; set; } = "Path";
        [Parameter]
        public string TextGoto { get; set; } = "Goto";
        [Parameter]
        public string TextCreationTime { get; set; } = "Creation Time";
        [Parameter]
        public string TextLastWriteTime { get; set; } = "Last Write Time";
        [Parameter]
        public string TextSize { get; set; } = "Size";
        [Parameter]
        public string TextName { get; set; } = "Name";

        [Parameter]
        public RenderFragment ToolbarAddonButtons { get; set; }

        [Parameter]
        public RenderFragment IconFolder { get; set; }
        [Parameter]
        public RenderFragment IconFile { get; set; }
        [Parameter]
        public RenderFragment IconUp { get; set; }
        [Parameter]
        public RenderFragment IconNewFolder { get; set; }
        [Parameter]
        public RenderFragment IconUpload { get; set; }
        [Parameter]
        public RenderFragment IconRefresh { get; set; }
        [Parameter]
        public RenderFragment IconDownload { get; set; }
        [Parameter]
        public RenderFragment IconRename { get; set; }
        [Parameter]
        public RenderFragment IconEdit { get; set; }
        [Parameter]
        public RenderFragment IconSave { get; set; }
        [Parameter]
        public RenderFragment IconDelete { get; set; }
        [Parameter]
        public RenderFragment IconDisplayList { get; set; }
        [Parameter]
        public RenderFragment IconDisplayIcon { get; set; }
        [Parameter]
        public RenderFragment IconGoto { get; set; }

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

            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            modalLoading?.Show(TextDownload, file.Name, false, cts.Cancel);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            DateTime lastDisplayTime = DateTime.MinValue;
            byte[] buffer = new byte[24 * 1024];
            var fileSize = file.Length;
            long readTotalCount = 0;

            try
            {
                using (var fs = file.OpenRead())
                {
                    while (!cts.IsCancellationRequested)
                    {
                        var ret = fs.Read(buffer, 0, buffer.Length);
                        if (ret <= 0)
                            break;
                        readTotalCount += ret;

                        if ((DateTime.Now - lastDisplayTime).TotalSeconds > 0.5 && stopwatch.ElapsedMilliseconds > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            var speed = Convert.ToDouble(readTotalCount / stopwatch.ElapsedMilliseconds);
                            sb.Append(TextTransferSpeed + ": " + storageUSC.GetString(Convert.ToDecimal(speed * 1000), 1, true) + "B/s");
                            var remainingTime = TimeSpan.FromMilliseconds((fileSize - readTotalCount) / speed);
                            sb.Append("," + TextRemainingTime + ": " + remainingTime.ToString(@"hh\:mm\:ss"));
                            modalLoading.UpdateProgress(Convert.ToInt32(readTotalCount * 100 / fileSize), sb.ToString());
                            await InvokeAsync(StateHasChanged);
                            lastDisplayTime = DateTime.Now;
                        }
                        await BlazorDownloadFileService.AddBuffer(new ArraySegment<byte>(buffer, 0, ret), cts.Token);
                    }
                }
                if (cts.IsCancellationRequested)
                {
                    modalAlert?.Show(TextDownload, TextCanceled);
                    return;
                }
                var result = await BlazorDownloadFileService.DownloadBinaryBuffers(file.Name, cts.Token);

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
                stopwatch.Stop();
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
                var buffer = new byte[1 * 1024 * 1024];
                uploadCts = new System.Threading.CancellationTokenSource();
                var cancellationToken = uploadCts.Token;

                foreach (var file in e.GetMultipleFiles())
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
                    var stopwatch = new System.Diagnostics.Stopwatch();

                    stopwatch.Start();
                    DateTime lastDisplayTime = DateTime.MinValue;

                    long readTotalCount = 0;
                    try
                    {
                        using (Stream stream = file.OpenReadStream(file.Size, cancellationToken))
                        using (var fileStream = File.OpenWrite(tmpFile))
                        {
                            while (true)
                            {
                                //如果已取消
                                if (cancellationToken.IsCancellationRequested)
                                    break;
                                var ret = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                                if (ret == 0)
                                {
                                    await Task.Delay(100);
                                    continue;
                                }
                                else
                                {
                                    readTotalCount += ret;
                                    fileStream.Write(buffer, 0, ret);
                                    if (readTotalCount >= fileSize)
                                        break;

                                    if ((DateTime.Now - lastDisplayTime).TotalSeconds > 0.5 && stopwatch.ElapsedMilliseconds > 0)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        var speed = Convert.ToDouble(readTotalCount / stopwatch.ElapsedMilliseconds);
                                        sb.Append(TextTransferSpeed + ": " + storageUSC.GetString(Convert.ToDecimal(speed * 1000), 1, true) + "B/s");
                                        var remainingTime = TimeSpan.FromMilliseconds((fileSize - readTotalCount) / speed);
                                        sb.Append("," + TextRemainingTime + ": " + remainingTime.ToString(@"hh\:mm\:ss"));
                                        modalLoading.UpdateProgress(Convert.ToInt32(readTotalCount * 100 / fileSize), sb.ToString());
                                        await InvokeAsync(StateHasChanged);
                                        lastDisplayTime = DateTime.Now;
                                    }
                                }
                            }
                        }
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException();
                    }
                    catch (OperationCanceledException)
                    {
                        modalAlert?.Show(TextUpload, TextCanceled);
                        File.Delete(tmpFile);
                        throw;
                    }
                    stopwatch.Stop();
                    modalAlert?.Show(TextUpload, TextSuccess);
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
                    [nameof(TextEditControl.TextSuccess)] = TextSuccess,
                    [nameof(TextEditControl.TextFailed)] = TextFailed,
                    [nameof(TextEditControl.TextEncoding)] = TextEncoding,
                    [nameof(TextEditControl.TextRows)] = TextRows
                });
            };
            //如果文件大小大于1MB，则弹出提示是否打开
            if (file.Length > 1 * 1024 * 1024)
            {
                modalAlert.Show(TextConfirm, $"File {0} is to large,are you sure to open it?", openTextEditWindowAction);
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

        private void refresh()
        {
            SelectedItem = null;
            Dirs = null;
            Files = null;

            if (CurrentDir == null)
            {
                if (DisplayFolder)
                    Dirs = DriveInfo.GetDrives().Where(t => t.IsReady).Select(t => t.RootDirectory).ToArray();
            }
            else
            {
                if (CurrentDir.Exists)
                {
                    try
                    {
                        if (DisplayFolder)
                            Dirs = CurrentDir.GetDirectories();
                        if (DisplayFile)
                            if (string.IsNullOrEmpty(FileFilter))
                                Files = CurrentDir.GetFiles();
                            else
                                Files = CurrentDir.GetFiles("*" + FileFilter);
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

        public void Dispose()
        {
            uploadCts?.Cancel();
            uploadCts = null;
        }
    }
}
