using System.Text.Json;
using Application.Domain.Constants;
using Application.Domain.Enums;
using Application.Domain.Enums.Notification;
using Application.Domain.Enums.PayslipItem;
using Application.Domain.Enums.Project;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.SalaryCycle;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.SalaryCycle;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.SalaryCycle;
using Application.StateMachines;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Attribute = Application.Domain.Models.Attribute;

namespace Application.Services
{
    public class SalaryCycleService : ISalaryCycleService
    {
        private readonly IMapper _mapper;
        private readonly IWalletService _walletService;
        private readonly IQueueService _redisQueueService;
        private readonly UnitOfWork _unitOfWork;

        public SalaryCycleService(UnitOfWork unitOfWork,
                           IMapper mapper,
                           IWalletService walletService,
                           IQueueService redisQueueService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _walletService = walletService;
            _redisQueueService = redisQueueService;
        }

        public async Task<PagedList<SalaryCycle>> GetAllSalaryCycle(SalaryCycleQueryParams queryParams)
        {
            var query = _unitOfWork.SalaryCycleRepository.GetQuery();

            if (queryParams.StartedAt != null)
            {
                query = query.Where(sc => sc.CreatedAt.Date >= queryParams.StartedAt.Value.Date);
            }

            if (queryParams.EndedBefore != null)
            {
                query = query.Where(sc => sc.EndedAt != null && sc.EndedAt <= queryParams.EndedBefore);
            }

            switch (queryParams.OrderBy)
            {
                case SalaryCycleOrderBy.DateAsc:
                    query = query.OrderBy(l => l.SalaryCycleStatus).ThenBy(l => l.CreatedAt);
                    break;
                case SalaryCycleOrderBy.DateDesc:
                    query = query.OrderByDescending(l => l.SalaryCycleStatus).ThenBy(l => l.CreatedAt);
                    break;
                default:
                    break;
            }

            return await PagedList<SalaryCycle>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<SalaryCycle> GetSalaryCycleById(Guid scId)
        {
            var scMinimal = await _unitOfWork.SalaryCycleRepository
              .GetQuery()
              .FirstOrDefaultAsync(sc => sc.SalaryCycleId == scId) ?? throw new NotFoundException("Salary Cycle Not Found!", ErrorNameValues.SalaryCycleNotFound);

            var salaryCycleFull = await _unitOfWork.SalaryCycleRepository.GetQuery()
            .Include(sc => sc.Payslips)
              .ThenInclude(p => p.PayslipItems)
                  .ThenInclude(x => x.PayslipItemAttributes)
                      .ThenInclude(x => x.Attribute)
            .Include(sc => sc.Payslips)
              .ThenInclude(p => p.Member)
            .Where(sc => sc.SalaryCycleId == scId)
            .FirstAsync();

            return salaryCycleFull;
        }

        public async Task<bool> UpdateSalaryCycleStatus(SalaryCycleUpdateDTO dto, string requesterEmail)
        {
            var requesterMember = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail) ?? throw new BadRequestException("Member not found!");

            var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository
              .GetQuery()
              .FirstOrDefaultAsync(sc => sc.SalaryCycleId == dto.SalaryCycleId);

            if (currentSalaryCycle == null)
                throw new NotFoundException("Salary Cycle Not Found!", ErrorNameValues.SalaryCycleNotFound);

            var oldStatus = currentSalaryCycle.SalaryCycleStatus;

            try
            {
                SalaryCycleStateMachine scStateMachine = new SalaryCycleStateMachine(currentSalaryCycle);
                scStateMachine.TriggerState(dto.Status);

                await _redisQueueService.AddToQueue(TaskName.UpdateProjectSalaryCycleStatus, new Dictionary<string, string>() {
                  {"SalaryCycleId",currentSalaryCycle.SalaryCycleId.ToString()},
                  {"SalaryCycleNewStatus", dto.Status.ToString()},
                  {"SalaryCycleOldStatus", oldStatus.ToString()},
                  {"MemberId", requesterMember.MemberId.ToString() }
                });

                currentSalaryCycle.SalaryCycleStatus = dto.Status;
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                throw new BadRequestException("Invalid status change!", ErrorNameValues.InvalidStateChange);
            }

            return true;

        }

