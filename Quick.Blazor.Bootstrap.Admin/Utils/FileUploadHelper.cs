using Microsoft.AspNetCore.Components.Forms;
using Quick.Blazor.Bootstrap.Admin.Core;

namespace Quick.Blazor.Bootstrap.Admin.Utils;

public static class FileUploadHelper
{
    private readonly static UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;
    
    public static async Task<string> UploadAsync(
        InputFileChangeEventArgs e,
        Action<string> fileInfoHandler,
        Action<int?, string> progressUpdated,
        CancellationTokenSource cts)
    {
        var cancellationToken = cts.Token;
        string tmpFile = null;
        try
        {
            var file = e.GetMultipleFiles().FirstOrDefault();
            using (var commonTransferContext = new CommonTransferContext(progress =>
            {
                progressUpdated?.Invoke(progress.Percent, progress.Message);
            }, file.Size))
            {
                var currentFileInfoStr = $"{file.Name} ({storageUSC.GetString(file.Size, 0, true)}B)";
                fileInfoHandler?.Invoke(currentFileInfoStr);
                tmpFile = Path.GetTempFileName();

                using (Stream stream = file.OpenReadStream(file.Size, cancellationToken))
                using (var fileStream = File.OpenWrite(tmpFile))
                    await commonTransferContext.TransferAsync(stream, fileStream, cancellationToken, file.Size);

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
}
