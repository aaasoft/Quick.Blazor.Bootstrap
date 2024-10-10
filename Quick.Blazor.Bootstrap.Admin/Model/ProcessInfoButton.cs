using System;

namespace Quick.Blazor.Bootstrap.Admin.Model;

public class ProcessInfoButton
{
    public string Name { get; set; }
    public Func<ProcessInfo, bool> IsVisiableFunc { get; set; }
    public Action<ProcessInfo> OnClickAction { get; set; }
}
