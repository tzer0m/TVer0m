using TVer0m.Web.Detection;
using TVer0m.Web.Endpoints;
using TVer0m.Web.Mocks;
using TVer0m.Web.Services;

// When published, wwwroot sits next to the binary, whatever the working directory is.
string publishedRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
WebApplicationOptions options = new() { Args = args, ContentRootPath = Directory.Exists(publishedRoot) ? AppContext.BaseDirectory : null };
WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

// Listen on the LAN unless a URL was given, for example by launchSettings.
if (string.IsNullOrEmpty(builder.Configuration["urls"]))
{
    builder.WebHost.UseUrls("http://0.0.0.0:8090");
}

// Real implementations on Linux, logging mocks elsewhere.
builder.Services.AddSingleton<DeviceDetector>();
if (OperatingSystem.IsLinux())
{
    builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
    builder.Services.AddSingleton<IInputInjector, InputInjector>();
    builder.Services.AddSingleton<IVolumeController, VolumeController>();
    builder.Services.AddHttpClient<IBrowserController, BrowserController>();
}
else
{
    builder.Services.AddSingleton<IInputInjector, MockInputInjector>();
    builder.Services.AddSingleton<IVolumeController, MockVolumeController>();
    builder.Services.AddSingleton<IBrowserController, MockBrowserController>();
}

// Serve the static pages without caching so updates show straight away, then map the routes.
WebApplication app = builder.Build();
app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-cache" });
app.MapPages().MapApi();
app.Run();