using Tewr.Blazor.FileReader;
using Quick.Blazor.Bootstrap.Admin.Core;
using static Quick.Blazor.Bootstrap.Admin.Core.CommonTransferContext;

namespace Quick.Blazor.Bootstrap.Admin.Utils;

public static class FileUploadHelper
{
    private readonly static UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;

    public struct UploadFileInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string SizeString { get; set; }

        public UploadFileInfo(string name, long size, string sizeString)
        {
            Name = name;
            Size = size;
            SizeString = sizeString;
        }
    }

    public static async Task<string> UploadFileAsync(
        IFileReference fileReference,
        Func<UploadFileInfo, string> fileInfoHandler,
        Action<TransferProgressInfo> progressUpdated,
        CancellationToken cancellationToken)
    {
        string tmpFile = null;
        try
        {
            var fileInfo = await fileReference.ReadFileInfoAsync();
            using (var commonTransferContext = new CommonTransferContext(progressUpdated, fileInfo.Size))
            {
                tmpFile = fileInfoHandler?.Invoke(new UploadFileInfo(fileInfo.Name, fileInfo.Size, storageUSC.GetString(fileInfo.Size, 0, true) + "B"));
                if (string.IsNullOrEmpty(tmpFile))
                    tmpFile = Path.GetTempFileName();
                using (Stream stream = await fileReference.OpenReadAsync())
                using (var fileStream = File.OpenWrite(tmpFile))
                    await commonTransferContext.TransferAsync(stream, fileStream, cancellationToken, fileInfo.Size);

                //返回上传的文件
                return tmpFile;
            }
        }
        catch
        {
            if (tmpFile != null && File.Exists(tmpFile))
                try { File.Delete(tmpFile); } catch { }
            throw;
        }
    }

    public static async Task<string[]> UploadFilesAsync(
        IFileReference[] fileReferences,
        Func<UploadFileInfo, string> fileInfoHandler,
        Action<TransferProgressInfo> progressUpdated,
        CancellationToken cancellationToken)
    {
        string tmpFile = null;
        try
        {
            var fileInfos = new IFileInfo[fileReferences.Length];
            for (var i = 0; i < fileReferences.Length; i++)
            {
                var fileReference = fileReferences[i];
                var fileInfo = await fileReference.ReadFileInfoAsync();
                fileInfos[i] = fileInfo;
            }
            var totalFileSize = fileInfos.Sum(t => t.Size);
            using (var commonTransferContext = new CommonTransferContext(progressUpdated, totalFileSize))
            {
                var tmpFiles = new string[fileReferences.Length];
                for (var i = 0; i < fileReferences.Length; i++)
                {
                    var fileReference = fileReferences[i];
                    var fileInfo = fileInfos[i];
                    tmpFile = fileInfoHandler?.Invoke(new UploadFileInfo(fileInfo.Name, fileInfo.Size, storageUSC.GetString(fileInfo.Size, 0, true) + "B"));
                    if (string.IsNullOrEmpty(tmpFile))
                        tmpFile = Path.GetTempFileName();

                    using (Stream stream = await fileReference.OpenReadAsync())
                    using (var fileStream = File.OpenWrite(tmpFile))
                        await commonTransferContext.TransferAsync(stream, fileStream, cancellationToken, fileInfo.Size);

                    tmpFiles[i] = tmpFile;
                }
                //返回上传的文件
                return tmpFiles;
            }
        }
        catch
        {
            if (tmpFile != null && File.Exists(tmpFile))
                try { File.Delete(tmpFile); } catch { }
            throw;
        }
    }
}
