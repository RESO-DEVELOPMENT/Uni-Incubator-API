using System.Text.Json;
using Application.Domain.Enums;
using Application.Domain.Enums.Notification;
using Application.Domain.Enums.Project;
using Application.DTOs.System;
using Application.Helpers;
using Application.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
  public class SystemService : ISystemService
  {
    private readonly UnitOfWork _unitOfWork;
    private readonly IQueueService _redisQueueService;

    public SystemService(UnitOfWork unitOfWork, IQueueService _redisQueueService)
    {
      _unitOfWork = unitOfWork;
      this._redisQueueService = _redisQueueService;
    }

    public async Task<SystemStatisticDTO> GetStatistic()
    {
      var dto = new SystemStatisticDTO();

      dto.Members.Total = await _unitOfWork.MemberRepository.GetQuery().CountAsync();
      dto.Members.Admin = await _unitOfWork.UserRepository.GetQuery().Where(u => u.RoleId == "ADMIN").CountAsync();

      dto.Projects.Total = await _unitOfWork.ProjectRepository.GetQuery().CountAsync();
      dto.Projects.Created = await _unitOfWork.ProjectRepository.GetQuery()
        .Where(p => p.ProjectStatus == ProjectStatus.Created).CountAsync();
      dto.Projects.Started = await _unitOfWork.ProjectRepository.GetQuery()
        .Where(p => p.ProjectStatus == ProjectStatus.Started).CountAsync();
      dto.Projects.Ended = await _unitOfWork.ProjectRepository.GetQuery()
        .Where(p => p.ProjectStatus == ProjectStatus.Ended).CountAsync();
      dto.Projects.Cancelled = await _unitOfWork.ProjectRepository.GetQuery()
        .Where(p => p.ProjectStatus == ProjectStatus.Cancelled).CountAsync();

      dto.Sponsors.Total = await _unitOfWork.SponsorRepository.GetQuery().CountAsync();

      return dto!;
    }


    public async Task<bool> SpamYourself(String email)
    {
      var member = await _unitOfWork.MemberRepository.GetByEmail(email);
      if (member == null) throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);

      var targetList = new Dictionary<String, String>();
      targetList.Add("Member", member.MemberId.ToString());

      await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
            {"MemberId", member.MemberId.ToString()},
             {"Type", NotificationType.Test.ToString()},
            {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
            {"Title", $"Test Notification"},
            {"Content", $"Test Notification"},
            {"SendNotification", "true"}
          });

      return true;
    }
  }
}