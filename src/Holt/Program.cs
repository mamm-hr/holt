using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Holt.Services;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Allow running as Windows service or systemd service
builder.Host.UseWindowsService();
builder.Host.UseSystemd();

// Register background services
builder.Services.AddHostedService<JobMonitorService>();

// Configure logging
if (OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog(config =>
    {
        config.SourceName = "Holt";
    });
}
else
{
    builder.Logging.AddEventSourceLogger();
}

var host = builder.Build();

await host.RunAsync();
