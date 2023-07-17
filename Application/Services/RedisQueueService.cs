using StackExchange.Redis;
using System.Text.Json;
using Application.Domain.Models;

namespace Application.Services
{
  public class RedisQueueService : IQueueService
  {
    private ConnectionMultiplexer _connection { get; }
    string queueName = "";
    string eventName = "";

    public RedisQueueService(IConfiguration configuration)
    {
      ConfigurationOptions options = new ConfigurationOptions()
      {
        EndPoints = { { configuration.GetValue<string>("redis:endpoint:host"), 11738 } },
        AllowAdmin = true,
        // User = configuration.GetValue<String>("redis:username"),
        Password = configuration.GetValue<string>("redis:password"),
      };

      _connection = ConnectionMultiplexer.Connect(options);

      queueName = configuration.GetValue<string>("redis:queueName");
      eventName = configuration.GetValue<string>("redis:eventName");
    }

    public ConnectionMultiplexer GetConnection() => _connection;

    public async Task AddToQueue(string actionName, dynamic data)
    {
      await AddToQueue(new QueueTask() { TaskName = actionName, TaskData = data });
    }

    public async Task AddToQueue(QueueTask qt)
    {
      var db = await _connection.GetDatabase().ListRightPushAsync(queueName, JsonSerializer.Serialize(qt));
      _connection.GetSubscriber().Publish(eventName, "");
    }

    public async Task<QueueTask?> GetFromQueue()
    {
      var res = await _connection.GetDatabase().ListLeftPopAsync(queueName);
      if (res.IsNull) return null;

      var qi = JsonSerializer.Deserialize<QueueTask>(res);
      return qi;
    }
  }
}