using Application.Domain.Enums.Payslip;
using Application.Domain.Enums.PayslipItem;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Models;
using Application.DTOs.Payslip;
using Application.DTOs.PayslipItems;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.Payslip;
using Application.QueryParams.PayslipItem;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class PayslipItemService : IPayslipItemService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;

        public PayslipItemService(UnitOfWork unitOfWork,
                           IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedList<PayslipItem>> GetPayslipItems(PayslipItemQueryParams queryParams, string requesterEmail, bool isAdmin = false)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail) ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            var query = _unitOfWork.PayslipItemRepository.GetQuery()
                .Include(p => p.Payslip.SalaryCycle)
                .AsQueryable();

            if (queryParams.MemberId != null)
            {
                if (member.MemberId != queryParams.MemberId && !isAdmin)
                {
                    throw new BadRequestException("Bạn không có quyền xem của thành viên khác!", ErrorNameValues.NoPermission);
                }
                query = query.Where(p => p.Payslip.MemberId == queryParams.MemberId);
            }
            else
            {
                if (!isAdmin)
                    query = query.Where(p => p.Payslip.MemberId == member.MemberId);
            }

            if (queryParams.SalaryCycleId != null)
            {
                query = query.Where(p => p.Payslip.SalaryCycleId == queryParams.SalaryCycleId);
            }

            if (queryParams.ProjectId != null)
            {
                if (!isAdmin)
                {
                    var pm = await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberActive(queryParams.ProjectId.Value, member.EmailAddress) ??
                              throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
                }

                query = query.Where(p => p.ProjectId == queryParams.ProjectId);
            }

            if (queryParams.Types.Any())
            {
                query = query.Where(p => queryParams.Types.Contains(p.Type));
            }

            return await PagedList<PayslipItem>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<PayslipItemsTotalDTO> GetPayslipItemsTotal(PayslipItemTotalQueryParams queryParams, string requesterEmail, bool isAdmin = false)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail) ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            var query = _unitOfWork.PayslipItemRepository.GetQuery()
                .Include(p => p.Payslip.SalaryCycle)
                .AsQueryable();

            if (queryParams.MemberId != null)
            {
                if (member.MemberId != queryParams.MemberId && !isAdmin)
                {
                    throw new BadRequestException("Bạn không có quyền xem của thành viên khác!", ErrorNameValues.NoPermission);
                }
                query = query.Where(p => p.Payslip.MemberId == queryParams.MemberId);
            }
            else
            {
                if (!isAdmin)
                    query = query.Where(p => p.Payslip.MemberId == member.MemberId);
            }

            if (queryParams.SalaryCycleId != null)
            {
                query = query.Where(p => p.Payslip.SalaryCycleId == queryParams.SalaryCycleId);
            }

            if (queryParams.ProjectId != null)
            {
                if (!isAdmin)
                {
                    var pm = await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberActive(queryParams.ProjectId.Value, member.EmailAddress) ??
                             throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
                }

                query = query.Where(p => p.ProjectId == queryParams.ProjectId);
            }

            if (queryParams.Types.Any())
            {
                query = query.Where(p => queryParams.Types.Contains(p.Type));
            }

            var payslipItemList = await query.ToListAsync();

            var totalPoint = payslipItemList.Where(x => x.Type != PayslipItemType.XP).Sum(x => x.Amount);
            var totalXP = payslipItemList.Where(x => x.Type == PayslipItemType.XP).Sum(x => x.Amount);

            var result = new PayslipItemsTotalDTO()
            {
                TotalPoint = totalPoint,
                TotalXP = totalXP,
            };

            return result;
        }
    }
}