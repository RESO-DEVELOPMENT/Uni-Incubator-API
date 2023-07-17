using Application.Domain.Enums;
using Application.Domain.Models;
using Application.Persistence.Repositories;

namespace Application.Services
{
  public class BackgroundStartupTask : BackgroundService
  {
    private readonly ILogger<BackgroundStartupTask> _logger;

    GlobalVar gv { get; set; }
    IBoxService bs { get; set; }

    public BackgroundStartupTask(IServiceProvider services, ILogger<BackgroundStartupTask> logger)
    {
      var serviceScope = services.CreateScope();
      var sp = serviceScope.ServiceProvider;

      bs = sp.GetService<IBoxService>()!;
      gv = sp.GetService<GlobalVar>()!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      await gv.LoadConfig();
      await bs.GetClient();
    }
  }
}