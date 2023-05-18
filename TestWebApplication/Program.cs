using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quick.Blazor.Bootstrap.ReverseProxy;
using Quick.EntityFrameworkCore.Plus;
using Quick.EntityFrameworkCore.Plus.SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var dbFile = SQLiteDbContextConfigHandler.CONFIG_DB_FILE;
#if DEBUG
            dbFile = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), dbFile);
#endif
            ConfigDbContext.Init(new SQLiteDbContextConfigHandler(dbFile), modelBuilder =>
            {
                Global.Instance.OnModelCreating(modelBuilder);
            });
            using (var dbContext = new ConfigDbContext())
                dbContext.DatabaseEnsureCreatedAndUpdated(t => Debug.Print(t));
            ConfigDbContext.CacheContext.LoadCache();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
