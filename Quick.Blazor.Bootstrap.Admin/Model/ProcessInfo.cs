using System;
using System.Diagnostics;
using System.IO;
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
    : this(Process.GetProcessById(pid)) { }

    [UnsupportedOSPlatform("browser")]
    public ProcessInfo(Process process, bool includeDetail = false)
    {
        PID = process.Id;
        var ProcessHasExited = process.HasExited;
        try
        {
            if (!ProcessHasExited)
            {
                Name = process.ProcessName;
                ThreadsCount = process.Threads.Count;
                Memory = process.WorkingSet64;
            }
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
            process.Refresh();
            ProcessHasExited = process.HasExited;
            if (!ProcessHasExited)
                throw;
        }
    }
}
