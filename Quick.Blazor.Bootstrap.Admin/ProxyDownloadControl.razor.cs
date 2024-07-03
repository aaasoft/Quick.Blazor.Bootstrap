using System;
using Microsoft.AspNetCore.Components;
using BlazorDownloadFile;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http.Headers;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Localize;

namespace Quick.Blazor.Bootstrap.Admin;

public partial class ProxyDownloadControl : ComponentBase_WithGettextSupport
{
    private UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;
    private static string TextDownload => Locale.GetString("Download");
    private static string TextCanceled => Locale.GetString("Canceled");
    private static string TextFailed => Locale.GetString("Failed");
    private static string TextSpeed => Locale.GetString("Speed");
    private static string TextRemainingTime => Locale.GetString("Remaining Time");

    private ModalLoading modalLoading;
    private ModalAlert modalAlert;

    [Inject]
    private IBlazorDownloadFileService BlazorDownloadFileService { get; set; }

    private string url { get; set; }

    private string getFileNameFromUrl(string url)
    {
        var uri = new Uri(url);
        var path = uri.LocalPath;
        return Path.GetFileName(path);
    }

    private async void Ok()
    {
        var cts = new System.Threading.CancellationTokenSource();
        var cancellationToken = cts.Token;
        var downloadUrl = url;

        modalLoading?.Show(TextDownload, downloadUrl, false, cts.Cancel);
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        DateTime lastDisplayTime = DateTime.MinValue;
        byte[] buffer = new byte[24 * 1024];
        long readTotalCount = 0;

        try
        {
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            };
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Quick.Blazor.Bootstrap", "1.0"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            var rep = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            downloadUrl = rep.RequestMessage.RequestUri.ToString();
            modalLoading.UpdateContent(downloadUrl);
            string fileName = null;
            long fileSize = -1;
            var contentHeader = rep.Content.Headers;
            {
                if (contentHeader.TryGetValues("Content-Length", out var values))
                {
                    fileSize = long.Parse(values.First());
                }
            }
            {
                if (contentHeader.TryGetValues("Content-Disposition", out var values))
                {
                    var line = values.First();
                    var segments = line.Split(";");
                    if (segments.Length >= 2 && segments[0] == "attachment")
                    {
                        var segment = segments[1];
                        var strs = segment.Split("=");
                        if (strs.Length >= 2 && strs[0] == "filename")
                            fileName = strs[1];
                    }
                }
            }
            if (string.IsNullOrEmpty(fileName))
                fileName = getFileNameFromUrl(downloadUrl);
            var loadingContent = fileName;
            if (fileSize > 0)
                loadingContent += $" ({storageUSC.GetString(fileSize, 1, true)}B)";
            modalLoading.UpdateContent(loadingContent);
            using (var repStream = await rep.Content.ReadAsStreamAsync())
            {
                while (!cts.IsCancellationRequested)
                {
                    var ret = await repStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (ret <= 0)
                        break;
                    readTotalCount += ret;

                    if ((DateTime.Now - lastDisplayTime).TotalSeconds > 0.5 && stopwatch.ElapsedMilliseconds > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        var speed = Convert.ToDouble(readTotalCount / stopwatch.ElapsedMilliseconds);
                        sb.Append(TextSpeed + ": " + storageUSC.GetString(Convert.ToDecimal(speed * 1000), 1, true) + "B/s");
                        if (fileSize > 0)
                        {
                            var remainingTime = TimeSpan.FromMilliseconds((fileSize - readTotalCount) / speed);
                            sb.Append("," + TextRemainingTime + ": " + remainingTime.ToString(@"hh\:mm\:ss"));
                            modalLoading.UpdateProgress(Convert.ToInt32(readTotalCount * 100 / fileSize), sb.ToString());
                        }
                        else
                        {
                            modalLoading.UpdateProgress(0, sb.ToString());
                        }
                        await InvokeAsync(StateHasChanged);
                        lastDisplayTime = DateTime.Now;
                    }
                    await BlazorDownloadFileService.AddBuffer(new ArraySegment<byte>(buffer, 0, ret), cancellationToken);
                }
            }
            var result = await BlazorDownloadFileService.DownloadBinaryBuffers(fileName, cancellationToken);
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
            if (readTotalCount > 0)
                await BlazorDownloadFileService.ClearBuffers();
            stopwatch.Stop();
            modalLoading?.Close();
        }
    }
}
