using System.Text;
using BlazorDownloadFile;
using Quick.LiteDB.Plus;
using Test.AspNetCore9.Components;
using Tewr.Blazor.FileReader;


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


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorDownloadFile();
builder.Services.AddFileReaderService();
Quick.Blazor.Bootstrap.ReverseProxy.ReverseProxyManager.Instance.Load(builder.Services.AddReverseProxy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapReverseProxy();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
