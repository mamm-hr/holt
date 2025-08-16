using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Holt.Configuration;

namespace Holt.Services;

public class RepositoryJob : IDisposable
{
    private readonly JobConfig _config;
    private readonly ILogger<RepositoryJob> _logger;
    private readonly CancellationTokenSource _cts = new();
    private Task? _task;

    public RepositoryJob(JobConfig config, ILogger<RepositoryJob> logger)
    {
        _config = config;
        _logger = logger;
    }

    public void Start()
    {
        _task = Task.Run(RunAsync);
    }

    private async Task RunAsync()
    {
        await EnsureCloneAsync();

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await SyncAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing {Name}", _config.Name);
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_config.IntervalMinutes), _cts.Token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private Task EnsureCloneAsync()
    {
        if (!Directory.Exists(_config.LocalPath))
        {
            _logger.LogInformation("Cloning {Url} into {Path}", _config.RepositoryUrl, _config.LocalPath);
            Repository.Clone(_config.RepositoryUrl, _config.LocalPath);
        }
        return Task.CompletedTask;
    }

    private Task SyncAsync()
    {
        using var repo = new Repository(_config.LocalPath);
        Commands.Fetch(repo, "origin", Array.Empty<string>(), new FetchOptions(), null);
        var branch = repo.Branches[$"origin/{_config.Branch}"];
        if (branch != null)
        {
            Commands.Checkout(repo, branch);
            repo.Reset(ResetMode.Hard, branch.Tip);
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Cancel();
        try
        {
            _task?.Wait();
        }
        catch
        {
            // ignore
        }
        _cts.Dispose();
    }
}
