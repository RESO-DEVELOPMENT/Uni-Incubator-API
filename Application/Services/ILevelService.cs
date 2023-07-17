using Application.Domain.Models;
using Application.DTOs.Level;
using Application.Helpers;
using Application.QueryParams.Level;

namespace Application.Services
{
  public interface ILevelService
  {
    Task<Level> CreateNewLevel(LevelCreateDTO dto);
    Task<List<Level>> CreateNewLevels(LevelCreateBulkDTO dto);
    Task<PagedList<Level>> GetAllLevels(LevelQueryParams queryParams);
    Task<bool> UpdateLevel(LevelUpdateDTO dto);
  }
}