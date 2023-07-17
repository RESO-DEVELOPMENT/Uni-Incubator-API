using Application.Domain.Models;
using StackExchange.Redis;

namespace Application.Services
{
  public interface IQueueService
  {
    Task AddToQueue(string actionName, dynamic data);
    Task AddToQueue(QueueTask qt);
    ConnectionMultiplexer GetConnection();
    Task<QueueTask?> GetFromQueue();
  }
}