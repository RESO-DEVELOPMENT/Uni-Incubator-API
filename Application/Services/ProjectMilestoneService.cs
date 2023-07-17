using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.ProjectMilestone;
using Application.Domain.Models;
using Application.DTOs.ProjetMilestone;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.ProjectMilestone;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
  public class ProjectMilestoneService : IProjectMilestoneService
  {
    private readonly IMapper _mapper;
    private readonly UnitOfWork _unitOfWork;

    public ProjectMilestoneService(UnitOfWork unitOfWork,
                    IWalletService walletService,
                       IMapper mapper)
    {
      _unitOfWork = unitOfWork;
      _mapper = mapper;
    }

    /// <summary>
    /// Create new project
    /// </summary>
    public async Task<PagedList<ProjectMilestone>> GetProjectMilestone(Guid projectId, ProjectUpdateQueryParams queryParams)
    {
      var project = await _unitOfWork.ProjectRepository.GetQuery()
      .Where(p => p.ProjectId == projectId)
      .FirstOrDefaultAsync();
      if (project == null) throw new NotFoundException("There is no project with that id!", ErrorNameValues.ProjectNotFound);

      var query = _unitOfWork.ProjectMilestoneRepository
        .GetQuery()
        .Where(p => p.ProjectId == project.ProjectId && p.Status == ProjectMilestoneStatus.Available);

      switch (queryParams.OrderBy)
      {
        case ProjectMilestoneOrderBy.DateAsc:
          query = query.OrderBy(p => p.CreatedAt);
          break;
        case ProjectMilestoneOrderBy.DateDesc:
          query = query.OrderByDescending(p => p.CreatedAt);
          break;
      }

      var updates = await PagedList<ProjectMilestone>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
      return updates;
    }

    public async Task<ProjectMilestone> CreateProjectMilestone(Guid projectId, ProjectMilestoneCreateDTO dto, string requesterEmail)
    {
      var project = await _unitOfWork.ProjectRepository.GetQuery().Where(p => p.ProjectId == projectId).FirstOrDefaultAsync();
      if (project == null) throw new NotFoundException("There is no project with that id!", ErrorNameValues.ProjectNotFound);

      var requesterMember = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail);
      if (requesterMember == null) throw new NotFoundException();

      var projectMember = await _unitOfWork.ProjectMemberRepository.GetQuery()
        .Where(pm => pm.ProjectId == projectId &&
        pm.Status == ProjectMemberStatus.Active &&
        pm.MemberId == requesterMember.MemberId &&
        pm.Role == ProjectMemberRole.Manager)
        .FirstOrDefaultAsync();

      // Check if PM;
      if (projectMember == null)
        throw new BadRequestException("You don't have permission", ErrorNameValues.NoPermission);

      ProjectMilestone pmstone = new ProjectMilestone();
      _mapper.Map(dto, pmstone);
      pmstone.ProjectId = projectId;

      _unitOfWork.ProjectMilestoneRepository.Add(pmstone);
      await _unitOfWork.SaveAsync();

      return pmstone;
    }

    public async Task<bool> UpdateProjectMilestone(ProjectMilestoneUpdateDTO dto, String requesterEmail)
    {
      var projectMs = await _unitOfWork.ProjectMilestoneRepository.GetByID(dto.MilestoneId);
      if (projectMs == null) throw new NotFoundException("There is no milestone with that id!", ErrorNameValues.MilestoneNotFound);

      var requesterMember = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail);
      if (requesterMember == null) throw new NotFoundException();

      var projectMember = await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(projectMs.ProjectId, requesterEmail);

      // Check if PM;
      if (projectMember == null)
        throw new BadRequestException("You don't have permission", ErrorNameValues.NoPermission);

      // projectMs.Status = ProjectMilestoneStatus.Unavailable;
      projectMs.Title = dto.Title;
      projectMs.Content = dto.Content;
      
      _unitOfWork.ProjectMilestoneRepository.Update(projectMs);
      var result = await _unitOfWork.SaveAsync();

      return result;
    }

    public async Task<bool> DeleteProjectMilestone(Guid projectMilestoneId, String requesterEmail)
    {
      var projectMs = await _unitOfWork.ProjectMilestoneRepository.GetByID(projectMilestoneId);
      if (projectMs == null) throw new NotFoundException("There is no milestone with that id!", ErrorNameValues.MilestoneNotFound);

      var requesterMember = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail);
      if (requesterMember == null) throw new NotFoundException();

      var projectMember = await _unitOfWork.ProjectMemberRepository.GetQuery()
        .Where(pm => pm.ProjectId == projectMs.ProjectId &&
        pm.Status == ProjectMemberStatus.Active &&
        pm.MemberId == requesterMember.MemberId &&
        pm.Role == ProjectMemberRole.Manager)
        .FirstOrDefaultAsync();

      // Check if PM;
      if (projectMember == null)
        throw new BadRequestException("You don't have permission", ErrorNameValues.NoPermission);

      projectMs.Status = ProjectMilestoneStatus.Unavailable;

      _unitOfWork.ProjectMilestoneRepository.Update(projectMs);
      var result = await _unitOfWork.SaveAsync();

      return result;
    }
  }
}