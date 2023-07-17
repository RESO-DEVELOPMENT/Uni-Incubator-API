using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Text.Json;
using Application.Domain;
using Application.Domain.Enums;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.ProjectSponsor;
using Application.Domain.Enums.ProjectSponsorTransaction;
using Application.Domain.Enums.Sponsor;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.ProjectSponsor;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.ProjectSponsor;
using Application.QueryParams.ProjectSponsorTransaction;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ProjectSponsorService : IProjectSponsorService
    {
        private readonly IMapper _mapper;
        private readonly IQueueService _redisQueueService;
        private readonly IBoxService _IBoxService;
        private readonly UnitOfWork _unitOfWork;

        public ProjectSponsorService(UnitOfWork unitOfWork,
                           IMapper mapper,
                           IQueueService redisQueueService,
                           IBoxService IBoxService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _redisQueueService = redisQueueService;
            _IBoxService = IBoxService;
        }

        /// <summary>
        /// Get all sponsors
        /// </summary>
        public async Task<PagedList<ProjectSponsor>> GetAllSponsorsInProject(Guid projectId, ProjectSponsorQueryParams queryParams, string requesterEmail, bool isAdmin = false)
        {
            var project = await _unitOfWork.ProjectRepository.GetByID(projectId);
            if (project == null) throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail);
            if (member == null) throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);


            if (!isAdmin)
            {
                var projectManager = await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(projectId, requesterEmail);

                if (projectManager == null)
                    throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
            }

            var query = _unitOfWork.ProjectSponsorRepository.GetQuery()
              .Include(ps => ps.Sponsor)
              .Include(ps => ps.ProjectSponsorTransactions.Where(pst => pst.Status == ProjectSponsorTransactionStatus.Paid)
                .OrderByDescending(pt => pt.CreatedAt))
              .Where(ps => ps.ProjectId == projectId);

            if (queryParams.SponsorName != null)
                query = query.Where(s => s.Sponsor.SponsorName.ToLower().Contains(queryParams.SponsorName.ToLower()));

            if (queryParams.Status.Any())
                query = query.Where(s => queryParams.Status.Contains(s.Status));

            switch (queryParams.OrderBy)
            {
                case ProjectSponsorOrderBy.DateAsc:
                    {
                        query = query.OrderBy(q => q.CreatedAt);
                        break;
                    }

                case ProjectSponsorOrderBy.DateDesc:
                    {
                        query = query.OrderByDescending(q => q.CreatedAt);
                        break;
                    }
            };
            return await PagedList<ProjectSponsor>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<ProjectSponsor> AddSponsorToProject(Guid projectId, ProjectSponsorCreateDTO dto)
        {
            var projectSponsor = await _unitOfWork.ProjectSponsorRepository.GetQuery()
              .Where(s => s.ProjectId == projectId
              && s.SponsorId == dto.SponsorId
              && s.Status == ProjectSponsorStatus.Available)
              .FirstOrDefaultAsync();

            if (projectSponsor != null) throw new BadRequestException("Nhà tài trọ dẵ nằm trong dự án rồi!", ErrorNameValues.SponsorExisted);

            var sponsor = await _unitOfWork.SponsorRepository.GetQuery().Where(s => s.SponsorId == dto.SponsorId).FirstOrDefaultAsync() ?? throw new NotFoundException("Nhà tài trợ không tồn tại!", ErrorNameValues.SponsorNotFound);

            if (sponsor.SponsorStatus != SponsorStatus.Active) throw new BadRequestException("Nhà tại trợ đã bị vô hiệu hoá!", ErrorNameValues.SponsorExisted);
            projectSponsor = new ProjectSponsor();

            projectSponsor.ProjectId = projectId;
            projectSponsor.Sponsor = sponsor;

            // _mapper.Map(dto, projectSponsor);

            _unitOfWork.ProjectSponsorRepository.Add(projectSponsor);

            var result = await _unitOfWork.SaveAsync();

            if (!result) throw new BadRequestException("Some error happen, try again!", ErrorNameValues.SystemError);
            return projectSponsor;
        }

        public async Task<ProjectSponsor> UpdateSponsorStatusInProject(ProjectSponsorUpdateDTO dto)
        {
            var projectSponsor = await _unitOfWork.ProjectSponsorRepository.GetQuery()
              .Where(s => s.ProjectSponsorId == dto.ProjectSponsorId
              && s.Status == ProjectSponsorStatus.Available)
              .Include(ps => ps.Sponsor)
              .FirstOrDefaultAsync();

            if (projectSponsor == null)
                throw new NotFoundException("Nhà tài trợ không nằm trong dự án!", ErrorNameValues.SponsorExisted);

            // if (dto.Status != null && dto.Status != ProjectSponsorStatus.Unavailable)
            // {
            //   throw new BadRequestException("You can only cancel the sponsor!", ErrorNameValues.InvalidStateChange);
            // }

            _mapper.Map(dto, projectSponsor);
            projectSponsor.UpdatedAt = DateTimeHelper.Now();
            _unitOfWork.ProjectSponsorRepository.Update(projectSponsor);

            var result = await _unitOfWork.SaveAsync();

            if (!result) throw new BadRequestException("Some error happen, try again!", ErrorNameValues.SystemError);
            return projectSponsor;
        }

        public async Task<bool> ProjectSponsorDeposit(ProjectSponsorDepositDTO dto)
        {
            var projectSponsor = await _unitOfWork.ProjectSponsorRepository.GetQuery()
              .Where(s => s.ProjectSponsorId == dto.ProjectSponsorId
              && s.Status == ProjectSponsorStatus.Available)
              .Include(ps => ps.Sponsor)
              .FirstOrDefaultAsync() ?? throw new BadRequestException("Sponsor is not in the project!", ErrorNameValues.SponsorNotFound);

            // var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository.GetQuery().OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            // if (currentSalaryCycle != null && currentSalaryCycle.SalaryCycleStatus <= SalaryCycleStatus.Review)
            // {
            await _redisQueueService.AddToQueue(TaskName.SendPoint, new Dictionary<string, string>()
      {
        {"FromId", Guid.Empty.ToString()},
        {"ToId", projectSponsor.ProjectId.ToString()},
        {"TransactionType", TransactionType.SponsorDepositToProject.ToString()},
        {"Amount", dto.Amount.ToString()},
        {"ToTag", JsonSerializer.Serialize(new List<String>() {"bonus"})},

        {"ProjectSponsorId", projectSponsor.ProjectSponsorId.ToString()},
      });
            // }
            // else
            // {
            //   _unitOfWork.ProjectSponsorTransactionRepository.Add(new ProjectSponsorTransaction()
            //   {
            //     ProjectSponsorId = projectSponsor.ProjectSponsorId,
            //     Amount = dto.Amount,
            //     PaidAt = DateTimeHelper.Now(),
            //     Type = ProjectSponsorTransactionType.Deposit,
            //     Status = ProjectSponsorTransactionStatus.Paid,
            //   });

            //   await _unitOfWork.SaveAsync();
            // }

            return true;
        }

        public async Task<PagedList<ProjectSponsorTransaction>> GetProjectSponsorTransaction(Guid projectSponsorId, ProjectSponsorTransactionsQueryParams queryParams)
        {
            var projectSponsorCount = await _unitOfWork.ProjectSponsorRepository.GetQuery()
              .Where(s => s.ProjectSponsorId == projectSponsorId
              && s.Status == ProjectSponsorStatus.Available)
              .Include(ps => ps.ProjectSponsorTransactions.Where(pst => pst.Status == ProjectSponsorTransactionStatus.Paid)
                .OrderByDescending(pt => pt.CreatedAt).Take(1))
              .Include(ps => ps.Sponsor).CountAsync();

            if (projectSponsorCount <= 0)
            {
                throw new BadRequestException("Sponsor is not in the project!", ErrorNameValues.SponsorNotFound);
            }

            // var projectSponsorTrxs = await _unitOfWork.ProjectSponsorTransactionRepository.GetQuery()
            //   .Where(s => s.ProjectSponsorId == projectSponsorId)
            //   .ToListAsync();

            var query = _unitOfWork.ProjectSponsorTransactionRepository.GetQuery();
            query = query
            .Where(s => s.ProjectSponsorId == projectSponsorId);
            // .Include(ps => ps.ProjectSponsor)
            // .ThenInclude(ps => ps.Sponsor);


            if (queryParams.Status.Count() > 0)
                query = query.Where(s => queryParams.Status.Contains(s.Status));

            switch (queryParams.OrderBy)
            {
                case ProjectSponsorTransactionOrderBy.DateAsc:
                    {
                        query = query.OrderBy(q => q.CreatedAt);
                        break;
                    }

                case ProjectSponsorTransactionOrderBy.DateDesc:
                    {
                        query = query.OrderByDescending(q => q.CreatedAt);
                        break;
                    }
            };
            return await PagedList<ProjectSponsorTransaction>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<PagedList<ProjectSponsorTransaction>> GetAllSponsorTransactions(ProjectSponsorTransactionsQueryParams queryParams)
        {
            var query = _unitOfWork.ProjectSponsorTransactionRepository.GetQuery();
            query = query
              .Include(ps => ps.ProjectSponsor)
              .ThenInclude(ps => ps.Sponsor);


            if (queryParams.Status.Count() > 0)
                query = query.Where(s => queryParams.Status.Contains(s.Status));

            switch (queryParams.OrderBy)
            {
                case ProjectSponsorTransactionOrderBy.DateAsc:
                    {
                        query = query.OrderBy(q => q.CreatedAt);
                        break;
                    }

                case ProjectSponsorTransactionOrderBy.DateDesc:
                    {
                        query = query.OrderByDescending(q => q.CreatedAt);
                        break;
                    }
            };
            return await PagedList<ProjectSponsorTransaction>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }
    }
}