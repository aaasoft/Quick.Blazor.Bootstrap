using System;
using Microsoft.EntityFrameworkCore;

namespace Quick.Blazor.Bootstrap.Admin;

public class Global
{
    public static Global Instance { get; } = new Global();

    public void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model.CommonConfig>();
    }
}
