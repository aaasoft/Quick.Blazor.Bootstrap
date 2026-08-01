using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Management.Infrastructure;
using Quick.Shell.Utils;

namespace Quick.Blazor.Bootstrap.Admin.Utils;

public static class WmiUtils
{
    [SupportedOSPlatform("windows")]
    public static string GetValue(string table, string condition, string propertyName)
    {
        try
        {
            string wql = null;
            if (string.IsNullOrEmpty(condition))
                wql = $"SELECT {propertyName} FROM {table}";
            else
                wql = $"SELECT {propertyName} FROM {table} WHERE {condition}";
            using (var session = CimSession.Create(null))
            {
                var instances = session.QueryInstances(@"root/cimv2", "WQL",
                    wql);
                foreach (var inst in instances)
                    using (inst)
                    {
                        var obj = inst.CimInstanceProperties[propertyName].Value;
                        if (obj is DateTime dt)
                            return dt.ToString("yyyy-MM-dd HH:mm:ss");
                        return obj?.ToString();
                    }
            }
            return default;
        }
        catch
        {
            string commandLine;
            if (string.IsNullOrEmpty(condition))
                commandLine = $"wmic path {table} get {propertyName}";
            else
                commandLine = $"wmic path {table} where {condition} get {propertyName}";
            var ret = ProcessUtils.ExecuteShell(commandLine);
            if (ret.ExitCode != 0)
                return default;
            return string.Join(' ', ret.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Skip(1)).Trim();
        }
    }

    private static Regex regexWmicResultHead = new Regex(@"(?<name>\w+\s*)");
    internal struct WmicResultHead
    {
        public string Name;
        public int Index;
        public int Length;
    }

    [SupportedOSPlatform("windows")]
    public static List<Dictionary<string, string>> Query(string table, string condition, out int? processId, params string[] propertyNames)
    {
        processId = null;
        var list = new List<Dictionary<string, string>>();
        var propertyNamesString = string.Join(',', propertyNames);
        try
        {
            string wql = null;
            if (string.IsNullOrEmpty(condition))
                wql = $"SELECT {propertyNamesString} FROM {table}";
            else
                wql = $"SELECT {propertyNamesString} FROM {table} WHERE {condition}";
            using (var session = CimSession.Create(null))
            {
                var instances = session.QueryInstances(@"root/cimv2", "WQL",
                    wql);
                foreach (var inst in instances)
                    using (inst)
                    {
                        var dict = new Dictionary<string, string>();
                        foreach (var propertyName in propertyNames)
                        {
                            var obj = inst.CimInstanceProperties[propertyName].Value;
                            if (obj is DateTime dt)
                                dict[propertyName] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            else
                                dict[propertyName] = obj?.ToString();
                        }
                        list.Add(dict);
                    }
            }
        }
        catch
        {
            string commandLine;
            if (string.IsNullOrEmpty(condition))
                commandLine = $"wmic path {table} get {propertyNamesString}";
            else
                commandLine = $"wmic path {table} where {condition} get {propertyNamesString}";
            var ret = ProcessUtils.ExecuteShell(commandLine);
            processId = ret.ProcessId;
            if (ret.ExitCode != 0)
                return list;
            var lines = ret.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
                return list;
            var headLine = lines[0];
            ICollection<Match> matchCollection = regexWmicResultHead.Matches(headLine);
            if (matchCollection.Count == 0)
                return list;
            var headList = new List<WmicResultHead>();
            foreach (var match in matchCollection)
            {
                headList.Add(new WmicResultHead()
                {
                    Name = match.Value.Trim(),
                    Index = match.Index,
                    Length = match.Length
                });
            }
            foreach (var line in lines.Skip(1))
            {
                var dict = new Dictionary<string, string>();
                foreach (var head in headList)
                {
                    var length = Math.Min(head.Length, line.Length - head.Index);
                    dict[head.Name] = line.Substring(head.Index, length).Trim();
                }
                list.Add(dict);
            }
        }
        return list;
    }
}