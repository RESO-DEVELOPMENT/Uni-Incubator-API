using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.ProjectReportMemberTask;
using Application.Helpers;
using Application.QueryParams.Member;

namespace Application.Services
{
  public interface IProjectReportMemberTasksService
  {
    Task<List<ProjectReportMemberTaskForPayslipDTO>> GetMemberTasks(Guid memberId, MemberTasksQueryParams queryParams);
    Task<List<ProjectReportMemberTaskForPayslipDTO>> GetMemberTasks(String memberEmail, MemberTasksQueryParams queryParams);
  }
}