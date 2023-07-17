using Application.Domain.Models;
using Application.DTOs.ProjectSponsor;
using Application.Helpers;
using Application.QueryParams.ProjectSponsor;
using Application.QueryParams.ProjectSponsorTransaction;

namespace Application.Services
{
  public interface IProjectSponsorService
  {
    Task<ProjectSponsor> AddSponsorToProject(Guid projectId, ProjectSponsorCreateDTO dto);
    Task<PagedList<ProjectSponsor>> GetAllSponsorsInProject(Guid projectId, ProjectSponsorQueryParams queryParams, string requesterEmail, bool isAdmin = false);
    Task<PagedList<ProjectSponsorTransaction>> GetAllSponsorTransactions(ProjectSponsorTransactionsQueryParams queryParams);
    Task<PagedList<ProjectSponsorTransaction>> GetProjectSponsorTransaction(Guid projectSponsorId, ProjectSponsorTransactionsQueryParams queryParams);
    Task<bool> ProjectSponsorDeposit(ProjectSponsorDepositDTO dto);
    Task<ProjectSponsor> UpdateSponsorStatusInProject(ProjectSponsorUpdateDTO dto);
  }
}