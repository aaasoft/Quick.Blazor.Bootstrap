using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using d_.Code.Quick_Blazor_Bootstrap.Quick_Blazor_Bootstrap;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Shell.Utils;

namespace Quick.Blazor.Bootstrap.Admin;

public class CrontabManager
{
    public static CrontabManager Instance { get; } = new CrontabManager();
    private CancellationTokenSource cts;
    private Action<string> logger;
    private CronJobContext[] contextList;

    public void Start(Action<string> logger, string crontab)
    {
        var lines = crontab.Split('r', 'n');
        var list = new List<CronJobInfo>();
        foreach (var line in lines)
        {
            var segmentList = new List<string>();
            while (true)
            {
                var str = line.Trim();
                var index = str.IndexOf(' ');
                if (index < 0)
                    break;
                segmentList.Add(str.Substring(0, index));
                str = str.Substring(index + 1).Trim();
                if (segmentList.Count == 5)
                {
                    segmentList.Add(str);
                    break;
                }
            }
            if (segmentList.Count == 6)
            {
                list.Add(new CronJobInfo()
                {
                    Time = string.Join(' ', segmentList.Take(5)),
                    Command = segmentList[5]
                });
            }
        }
        Start(logger, list.ToArray());
    }

    private class CronJobContext
    {
        private NCrontab.CrontabSchedule crontabSchedule;
        public CronJobInfo JobInfo { get; private set; }
        public DateTime ExecuteTime { get; private set; }
        public CronJobContext(CronJobInfo jobInfo)
        {
            JobInfo = jobInfo;
            crontabSchedule = NCrontab.CrontabSchedule.Parse(jobInfo.Time);
            GenerateNextExcecuteTime();
        }

        public void GenerateNextExcecuteTime()
        {
            ExecuteTime = crontabSchedule.GetNextOccurrence(DateTime.Now);
        }
    }


    public void Start(Action<string> logger, params CronJobInfo[] jobs)
    {
        this.logger = logger;
        contextList = jobs.Select(t => new CronJobContext(t)).ToArray();
        cts?.Cancel();
        cts = new CancellationTokenSource();
        _ = ScheduleAsync(cts.Token);
    }

    private async Task ScheduleAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                var nowTime = DateTime.Now;
                foreach (var context in contextList)
                {
                    if (context.ExecuteTime >= nowTime)
                    {
                        var command = context.JobInfo.Command;
                        logger?.Invoke($"开始执行[{context.JobInfo.Time} {command}]");
                        var ret = ProcessUtils.ExecuteShell(command);
                        if (!string.IsNullOrEmpty(ret.Output))
                            logger?.Invoke(ret.Output);
                        if (!string.IsNullOrEmpty(ret.Error))
                            logger?.Invoke(ret.Error);
                        logger?.Invoke($"执行完成。退出码：{ret.ExitCode}");
                        context.GenerateNextExcecuteTime();
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            logger?.Invoke(ExceptionUtils.GetExceptionString(ex));
        }
    }

    public void Stop()
    {
        cts?.Cancel();
        cts = null;
    }
}
