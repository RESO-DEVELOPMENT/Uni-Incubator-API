using Application.Domain.Enums;
using Application.Domain.Models;
using Application.Persistence.Repositories;

namespace Application.Services
{
    public class BackgroundHourTaskService : BackgroundService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IQueueService _redisService;
        private readonly ILogger<BackgroundHourTaskService> _logger;
        private Timer? _timer = null;

        public BackgroundHourTaskService(IServiceProvider services, ILogger<BackgroundHourTaskService> logger)
        {
            _logger = logger;
            var scope = services.CreateScope();

            _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
            _redisService = scope.ServiceProvider.GetRequiredService<IQueueService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            // When the timer should have no due-time, then do the work once now.
            await DoWork();

            using PeriodicTimer timer = new(TimeSpan.FromMinutes(15));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWork();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }

        private async Task DoWork()
        {
            _logger.LogInformation("=================================== Background Task Queue Start ===================================");

            _logger.LogInformation("[ Check Expired Wallet ] : ADDED");
            await _redisService.AddToQueue(new QueueTask() { TaskName = TaskName.CheckExpiredWallets });

            _logger.LogInformation("[ Check Expired Projects ] : ADDED");
            await _redisService.AddToQueue(new QueueTask() { TaskName = TaskName.CheckExpiredProject });

            _logger.LogInformation("[ Check Expired Voucher ] : ADDED");
            await _redisService.AddToQueue(new QueueTask() { TaskName = TaskName.CheckExpiredVoucher });

            _logger.LogInformation("[ Check Ended Project Wallet ] : ADDED");
            await _redisService.AddToQueue(new QueueTask() { TaskName = TaskName.CheckEndedProjectWallet });

            _logger.LogInformation("=================================== Background Task Queue Ended ===================================");
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _timer?.Dispose();
        }

    }
}