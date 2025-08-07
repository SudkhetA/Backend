
using Backend.Models;

namespace Backend.Services.BackgroundTask;

public class TransactionLogService(
    ILogger<TransactionLogService> _logger,
    IServiceScopeFactory _serviceScope)
    : IHostedService, IDisposable
{
    private Timer? _timer = null;

    ~TransactionLogService() => Dispose(false);
    private bool _disposed;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;

            if (disposing)
            {
                _timer?.Dispose();
                _timer = null;
            }
        }
    }

    // Delete Transaction Log created more then 1 month
    private async Task DoWork(object? state)
    {
        using var scope = _serviceScope.CreateAsyncScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var dueDate = DateTime.Now.AddMonths(-1);
        var data = context.TransactionLogs.Where(x => x.TimeStamp < dueDate);

        context.TransactionLogs.RemoveRange(data);
        await context.SaveChangesAsync();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        // start at 01:00
        var dueTime = DateTime.Now - DateTime.Now.Date.AddHours(25);

        _timer = new Timer(async (obj) => await DoWork(obj), null, dueTime, TimeSpan.FromDays(1));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }
}
