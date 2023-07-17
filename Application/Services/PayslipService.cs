using Application.Domain.Enums.Payslip;
using Application.Domain.Enums.PayslipItem;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Models;
using Application.DTOs.Payslip;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.Payslip;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class PayslipService : IPayslipService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;

        public PayslipService(UnitOfWork unitOfWork,
                           IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // For Get Self And Member
        public async Task<PagedList<Payslip>> GetPayslipsForMember(string memberEmail, MemberPayslipQueryParams queryParams)
        {
            var member = await _unitOfWork.MemberRepository.GetQuery()
              .Where(e => e.EmailAddress == memberEmail)
              .FirstOrDefaultAsync();
            if (member == null) throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            return await GetPayslipsForMember(member.MemberId, queryParams);
        }

        public async Task<PayslipsTotalDTO> GetPayslipsForMemberTotal(string memberEmail, MemberPayslipTotalQueryParams queryParams)
        {
            var member = await _unitOfWork.MemberRepository.GetQuery()
              .Where(e => e.EmailAddress == memberEmail)
              .FirstOrDefaultAsync();
            if (member == null) throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            return await GetPayslipsForMemberTotal(member.MemberId, queryParams);
        }

        public async Task<PagedList<Payslip>> GetPayslipsForMember(Guid memberId, MemberPayslipQueryParams queryParams)
        {
            var query = _unitOfWork.PayslipRepository
              .GetPaySlipQueryFullForProjectOrNull(queryParams.ProjectId);

            query = query.Where(p => p.MemberId == memberId);


            if (queryParams.From != null) query = query.Where(p => p.CreatedAt >= queryParams.From);
            if (queryParams.To != null) query = query.Where(p => p.CreatedAt <= queryParams.To);
            if (queryParams.SalaryCycleId != null) query = query.Where(p => p.SalaryCycle.SalaryCycleId == queryParams.SalaryCycleId);

            switch (queryParams.OrderBy)
            {
                case PayslipOrderBy.DateAsc:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case PayslipOrderBy.DateDesc:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            if (queryParams.Status.Any()) query = query.Where(p => queryParams.Status.Contains(p.Status));

            var payslips = await PagedList<Payslip>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return payslips;
        }

        public async Task<PayslipsTotalDTO> GetPayslipsForMemberTotal(Guid memberId, MemberPayslipTotalQueryParams queryParams)
        {
            var query = _unitOfWork.PayslipRepository
                .GetPaySlipQueryFullForProjectOrNull(queryParams.ProjectId);

            query = query.Where(p => p.MemberId == memberId);

            if (queryParams.From != null) query = query.Where(p => p.CreatedAt >= queryParams.From);
            if (queryParams.To != null) query = query.Where(p => p.CreatedAt <= queryParams.To);
            if (queryParams.SalaryCycleId != null) query = query.Where(p => p.SalaryCycle.SalaryCycleId == queryParams.SalaryCycleId);

            if (queryParams.Status.Any()) query = query.Where(p => queryParams.Status.Contains(p.Status));

            var payslips = await query.ToListAsync();

            var p1 = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.P1).Sum(psi => psi.Amount));
            var p2 = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.P2).Sum(psi => psi.Amount));
            var p3 = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.P3).Sum(psi => psi.Amount));
            var xp = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.XP).Sum(psi => psi.Amount));
            var bonus = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.Bonus).Sum(psi => psi.Amount));

            var result = new PayslipsTotalDTO()
            {
                TotalP1 = p1,
                TotalP2 = p2,
                TotalP3 = p3,
                TotalBonus = bonus,
                TotalXP = xp,
                PayslipsCount = payslips.Count
            };

            return result;
        }

        // For Get ALl
        public async Task<PayslipsTotalDTO> GetPayslipsTotal(PayslipTotalQueryParams queryParams)
        {
            var query = _unitOfWork.PayslipRepository
              .GetQuery();

            query = query
              .Include(p => p.Member)
              .Include(p => p.PayslipItems)
              .Where(p => p.Status == PayslipStatus.Paid);

            if (queryParams.From != null) query = query.Where(p => p.CreatedAt >= queryParams.From);
            if (queryParams.To != null) query = query.Where(p => p.CreatedAt <= queryParams.To);
            if (queryParams.ProjectId != null)
            {
                var project = await _unitOfWork.ProjectRepository
                  .GetQuery()
                  .Include(p => p.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                  .Where(p => p.ProjectId == queryParams.ProjectId)
                  .FirstOrDefaultAsync();
                if (project == null) throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);
                query = query.Where(p => project.ProjectMember.Select(s => s.MemberId).Contains(p.MemberId));
            }

            if (queryParams.SalaryCycleId != null)
            {
                query = query.Where(p => queryParams.SalaryCycleId == p.SalaryCycleId);
            }

            if (queryParams.MemberId != null)
            {
                var member = await _unitOfWork.MemberRepository.GetQuery()
                .Where(s => s.MemberId == queryParams.MemberId).FirstOrDefaultAsync();
                if (member == null) throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);
                query = query.Where(p => member.MemberId == queryParams.MemberId);
            }

            if (queryParams.Status.Any()) query = query.Where(p => queryParams.Status.Contains(p.Status));

            var payslips = await query.ToListAsync();

            var p1 = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.P1).Sum(psi => psi.Amount));
            var p2 = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.P2).Sum(psi => psi.Amount));
            var p3 = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.P3).Sum(psi => psi.Amount));
            var xp = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.XP).Sum(psi => psi.Amount));
            var bonus = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.Bonus).Sum(psi => psi.Amount));

            var result = new PayslipsTotalDTO()
            {
                TotalP1 = p1,
                TotalP2 = p2,
                TotalP3 = p3,
                TotalBonus = bonus,
                TotalXP = xp,
                PayslipsCount = payslips.Count
            };

            return result;
        }

        public async Task<PagedList<Payslip>> GetPayslips(PayslipQueryParams queryParams)
        {
            if (queryParams.ProjectId != null)
            {
                var project = await _unitOfWork.ProjectRepository
                  .GetQuery()
                  .Include(p => p.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                  .Where(p => p.ProjectId == queryParams.ProjectId)
                  .FirstOrDefaultAsync();
                if (project == null) throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);
            }

            IQueryable<Payslip> query = _unitOfWork.PayslipRepository.GetPaySlipQueryFullForProjectOrNull(queryParams.ProjectId);

            if (queryParams.SalaryCycleId != null)
            {
                query = query.Where(p => queryParams.SalaryCycleId == p.SalaryCycleId);
            }

            if (queryParams.MemberId != null)
            {
                var member = await _unitOfWork.MemberRepository.GetQuery()
                .Where(s => s.MemberId == queryParams.MemberId).FirstOrDefaultAsync();
                if (member == null) throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);
                query = query.Where(p => member.MemberId == queryParams.MemberId);
            }
            if (queryParams.From != null) query = query.Where(p => p.CreatedAt >= queryParams.From);
            if (queryParams.To != null) query = query.Where(p => p.CreatedAt <= queryParams.To);

            switch (queryParams.OrderBy)
            {
                case PayslipOrderBy.DateAsc:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case PayslipOrderBy.DateDesc:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            if (queryParams.Status.Any()) query = query.Where(p => queryParams.Status.Contains(p.Status));

            var payslips = await PagedList<Payslip>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return payslips;
        }

        // For Get Project
        public async Task<PayslipsTotalDTO> GetPayslipsTotalForProject(Guid projectId, ProjectPayslipTotalQueryParams queryParams, String requesterEmail, bool isAdmin = false)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail);
            if (member == null) throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);


            var projectMember = await _unitOfWork.ProjectMemberRepository.GetQuery()
            .Where(p => p.ProjectId == projectId &&
              p.Status == ProjectMemberStatus.Active).ToListAsync();

            if (!isAdmin)
            {
                var curProjectMember = projectMember.FirstOrDefault(p => p.MemberId == member.MemberId);
                if (curProjectMember is not { Role: ProjectMemberRole.Manager })
                    throw new BadRequestException("Bạn không phải là quản lý dự án!", ErrorNameValues.NoPermission);
            }

            var query = _unitOfWork.PayslipRepository.GetPaySlipQueryFullForProject(projectId);

            //query = query.Where(p => projectMember.Select(s => s.MemberId).Contains(p.MemberId))

            if (queryParams.From != null) query = query.Where(p => p.CreatedAt >= queryParams.From);
            if (queryParams.To != null) query = query.Where(p => p.CreatedAt <= queryParams.To);

            if (queryParams.SalaryCycleId != null)
            {
                query = query.Where(p => queryParams.SalaryCycleId == p.SalaryCycleId);
            }

            if (queryParams.Status.Any()) query = query.Where(p => queryParams.Status.Contains(p.Status));

            var payslips = await query.ToListAsync();

            var p1 = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.P1).Sum(psi => psi.Amount));
            var p2 = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.P2).Sum(psi => psi.Amount));
            var p3 = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.P3).Sum(psi => psi.Amount));
            var xp = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.XP).Sum(psi => psi.Amount));
            var bonus = payslips.Sum(p => p.PayslipItems.Where(p => p.Type == PayslipItemType.Bonus).Sum(psi => psi.Amount));

            var result = new PayslipsTotalDTO()
            {
                TotalP1 = p1,
                TotalP2 = p2,
                TotalP3 = p3,
                TotalXP = xp,
                TotalBonus = bonus,
                PayslipsCount = payslips.Count
            };

            return result;
        }

        public async Task<PagedList<Payslip>> GetPayslipsForProject(Guid projectId, ProjectPayslipQueryParams queryParams, String requesterEmail, bool isAdmin = false)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail);
            if (member == null) throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            var projectMember = await _unitOfWork.ProjectMemberRepository.GetQuery()
            .Where(p => p.ProjectId == projectId &&
              p.Status == ProjectMemberStatus.Active).ToListAsync();

            if (!isAdmin)
            {
                var curProjectMember = projectMember.FirstOrDefault(p => p.MemberId == member.MemberId);
                if (curProjectMember is not { Role: ProjectMemberRole.Manager })
                    throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
            }

            var query = _unitOfWork.PayslipRepository.GetPaySlipQueryFullForProject(projectId);

            //query = query.Where(p => projectMember.Select(s => s.MemberId).Contains(p.MemberId));

            if (queryParams.SalaryCycleId != null)
            {
                query = query.Where(p => queryParams.SalaryCycleId == p.SalaryCycleId);
            }

            if (queryParams.From != null) query = query.Where(p => p.CreatedAt >= queryParams.From);
            if (queryParams.To != null) query = query.Where(p => p.CreatedAt <= queryParams.To);

            switch (queryParams.OrderBy)
            {
                case PayslipOrderBy.DateAsc:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case PayslipOrderBy.DateDesc:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            if (queryParams.Status.Any()) query = query.Where(p => queryParams.Status.Contains(p.Status));

            var payslips = await PagedList<Payslip>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return payslips;
        }

        public async Task<Payslip> GetPayslipsById(Guid payslipId)
        {
            var payslip = await _unitOfWork.PayslipRepository.GetPaySlipQueryFull()
                    .Where(p => p.PayslipId == payslipId)
                    .FirstOrDefaultAsync();

            if (payslip == null) throw new NotFoundException("Phiếu lương không tồn tại!", "PAYSLIP_NOT_FOUND");
            return payslip;
        }
    }
}