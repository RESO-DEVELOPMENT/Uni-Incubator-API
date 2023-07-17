using Application.Domain.Models;
using Application.DTOs.Sponsor;
using Application.Helpers;
using Application.QueryParams;
using Application.QueryParams.Project;

namespace Application.Services
{
  public interface ISponsorService
  {
    Task<Guid> CreateSponsor(SponsorCreateDTO dto);
    Task<PagedList<Sponsor>> GetAllSponsors(SponsorQueryParams queryParams);
    Task<Sponsor> GetSponsorById(Guid sponsorId);
    Task<PagedList<Project>> GetSponsorJoinedProjects(ProjectForSponsorQueryParams queryParams, Guid sponsorId);
    Task<bool> UpdateSponsor(SponsorUpdateDTO dto);
  }
}