using System.Collections.Concurrent;
using System.Data.Common;
using System.Text;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class ConnectionPoolManager : IDisposable
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _throttles = new();
    private readonly int _maxConcurrentPerKey;
    private bool _disposed;

    public ConnectionPoolManager(int maxConcurrentPerKey = 5)
    {
        _maxConcurrentPerKey = maxConcurrentPerKey;
    }

    public async Task<PooledConnection> AcquireAsync(IDatabaseProvider provider, string connectionString)
    {
        // 使用连接字符串本身而非 GetHashCode() 避免 hash 冲突
        var key = $"{provider.ProviderType}:{Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(connectionString ?? string.Empty)))}";
        var throttle = _throttles.GetOrAdd(key, _ => new SemaphoreSlim(_maxConcurrentPerKey));
        await throttle.WaitAsync();

        try
        {
            var conn = provider.CreateConnection(connectionString);
            await conn.OpenAsync();
            return new PooledConnection(conn, () => throttle.Release());
        }
        catch
        {
            throttle.Release();
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var throttle in _throttles.Values)
            throttle.Dispose();
        _throttles.Clear();
    }
}

public sealed class PooledConnection : IAsyncDisposable, IDisposable
{
    private readonly DbConnection _connection;
    private readonly Action _onRelease;
    private bool _disposed;

    public PooledConnection(DbConnection connection, Action onRelease)
    {
        _connection = connection;
        _onRelease = onRelease;
    }

    public DbConnection Connection => _connection;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connection.Dispose();
        _onRelease();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await _connection.DisposeAsync();
        _onRelease();
    }
}
