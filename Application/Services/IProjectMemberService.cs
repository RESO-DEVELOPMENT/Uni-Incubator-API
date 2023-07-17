using Application.Domain.Models;
using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectMemberRequest;
using Application.Helpers;
using Application.QueryParams.ProjectMemberRequest;

namespace Application.Services
{
    public interface IProjectMemberService
    {
        Task<List<ProjectMember>> GetProjectMembersDetailed(Guid projectId, string? requesterEmail = null, bool isAdmin = false);
        Task<ProjectMember> GetProjectMemberDetailed(Guid projectMemberId, string? requesterEmail = null, bool isAdmin = false);
        Task<PagedList<ProjectMemberRequest>> GetProjectMemberRequests(Guid projectId, ProjectMemberRequestQueryParams queryParams, string requesterEmail);
        Task<PagedList<ProjectMemberRequest>> GetSelfProjectMemberRequest(SelfProjectMemberRequestQueryParams queryParams, string requesterEmail);
        Task<Guid> RequestToJoin(Guid projectId, ProjectMemberRequestCreateDTO dto, string requesterEmail);
        Task<bool> ChangeProjectManager(Guid projectId, ProjectMemberChangePMDTO dto, string requesterEmail);
        Task<bool> UpdateProjectMemberStatus(ProjectMemberUpdateStatusDTO dto, string requesterEmail);
        Task<bool> ReviewRequestToJoin(Guid projectEmpRequestId, ProjectMemberRequestReviewDTO dto, string requesterEmail);
        Task<bool> UpdateProjectMember(ProjectMemberUpdateDTO dto, string requesterEmail);
    }
}