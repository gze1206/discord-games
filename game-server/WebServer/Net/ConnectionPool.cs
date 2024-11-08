using System.Collections.Concurrent;

namespace WebServer.Net;

public class ConnectionPool
{
    private static readonly Lazy<ConnectionPool> Instance = new();
    public static ConnectionPool I => Instance.Value;

    private readonly ConcurrentQueue<Connection> pool = new();
    private readonly ConcurrentDictionary<UserId, Connection> activeConnections = new();

    public int Actives => this.activeConnections.Count;

    public bool Register(UserId userId, Connection conn) => this.activeConnections.TryAdd(userId, conn);
    public bool Unregister(UserId userId) => this.activeConnections.TryRemove(userId, out _);
    
    public Connection GetConnection(UserId userId) => this.activeConnections[userId];
    public Connection[] GetActiveConnections() => this.activeConnections.Values.ToArray();

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