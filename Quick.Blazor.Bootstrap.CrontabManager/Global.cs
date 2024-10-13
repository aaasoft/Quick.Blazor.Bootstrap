using System;
using Quick.LiteDB.Plus;

namespace Quick.Blazor.Bootstrap.CrontabManager;

public class Global
{
    public static Global Instance { get; } = new Global();

    public void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model.CommonConfig>(c => c.EnsureIndex(t => t.Id, true));
    }
}
