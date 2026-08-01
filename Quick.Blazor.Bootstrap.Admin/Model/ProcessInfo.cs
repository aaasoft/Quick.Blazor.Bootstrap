using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Shell.PowerShell;
using Quick.Shell.Utils;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

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
            if (OperatingSystem.IsMacOS())
            {
                if (includeDetail)
                {
                    CmdLine = ProcessUtils.ExecuteShell($"ps -o command -p {PID}").Output?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)?.LastOrDefault()?.Trim();
                    FileName = ProcessUtils.ExecuteShell($"ps -o comm -p {PID}").Output?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)?.LastOrDefault()?.Trim();
                    WorkingDirectory = ProcessUtils.ExecuteShell($"lsof -d cwd | grep {PID}").Output?.Trim()?.Split(" ", StringSplitOptions.RemoveEmptyEntries)?.LastOrDefault();
                }
            }
            else if (OperatingSystem.IsLinux())
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
                            var line = WmiUtils.GetValue("Win32_Process", $"ProcessId={PID}", "CreationDate");
                            if (line.Contains("+"))
                            {
                                var sb = new StringBuilder(line.Split('+')[0]);
                                sb.Insert(12, ':');
                                sb.Insert(10, ':');
                                sb.Insert(8, ' ');
                                sb.Insert(6, '-');
                                sb.Insert(4, '-');
                                line = sb.ToString();
                            }
                            StartTime = DateTime.Parse(line);
                        }
                        CmdLine = WmiUtils.GetValue("Win32_Process", $"ProcessId={PID}", "CommandLine");
                        FileName = WmiUtils.GetValue("Win32_Process", $"ProcessId={PID}", "ExecutablePath");
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
            var ret = WmiUtils.Query("Win32_Process", $"ParentProcessId={PID}", "Name", "ProcessId");
            return ret
            .Select(dict =>
            {
                return new ProcessInfo()
                {
                    PID = int.Parse(dict["ProcessId"]),
                    Name = dict["Name"]
                };
            })
            .ToArray();
        }
        else
        {
            IEnumerable<ProcessInfo> processes = null;
            if (OperatingSystem.IsMacOS())
            {
                var ret = ProcessUtils.ExecuteShell($"ps -ax -o pid,ppid,ucomm | grep {PID}");
                if (ret.ExitCode != 0)
                    return null;
                processes = ret.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line=>
                    {
                        var segments = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (segments.Length < 3)
                            return null;
                        var pid = int.Parse(segments[0]);
                        var ppid = int.Parse(segments[1]);
                        if (ppid != PID)
                            return null;
                        var name = string.Join(' ', segments.Skip(2));
                        return new ProcessInfo()
                        {
                            PID = pid,
                            Name = name
                        };
                    })
                    .Where(t => t != null && t.PID != ret.ProcessId);
            }
            else
            {
                var ret = ProcessUtils.ExecuteShell($"ps --ppid {PID}");
                if (ret.ExitCode != 0)
                    return null;
                processes = ret.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Skip(1)
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
                    .Where(t => t != null && t.PID != ret.ProcessId);
            }
            return processes.ToArray();
        }
    }
}
