using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quick.EntityFrameworkCore.Plus;
using Quick.EntityFrameworkCore.Plus.SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var dbFile = SQLiteDbContextConfigHandler.CONFIG_DB_FILE;
#if DEBUG
            dbFile = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), dbFile);
#endif
            ConfigDbContext.Init(new SQLiteDbContextConfigHandler(dbFile), modelBuilder =>
            {
                Quick.Blazor.Bootstrap.Admin.Global.Instance.OnModelCreating(modelBuilder);
                Quick.Blazor.Bootstrap.ReverseProxy.Global.Instance.OnModelCreating(modelBuilder);
            });
            using (var dbContext = new ConfigDbContext())
                dbContext.DatabaseEnsureCreatedAndUpdated(t => Debug.Print(t));
            ConfigDbContext.CacheContext.LoadCache();

            Quick.Blazor.Bootstrap.Admin.Core.CrontabManager.Instance.Start();

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
