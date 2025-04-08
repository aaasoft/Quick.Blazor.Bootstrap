using System.Text;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Localize;

namespace Quick.Blazor.Bootstrap.Admin.Core;

public class CommonTransferContext : IDisposable
{
    private static string TextSpeed => Locale.GetString("Speed");
    private static string TextRemainingTime => Locale.GetString("Remaining Time");

    private readonly UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;
    private System.Diagnostics.Stopwatch stopwatch;
    private DateTime lastNotifyTime = DateTime.MinValue;
    private int minNotifyInterval;
    private long totalCount;
    private long readCount;
    private byte[] buffer;
    private Action<TransferProgressInfo> progressUpdateHandler;
    
    public CommonTransferContext(Action<TransferProgressInfo> progressUpdateHandler, long totalCount, int bufferSize = 1 * 1024 * 1024, int minNotifyInterval = 500)
    {
        this.progressUpdateHandler = progressUpdateHandler;
        this.totalCount = totalCount;
        this.minNotifyInterval = minNotifyInterval;
        buffer = new byte[bufferSize];
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
    }

    public struct TransferProgressInfo
    {
        public int Percent { get; set; }
        public string Message { get; set; }

        public TransferProgressInfo(int percent, string message)
        {
            Percent = percent;
            Message = message;
        }
    }

    public async Task TransferAsync(Stream srcStream, Func<ArraySegment<byte>, Task> desHandler, CancellationToken cancellationToken = default, long transferSize = 0)
    {
        var isSetTransferSize = transferSize > 0;
        long currentTransferReadCount = 0;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            var ret = await srcStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            readCount += ret;
            if (isSetTransferSize)
            {
                if (ret <= 0)
                {
                    await Task.Delay(100);
                    continue;
                }
                else
                {
                    currentTransferReadCount += ret;
                }
            }
            else
            {
                if (ret <= 0)
                    break;
            }
            await desHandler.Invoke(new ArraySegment<byte>(buffer, 0, ret));
            if ((DateTime.Now - lastNotifyTime).TotalMilliseconds > minNotifyInterval && stopwatch.ElapsedMilliseconds > 0)
            {
                lastNotifyTime = DateTime.Now;
                StringBuilder sb = new StringBuilder();
                var speed = readCount / stopwatch.Elapsed.TotalMilliseconds;
                sb.Append(TextSpeed + ": " + storageUSC.GetString(Convert.ToDecimal(speed * 1000), 1, true) + "B/s");
                var transferProgressInfo = new TransferProgressInfo();
                if (totalCount > 0)
                {
                    var remainingTime = TimeSpan.FromMilliseconds((totalCount - readCount) / speed);
                    sb.Append("," + TextRemainingTime + ": " + remainingTime.ToString(@"hh\:mm\:ss"));
                    transferProgressInfo.Percent = Convert.ToInt32(readCount * 100 / totalCount);
                }
                else
                {
                    transferProgressInfo.Percent = 0;
                }
                transferProgressInfo.Message = sb.ToString();
                progressUpdateHandler?.Invoke(transferProgressInfo);
            }
            if (isSetTransferSize && currentTransferReadCount >= transferSize)
                break;
        }
    }

    public async Task TransferAsync(Stream srcStream, Stream desStream, CancellationToken cancellationToken = default, long transferSize = 0)
    {
        await TransferAsync(srcStream, async m =>
        {
            await desStream.WriteAsync(m, cancellationToken);
        }, cancellationToken, transferSize);
    }

    public void Dispose()
    {
        stopwatch.Stop();
    }
}
