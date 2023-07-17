using Application.Domain.Models;
using Application.DTOs.Project;
using Application.Helpers;
using Application.QueryParams.Project;
using Application.QueryParams.ProjectBonus;

namespace Application.Services
{
  public interface IProjectService
  {
    public Task<Guid> CreateProject(ProjectCreateDTO dto);
    public Task<bool> UpdateProjectAsAdmin(ProjectAdminUpdateDTO dto);
    public Task<bool> UpdateProjectAsPM(ProjectPMUpdateDTO dto, string requesterEmail);
    public Task<bool> UpdateProjectStatus(ProjectStatusUpdateDTO dto, string requesterEmail, bool isAdmin = false);
    public Task<PagedList<Project>> GetAll(ProjectQueryParams queryParams);
    public Task<PagedList<Project>> GetAllSelf(ProjectSelfQueryParams queryParams, string requesterEmail);
    public Task<Project?> GetProjectById(Guid projectId, string? requesterEmail = null, bool isAdmin = false);
    public Task<List<Wallet>> GetProjectWalletById(Guid projectId, string requesterEmail, bool isAdmin = false);
    public Task<bool> SendPointToProject(Guid projectId, ProjectSendPointDTO dto);
    public Task<Project?> UploadProjectFinalReport(Guid projectId, IFormFile file);
    public Task<PagedList<PayslipItem>> GetBonusFromProject(Guid projectId, ProjectBonusQueryParams queryParams, string requesterEmail);
    public Task<List<ProjectWithTotalFundDTO>> GetAllWithTransactions(ProjectMinimalQueryParams queryParams);
  }
}