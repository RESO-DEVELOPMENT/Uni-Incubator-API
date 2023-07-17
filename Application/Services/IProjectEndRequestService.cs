using Application.Domain.Models;
using Application.DTOs.ProjectEndRequest;
using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectMemberRequest;
using Application.Helpers;
using Application.QueryParams.ProjectEndRequest;
using Application.QueryParams.ProjectMemberRequest;

namespace Application.Services
{
    public interface IProjectEndRequestService
    {
        Task<List<ProjectEndRequestDTO>> GetAllRequestToEnd(ProjectEndRequestQueryParams queryParams, string? requesterEmail = null, bool isAdmin = false);

        Task<List<ProjectEndRequestDTO>> GetAllRequestToEndFromProject(Guid projectId,
            ProjectEndRequestQueryParams queryParams, string? requesterEmail = null, bool isAdmin = false);

        Task<ProjectEndRequestDTO> GetRequestToEnd(Guid requestId ,string? requesterEmail = null, bool isAdmin = false);

        Task<Guid> RequestToEnd(Guid projectId, ProjectEndRequestCreateDTO dto, string requesterEmail);
        Task<bool> ReviewRequestToEnd(ProjectEndRequestReviewDTO dto, string requesterEmail, bool isAdmin = false);
    }
}