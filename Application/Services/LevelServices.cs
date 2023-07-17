using Application.Domain.Enums.Level;
using Application.Domain.Models;
using Application.DTOs.Level;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.Level;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class LevelService : ILevelService
  {
    private readonly IMapper _mapper;
    private readonly UnitOfWork _unitOfWork;

    public LevelService(UnitOfWork unitOfWork,
                       IMapper mapper)
    {
      _unitOfWork = unitOfWork;
      _mapper = mapper;
    }

    /// <summary>
    /// Get all user
    /// </summary>
    /// <returns>User</returns>
    public async Task<PagedList<Level>> GetAllLevels(LevelQueryParams queryParams)
    {
      var query = _unitOfWork.LevelRepository.GetQuery();
      query = query.Where(level => level.Status == LevelStatus.Active);

      if (queryParams.LevelID != null) query = query.Where(l => l.LevelId == queryParams.LevelID);
      if (queryParams.LevelName != null) query = query.Where(l => l.LevelName.Contains(queryParams.LevelName));
      if (queryParams.MinXPNeeded != null) query = query.Where(l => queryParams.MinXPNeeded <= l.XPNeeded);

      switch (queryParams.OrderBy)
      {
        case LevelOrderBy.CreateAtAsc:
          query = query.OrderBy(l => l.CreatedAt);
          break;
        case LevelOrderBy.CreateAtDesc:
          query = query.OrderByDescending(l => l.CreatedAt);
          break;
        case LevelOrderBy.RequiredXPAsc:
          query = query.OrderBy(l => l.XPNeeded);
          break;
        case LevelOrderBy.RequiredXPDesc:
          query = query.OrderByDescending(l => l.XPNeeded);
          break;
        default:
          break;
      }

      return await PagedList<Level>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
    }

    public async Task<Level> CreateNewLevel(LevelCreateDTO dto)
    {
        var levelsWithSameXPNeeded = await _unitOfWork.LevelRepository
         .GetQuery()
         .Where(l => dto.XPNeeded == l.XPNeeded)
         .FirstOrDefaultAsync();

      if (levelsWithSameXPNeeded != null)
      {
        throw new BadRequestException($"There can't be 2 level with same XP needed [{levelsWithSameXPNeeded.LevelId}]", ErrorNameValues.LevelDuplicated);
      }

      var mappedLevel = _mapper.Map<Level>(dto);
      _unitOfWork.LevelRepository.Add(mappedLevel);

      var result = await _unitOfWork.SaveAsync();
      if (!result) throw new BadRequestException("Some error happen, try again!", ErrorNameValues.SystemError);

      return mappedLevel;
    }

    public async Task<List<Level>> CreateNewLevels(LevelCreateBulkDTO dto)
    {
        var levelsWithSameXPNeeded = await _unitOfWork.LevelRepository
         .GetQuery()
         .Where(l => dto.Levels.Select(dl => dl.XPNeeded).Contains(l.XPNeeded))
         .FirstOrDefaultAsync();

      if (levelsWithSameXPNeeded != null)
      {
        var levelCreatedThatDuplicated = dto.Levels.First(l => levelsWithSameXPNeeded.XPNeeded == l.XPNeeded);
        throw new BadRequestException($"There can't be 2 level with same XP needed [{levelCreatedThatDuplicated.LevelName} same with {levelsWithSameXPNeeded.LevelName}]", ErrorNameValues.LevelDuplicated);
      }

      var mappedLevelList = _mapper.Map<List<Level>>(dto.Levels);
      _unitOfWork.LevelRepository.Add(mappedLevelList);

      var result = await _unitOfWork.SaveAsync();
      if (!result) throw new BadRequestException("Some error happen, try again!", ErrorNameValues.SystemError);

      return mappedLevelList;
    }

    public async Task<bool> UpdateLevel(LevelUpdateDTO dto)
    {
      var level = await _unitOfWork.LevelRepository
        .GetQuery()
        .Where(l => dto.LevelId == l.LevelId)
        .FirstOrDefaultAsync();

      if (level == null)
        throw new NotFoundException($"Level not found!", ErrorNameValues.LevelNotFound);

      var levelsWithSameXPNeeded = await _unitOfWork.LevelRepository
         .GetQuery()
         .Where(l => dto.XPNeeded == l.XPNeeded && dto.LevelId != l.LevelId)
         .FirstOrDefaultAsync();

      if (levelsWithSameXPNeeded != null)
      {
        throw new BadRequestException($"There can't be 2 level with same XP needed [{levelsWithSameXPNeeded.LevelId}]", ErrorNameValues.LevelDuplicated);
      }

      _mapper.Map(dto, level);
      _unitOfWork.LevelRepository.Update(level);

      var result = await _unitOfWork.SaveAsync();
      if (!result) throw new BadRequestException("Some error happen, try again!", ErrorNameValues.SystemError);

      return result;
    }
  }
}