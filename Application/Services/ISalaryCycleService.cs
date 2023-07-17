using Application.Domain.Models;
using Application.DTOs.SalaryCycle;
using Application.Helpers;
using Application.QueryParams.SalaryCycle;

namespace Application.Services
{
  public interface ISalaryCycleService
  {
    Task<SalaryCycle> CreateSalaryCycle(SalaryCycleCreateDTO dto);

    Task<PagedList<SalaryCycle>> GetAllSalaryCycleOfMember(String memberEmail, SalaryCycleQueryParams queryParams);

    Task<PagedList<MemberLevel>> GetAllMemberLevelUpInSalaryCycle(Guid salaryCycleId, SalaryCycleMemberLevelUpQueryParams queryParams);
    Task<PagedList<SalaryCycle>> GetAllSalaryCycle(SalaryCycleQueryParams queryParams);

    Task<SalaryCycle> GetSalaryCycleById(Guid scId);
    Task<bool> UpdateSalaryCycleStatus(SalaryCycleUpdateDTO dto, String requesterEmail);
  }
}