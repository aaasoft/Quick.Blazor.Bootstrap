using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.EntityFrameworkCore.Plus;
using Quick.Localize;
using Quick.Shell.Utils;

namespace Quick.Blazor.Bootstrap.Admin.Core;

public class CrontabManager
{
    private const string CONFIG_ID = "crontab";
    public static CrontabManager Instance { get; } = new CrontabManager();
    private CancellationTokenSource cts;
    private CronJobContext[] contextList;

    private static string TextTrace => Locale.GetString("Trace");
    private static string TextDebug => Locale.GetString("Debug");
    private static string TextInformation => Locale.GetString("Information");
    private static string TextWarning => Locale.GetString("Warning");
    private static string TextError => Locale.GetString("Error");

    private static string TextJobString => Locale.GetString("Job[{0} {1}]");
    private static string TextNoJobStartCancelled => Locale.GetString("Current has no jobs,start cancelled.");
    private static string TextJobLoadedFirstExecuteTime => Locale.GetString("{0} loaded,first execute time: {1}");
    private static string TextJobExecuting => Locale.GetString("{0} executing...");
    private static string TextJobDone => Locale.GetString("Job done,exit code: {0}");
    private static string TextJobNextExecuteTime => Locale.GetString("Job next execute time: {0}");
    private static string TextStarted => Locale.GetString("Started");
    private static string TextStoped => Locale.GetString("Stoped");

    //日志最多1000行
    public const int MAX_CONSOLE_OUTPUT_LINES = 1000;
    //日志最多50K个字符
    public const int MAX_CONSOLE_OUTPUT_CHARS = 50 * 1024;
    private int consoleOutputCharCount = 0;
    public Queue<string> ConsoleOutputQueue { get; private set; } = new Queue<string>();
    public bool IsStarted { get; private set; } = false;
    public string ConsoleHistory
    {
        get
        {
            lock (ConsoleOutputQueue)
                return string.Join(Environment.NewLine, ConsoleOutputQueue);
        }
    }
    public string[] ConsoleHistoryLines
    {
        get
        {
            lock (ConsoleOutputQueue)
                return ConsoleOutputQueue.ToArray();
        }
    }

    public event EventHandler<string> NewConsoleHistory;
    public event EventHandler ConsoleHistoryChanged;

    private void addConsoleHistory(string line)
    {
        line = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {line}";
        lock (ConsoleOutputQueue)
        {
            while (ConsoleOutputQueue.Count > MAX_CONSOLE_OUTPUT_LINES
                || consoleOutputCharCount > MAX_CONSOLE_OUTPUT_CHARS)
            {
                var delLine = ConsoleOutputQueue.Dequeue();
                consoleOutputCharCount -= delLine.Length;
            }
            ConsoleOutputQueue.Enqueue(line);
            consoleOutputCharCount += line.Length;
        }
        ConsoleHistoryChanged?.Invoke(this, EventArgs.Empty);
        NewConsoleHistory?.Invoke(this, line);
    }

    private void pushLog(LogLevel level, string message)
    {
        var levelString = Locale.GetString(level.ToString());
        addConsoleHistory($"[{levelString}] {message}");
    }

    private CrontabManager() { }


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

        public override string ToString()
        {
            return string.Format(TextJobString, JobInfo.Time, JobInfo.Command);
        }
    }

    public void Start(string crontab)
    {
        var list = new List<CronJobInfo>();
        if (!string.IsNullOrEmpty(crontab))
        {
            var lines = crontab.Split('\r', '\n');
            foreach (var line in lines)
            {
                var str = line.Trim();
                var segmentList = new List<string>();
                while (true)
                {
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
        }
        Start(list.ToArray());
    }

    public void Start()
    {
        Start(Load());
    }

    public string Load()
    {
        return ConfigDbContext.CacheContext.Find(new Model.CommonConfig() { Id = CONFIG_ID })?.Value;
    }

    public void Save(string crontab)
    {
        var existModel = ConfigDbContext.CacheContext.Find(new Model.CommonConfig() { Id = CONFIG_ID });
        if (existModel == null)
        {
            ConfigDbContext.CacheContext.Add(new Model.CommonConfig()
            {
                Id = CONFIG_ID,
                Value = crontab
            });
        }
        else
        {
            existModel.Value = crontab;
            ConfigDbContext.CacheContext.Update(existModel);
        }
    }

    public void Start(params CronJobInfo[] jobs)
    {
        try
        {
            if (jobs == null || jobs.Length == 0)
            {
                pushLog(LogLevel.Information, TextNoJobStartCancelled);
                return;
            }
            IsStarted = true;
            contextList = jobs.Select(t =>
            {
                var context = new CronJobContext(t);
                pushLog(LogLevel.Information, string.Format(TextJobLoadedFirstExecuteTime, context, context.ExecuteTime));
                return context;
            }).ToArray();
            cts?.Cancel();
            cts = new CancellationTokenSource();
            _ = ScheduleAsync(cts.Token);
            pushLog(LogLevel.Information, TextStarted);
        }
        catch (Exception ex)
        {
            Stop();
            pushLog(LogLevel.Error, ExceptionUtils.GetExceptionString(ex));
        }
    }

    private async Task ScheduleAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
                var nowTime = DateTime.Now;
                foreach (var context in contextList)
                {
                    if (context.ExecuteTime <= nowTime)
                    {
                        var command = context.JobInfo.Command;
                        pushLog(LogLevel.Information, "-----------------------");
                        pushLog(LogLevel.Information, string.Format(TextJobExecuting, context));
                        pushLog(LogLevel.Information, "-----------------------");
                        var ret = ProcessUtils.ExecuteShell(command);
                        if (!string.IsNullOrEmpty(ret.Output))
                            pushLog(LogLevel.Information, ret.Output);
                        if (!string.IsNullOrEmpty(ret.Error))
                            pushLog(LogLevel.Error, ret.Error);
                        pushLog(LogLevel.Information, string.Format(TextJobDone, ret.ExitCode));
                        context.GenerateNextExcecuteTime();
                        pushLog(LogLevel.Information, string.Format(TextJobNextExecuteTime, context.ExecuteTime));
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            pushLog(LogLevel.Error, ExceptionUtils.GetExceptionString(ex));
        }
    }

    public void Stop()
    {
        cts?.Cancel();
        cts = null;
        IsStarted = false;
        pushLog(LogLevel.Information, TextStoped);
    }
}
