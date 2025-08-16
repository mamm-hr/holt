using Holt;
using Holt.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Allow running as Windows service or systemd service
builder.Services.AddWindowsService();
builder.Services.AddSystemd();

// Register background services
builder.Services.AddHostedService<JobMonitorService>();

// Configure logging
PlatformSpecific.ConfigureLogging( builder.Logging );

await builder.Build().RunAsync();
