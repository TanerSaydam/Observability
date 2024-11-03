using StackExchange.Redis;

namespace Observability.Orders.WebAPI;

public sealed class RedisService
{
    private readonly ConnectionMultiplexer connectionMultiplexer;

    public RedisService()
    {
        var host = "localhost";
        var port = 6379;

        var config = $"{host}:{port}";

        connectionMultiplexer = ConnectionMultiplexer.Connect(config);
    }

    public IDatabase GetDb(int db)
    {
        return connectionMultiplexer.GetDatabase(db);
    }

    public ConnectionMultiplexer GetConnectionMultiplexer => connectionMultiplexer;

}
