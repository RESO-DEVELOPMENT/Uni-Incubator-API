using Application.Domain.Models;
using Application.DTOs.ProjetMilestone;
using Application.Helpers;
using Application.QueryParams.ProjectMilestone;

namespace Application.Services
{
  public interface IProjectMilestoneService
  {
    Task<ProjectMilestone> CreateProjectMilestone(Guid projectId, ProjectMilestoneCreateDTO dto, string requesterEmail);
    Task<PagedList<ProjectMilestone>> GetProjectMilestone(Guid projectId, ProjectUpdateQueryParams queryParams);
    Task<bool> UpdateProjectMilestone(ProjectMilestoneUpdateDTO dto , String requesterEmail);
    Task<bool> DeleteProjectMilestone(Guid projectMilestoneId, String requesterEmail);
  }
}