using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Versioning;
using System.Text;
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
                    if (StartTime == DateTime.MinValue)
                        StartTime = Directory.GetCreationTime($"/proc/{PID}");
                    CmdLine = File.ReadAllText($"/proc/{PID}/cmdline").Trim().Replace('\0', ' ');
                    FileName = ProcessUtils.ExecuteShell($"readlink /proc/{PID}/exe").Output.Trim();
                    WorkingDirectory = ProcessUtils.ExecuteShell($"readlink /proc/{PID}/cwd").Output.Trim();
                }
            }
            else if (OperatingSystem.IsWindows())
            {
                if (includeDetail)
                {
                    try
                    {
                        if (StartTime == DateTime.MinValue)
                        {
                            var line = GetWmicResult($"wmic process where ProcessId={PID} get CreationDate");
                            var sb = new StringBuilder(line.Split('+')[0]);
                            sb.Insert(12, ':');
                            sb.Insert(10, ':');
                            sb.Insert(8, ' ');
                            sb.Insert(6, '-');
                            sb.Insert(4, '-');
                            StartTime = DateTime.Parse(sb.ToString());
                        }
                        CmdLine = GetWmicResult($"wmic process where ProcessId={PID} get CommandLine");
                        FileName = GetWmicResult($"wmic process where ProcessId={PID} get ExecutablePath");
                        WorkingDirectory = process.StartInfo?.WorkingDirectory;
                    }
                    catch { }
                }
            }
            else
            {
                if (includeDetail)
                {
                    try
                    {
                        var psi = process.StartInfo;
                        FileName = psi.FileName;
                        CmdLine = string.Join(' ', psi.ArgumentList);
                        WorkingDirectory = process.StartInfo?.WorkingDirectory;
                    }
                    catch { }
                }
            }
        }
        catch
        {
        }
    }

    private string GetWmicResult(string commandLine)
    {
        var ret = ProcessUtils.ExecuteShell(commandLine);
        if (ret.ExitCode != 0)
            return null;
        return string.Join(' ', ret.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Skip(1)).Trim();
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
                var segments = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
            .Where(t => t != null && t.PID != ret.ProcessId)
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
                var segments = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
            .Where(t => t != null && t.PID != ret.ProcessId)
            .ToArray();
        }
    }
}
