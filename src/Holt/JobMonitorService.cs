using System.Xml.Serialization;
using Holt.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Holt.Services;

public class JobMonitorService : BackgroundService
{
    private readonly ILogger<JobMonitorService> _logger;
    private readonly IServiceProvider _services;
    private readonly Dictionary<string, RepositoryJob> _jobs = new();
    private FileSystemWatcher? _watcher;
    private readonly string _jobDirectory;

    public JobMonitorService(ILogger<JobMonitorService> logger, IServiceProvider services, IConfiguration configuration)
    {
        _logger = logger;
        _services = services;
        _jobDirectory = configuration["JobDirectory"] ?? Path.Combine(AppContext.BaseDirectory, "jobs");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(_jobDirectory))
        {
            Directory.CreateDirectory(_jobDirectory);
        }

        foreach (var file in Directory.GetFiles(_jobDirectory, "*.xml"))
        {
            StartOrUpdateJob(file);
        }

        _watcher = new FileSystemWatcher(_jobDirectory, "*.xml")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };
        _watcher.Created += (s, e) => StartOrUpdateJob(e.FullPath);
        _watcher.Changed += (s, e) => StartOrUpdateJob(e.FullPath);
        _watcher.Deleted += (s, e) => RemoveJob(e.FullPath);
        _watcher.Renamed += (s, e) =>
        {
            RemoveJob(e.OldFullPath);
            StartOrUpdateJob(e.FullPath);
        };
        _watcher.EnableRaisingEvents = true;

        return Task.CompletedTask;
    }

    private void StartOrUpdateJob(string path)
    {
        try
        {
            var config = LoadConfig(path);
            if (config == null)
            {
                return;
            }

            if (_jobs.TryGetValue(path, out var existing))
            {
                existing.Dispose();
                _jobs.Remove(path);
            }

            var jobLogger = _services.GetRequiredService<ILogger<RepositoryJob>>();
            var job = new RepositoryJob(config, jobLogger);
            job.Start();
            _jobs[path] = job;
            _logger.LogInformation("Started job {Name}", config.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start job from {Path}", path);
        }
    }

    private void RemoveJob(string path)
    {
        if (_jobs.Remove(path, out var job))
        {
            job.Dispose();
            _logger.LogInformation("Stopped job for {Path}", path);
        }
    }

    private static JobConfig? LoadConfig(string path)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(JobConfig));
            using var stream = File.OpenRead(path);
            return serializer.Deserialize(stream) as JobConfig;
        }
        catch
        {
            return null;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();
        foreach (var job in _jobs.Values)
        {
            job.Dispose();
        }
        _jobs.Clear();
        return base.StopAsync(cancellationToken);
    }
}
