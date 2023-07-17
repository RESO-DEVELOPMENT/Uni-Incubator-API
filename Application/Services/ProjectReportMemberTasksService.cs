using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Domain.Enums.ProjectReport;
using Application.Domain.Models;
using Application.DTOs.ProjectReportMemberTask;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.Member;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ProjectReportMemberTasksService : IProjectReportMemberTasksService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectReportMemberTasksService(UnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<ProjectReportMemberTaskForPayslipDTO>> GetMemberTasks(string memberEmail, MemberTasksQueryParams queryParams)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmail(memberEmail) ?? throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);

            
            return await GetMemberTasks(member.MemberId, queryParams);
        }


        public async Task<List<ProjectReportMemberTaskForPayslipDTO>> GetMemberTasks(Guid memberId, MemberTasksQueryParams queryParams)
        {
            var member = await _unitOfWork.MemberRepository.GetByID(memberId) ?? throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);

            var query = _unitOfWork.ProjectReportMemberTaskRepository.GetQuery();
            if (queryParams.SalaryCycleId != null)
                query = query.Where(prmt => prmt.ProjectReportMember.ProjectReport.SalaryCycleId == queryParams.SalaryCycleId);

            query = query
                .Include(prmt => prmt.ProjectReportMember.ProjectMember.Project)
                .Where(prmt =>
                    prmt.ProjectReportMember.ProjectMember.MemberId == member.MemberId &&
                    prmt.ProjectReportMember.ProjectReport.Status == ProjectReportStatus.Processed
                );

            var tasks = await PagedList<ProjectReportMemberTask>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            var mappedTasks = _mapper.Map<List<ProjectReportMemberTaskForPayslipDTO>>(tasks);

            if (_httpContextAccessor.HttpContext != null)
                Pagination.AddPaginationHeader<ProjectReportMemberTask>(_httpContextAccessor.HttpContext.Response, tasks);

            return mappedTasks;
        }
    }
}