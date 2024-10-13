using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Quick.LiteDB.Plus;
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

            var dbFile = "Config.litedb";
#if DEBUG
            dbFile = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), dbFile);
#endif
            ConfigDbContext.Init(dbFile, modelBuilder =>
            {
                Quick.Blazor.Bootstrap.CrontabManager.Global.Instance.OnModelCreating(modelBuilder);
                Quick.Blazor.Bootstrap.ReverseProxy.Global.Instance.OnModelCreating(modelBuilder);
            });
            ConfigDbContext.CacheContext.LoadCache();
            Quick.Blazor.Bootstrap.CrontabManager.Core.CrontabManager.Instance.Start();

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
