using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Versioning;
using Quick.Shell.Utils;

namespace Quick.Blazor.Bootstrap.Admin;

public class ProcessInfo
{
    public int PID { get; set; }
    public string Name { get; set; }
    public string FileName { get; set; }
    public string CmdLine { get; set; }
    public string WorkingDirectory { get; set; }
    public int ThreadsCount { get; set; }
    public long Memory { get; set; }
    public DateTime StartTime { get; set; }

    public ProcessInfo() { }

    [UnsupportedOSPlatform("browser")]
    public ProcessInfo(int pid, bool includeDetail = false)
    : this(Process.GetProcessById(pid), includeDetail) { }

    [UnsupportedOSPlatform("browser")]
    public ProcessInfo(Process process, bool includeDetail = false)
    {
        try
        {
            PID = process.Id;
            Name = process.ProcessName;
            ThreadsCount = process.Threads.Count;
            Memory = process.WorkingSet64;
            if (includeDetail)
            {
                try { StartTime = process.StartTime; }
                catch { }
            }
            if (OperatingSystem.IsLinux())
            {
                Name = File.ReadAllText($"/proc/{PID}/comm").Trim();
                if (includeDetail)
                {
                    CmdLine = File.ReadAllText($"/proc/{PID}/cmdline").Trim().Replace('\0', ' ');
                    FileName = ProcessUtils.ExecuteShell($"readlink /proc/{PID}/exe").Output.Trim();
                    WorkingDirectory = ProcessUtils.ExecuteShell($"readlink /proc/{PID}/cwd").Output.Trim();
                }
            }
            else
            {
                if (includeDetail)
                {
                    try
                    {
                        var processStartInfo = process.StartInfo;
                        FileName = processStartInfo?.FileName;
                        CmdLine = processStartInfo?.FileName;
                        var argumentList = processStartInfo?.ArgumentList;
                        if (argumentList != null && argumentList.Count > 0)
                            CmdLine += " " + string.Join(" ", argumentList);
                        WorkingDirectory = processStartInfo?.WorkingDirectory;
                    }
                    catch { }
                }
            }
        }
        catch
        {
        }
    }

    public ProcessInfo[] GetChildProcesses()
    {
        if (OperatingSystem.IsWindows())
        {
            var ret = ProcessUtils.ExecuteShell($"wmic process where ParentProcessId={PID} get Name,ProcessId");
            if (ret.ExitCode != 0)
                return null;
            return ret.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Skip(1)
            .Select(line =>
            {
                var segments = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length < 2)
                    return null;
                var pid = int.Parse(segments.Last());
                var name = string.Join(' ', segments.Take(segments.Length - 1));
                return new ProcessInfo()
                {
                    PID = pid,
                    Name = name
                };
            })
            .Where(t => t != null)
            .ToArray();
        }
        else
        {
            var ret = ProcessUtils.ExecuteShell($"ps --ppid {PID}");
            if (ret.ExitCode != 0)
                return null;
            return ret.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Skip(1)
            .Select(line =>
            {
                var segments = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length < 4)
                    return null;
                var pid = int.Parse(segments[0]);
                var name = string.Join(' ', segments.Skip(3));
                return new ProcessInfo()
                {
                    PID = pid,
                    Name = name
                };
            })
            .Where(t => t != null)
            .ToArray();
        }
    }
}
