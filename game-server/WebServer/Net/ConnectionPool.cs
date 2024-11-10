using System.Collections.Concurrent;

namespace DiscordGames.WebServer.Net;

public class ConnectionPool
{
    private static readonly Lazy<ConnectionPool> Instance = new();
    public static ConnectionPool I => Instance.Value;

    private readonly ConcurrentQueue<Connection> pool = new();
    private readonly ConcurrentDictionary<UserId, Connection> activeConnections = new();

    public bool Register(Connection conn)
    {
        if (conn.UserId == 0) CoreThrowHelper.ThrowInvalidOperation();
        return this.activeConnections.TryAdd(conn.UserId, conn);
    }

    public bool Unregister(UserId userId) => this.activeConnections.TryRemove(userId, out _);
    
    public IEnumerable<Connection> GetActiveConnections() => this.activeConnections.Values.ToArray();

    public Connection Rent(ILogger<Connection> logger, IClusterClient cluster)
    {
        if (this.pool.TryDequeue(out var connection)) return connection;

        return new Connection(logger, cluster);
    }

    public void Return(Connection connection)
    {
        connection.Dispose();
        this.pool.Enqueue(connection);
    }
}