        public async Task<SalaryCycle> CreateSalaryCycle(SalaryCycleCreateDTO dto)
        {
            var currentStartedCycle = await _unitOfWork.SalaryCycleRepository.GetQuery()
              .FirstOrDefaultAsync(sc => sc.SalaryCycleStatus < SalaryCycleStatus.Paid);

            if (currentStartedCycle != null)
                throw new BadRequestException("There is already a salary cycle ongoing!", ErrorNameValues.SalaryCycleExist);

            // var memberIds = await _unitOfWork.MemberRepository.GetQuery()
            //   .Where(m => m.MemberStatus == MemberStatus.Available)
            //   .Select(s => s.MemberId)
            //   .ToListAsync();

            var memberInProjects = await _unitOfWork.ProjectMemberRepository
            .GetQuery()
              .Include(pm => pm.Project)
              .Include(pm => pm.Member.MemberLevels.Where(ml => ml.IsActive))
                .ThenInclude(ml => ml.Level)
              .Where(m =>
                m.Status == ProjectMemberStatus.Active &&
                (m.Project.ProjectStatus == ProjectStatus.Started || m.Project.ProjectStatus == ProjectStatus.Stopped ))
              .GroupBy(pm => pm.MemberId)
              .ToListAsync();

            var newSalaryCycle = new SalaryCycle
            {
                StartedAt = dto.StartedAt,
                Name = dto.Name
            };

            _unitOfWork.SalaryCycleRepository.Add(newSalaryCycle);

            memberInProjects.ForEach(projectMember =>
            {
                var member = projectMember.First().Member;
                var level = member.MemberLevels.First().Level;

                var attrs = new List<PayslipAttribute>() {
                  new()
                  {
                    Attribute = new Attribute()
                    {
                      AttributeGroupId = AttributeGroupNameValues.BasePoint,
                      Value = level.BasePoint.ToString()
                    }
                  },
                  new()
                  {
                    Attribute = new Attribute()
                    {
                      AttributeGroupId = AttributeGroupNameValues.PointPerHour,
                      Value = level.BasePointPerHour.ToString()
                    }
                  }
                };

                var payslip = new Payslip()
                {
                    MemberId = member.MemberId,
                    PayslipAttributes = attrs,
                    Note = $"Phiếu lương cho kỳ lương <@SalaryCycle:{newSalaryCycle.SalaryCycleId}>",
                    SalaryCycleId = newSalaryCycle.SalaryCycleId
                };

                _unitOfWork.PayslipRepository.Add(payslip);
            });

            var result = await _unitOfWork.SaveAsync();

            if (result)
            {
                await _redisQueueService.AddToQueue(TaskName.ProcessSalaryCycleCreate, new Dictionary<string, string>() {
                        {"SalaryCycleId",newSalaryCycle.SalaryCycleId.ToString()
                        }}
                );

                if (dto.SendNotification)
                {
                    var targetList = new Dictionary<string, string> { { "SalaryCycle", newSalaryCycle.SalaryCycleId.ToString() } };

                    memberInProjects.ForEach(async memberId =>
                    {
                        await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                            {"MemberId", memberId.ToString()},
                            {"Type", NotificationType.SalaryCycleStarted.ToString()},
                            {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                            {"Content", $"Kì lương mới đã bắt đầu!"},
                            {"Title", $"Kì lương mới đã bắt đầu!"}
                        });
                    });
                }
            }
            return newSalaryCycle;
        }

        public async Task<PagedList<MemberLevel>> GetAllMemberLevelUpInSalaryCycle(Guid salaryCycleId, SalaryCycleMemberLevelUpQueryParams queryParams)
        {
            var sc = await _unitOfWork.SalaryCycleRepository.GetQuery()
            .OrderByDescending(sc => sc.CreatedAt)
            .FirstOrDefaultAsync(sc => sc.SalaryCycleId == salaryCycleId);

            if (sc == null) throw new NotFoundException("Salary cycle not not found!", ErrorNameValues.SalaryCycleNotFound);
            if (sc.SalaryCycleStatus != SalaryCycleStatus.Paid)
            {
                throw new BadRequestException("Salary cycle is not paid!", ErrorNameValues.SalaryCycleNotAvailable);
            }

            var query = _unitOfWork.MemberLevelRepository.GetQuery()
              .Include(ml => ml.Level)
              .Include(ml => ml.Member)
              .Where(ml => 
                ml.CreatedAt >= sc.CreatedAt && 
                ml.CreatedAt <= sc.EndedAt.Value.AddMinutes(5) && 
                ml.IsActive);


            if (!queryParams.IncludeNewAccount)
            {
                var baseLevel = _unitOfWork.LevelRepository.GetQuery().OrderBy(x => x.XPNeeded).First();
                query = query.Where(ml => ml.LevelId != baseLevel.LevelId);
            }

            switch (queryParams.OrderBy)
            {
                case SalaryCycleMemberLevelUpOrderBy.DateAsc:
                    query = query.OrderBy(l => l.CreatedAt);
                    break;
                case SalaryCycleMemberLevelUpOrderBy.DateDesc:
                    query = query.OrderByDescending(l => l.CreatedAt);
                    break;
                default:
                    break;
            }

            return await PagedList<MemberLevel>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<PagedList<SalaryCycle>> GetAllSalaryCycleOfMember(string memberEmail, SalaryCycleQueryParams queryParams)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmail(memberEmail);
            if (member == null) throw new NotFoundException();

            var query = _unitOfWork.SalaryCycleRepository.GetQuery()
              .Include(sc => sc.Payslips)
              .Where(sc => sc.Payslips.Any(m => m.MemberId == member.MemberId));

            if (queryParams.StartedAt != null)
            {
                query = query.Where(sc => sc.CreatedAt.Date >= queryParams.StartedAt.Value.Date);
            }

            if (queryParams.StartedBefore != null)
            {
                query = query.Where(sc => sc.CreatedAt.Date <= queryParams.StartedBefore.Value.Date);
            }


            if (queryParams.EndedAfter != null)
            {
                query = query.Where(sc => sc.EndedAt != null &&
                                          sc.EndedAt.Value.Date >= queryParams.EndedAfter.Value.Date);
            }

            if (queryParams.EndedBefore != null)
            {
                query = query.Where(sc => sc.EndedAt != null &&
                  sc.EndedAt.Value.Date <= queryParams.EndedBefore.Value.Date);
            }

            switch (queryParams.OrderBy)
            {
                case SalaryCycleOrderBy.DateAsc:
                    query = query.OrderBy(l => l.CreatedAt);
                    break;
                case SalaryCycleOrderBy.DateDesc:
                    query = query.OrderByDescending(l => l.CreatedAt);
                    break;
                default:
                    break;
            }
            return await PagedList<SalaryCycle>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }
    }
}