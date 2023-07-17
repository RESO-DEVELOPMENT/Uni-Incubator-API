using Application.DTOs;
using Application.DTOs.ProjetMilestone;
using Application.Helpers;
using Application.QueryParams.ProjectMilestone;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
  [Route("v1/projects")]
  public class ProjectMilestonesController : BaseApiController
  {
    private readonly IProjectMilestoneService _projectMilestoneService;
    private readonly IMapper _mapper;

    public ProjectMilestonesController(
                              IProjectMilestoneService projectMilestoneService,
                              IMapper mapper)
    {
      _projectMilestoneService = projectMilestoneService;
      _mapper = mapper;
    }

    [Authorize]
    [HttpGet("{projectId}/milestones")]
    [SwaggerOperation("Get project's milestone")]
    public async Task<ResponseDTO<List<ProjectMilestoneDTO>>> GetProjectMilestone([FromRoute] Guid projectId,
    [FromQuery] ProjectUpdateQueryParams queryParams)
    {
      var result = await _projectMilestoneService.GetProjectMilestone(projectId, queryParams);

      Response.AddPaginationHeader(result);
      var mapped = _mapper.Map<List<ProjectMilestoneDTO>>(result);
      return mapped.FormatAsResponseDTO(200);
    }

    [Authorize]
    [HttpPost("{projectId}/milestones")]
    [SwaggerOperation("[PM] Create project's milestone ")]
    public async Task<ResponseDTO<ProjectMilestoneDTO>> CreateProjectMilestone([FromRoute] Guid projectId, [FromBody] ProjectMilestoneCreateDTO dto)
    {
      var result = await _projectMilestoneService.CreateProjectMilestone(projectId, dto, User.GetEmail());

      var mapped = _mapper.Map<ProjectMilestoneDTO>(result);
      return mapped.FormatAsResponseDTO(200);
    }

    [Authorize]
    [HttpPut ("milestones")]
    [SwaggerOperation("[PM] Update project's milestone ")]
    public async Task<ResponseDTO<bool>> UpdateProjectMilestone([FromBody] ProjectMilestoneUpdateDTO dto)
    {
      var result = await _projectMilestoneService.UpdateProjectMilestone(dto, User.GetEmail());
      return result.FormatAsResponseDTO(200);
    }

    [Authorize]
    [HttpDelete("milestones/{milestoneId}")]
    [SwaggerOperation("[[PM] Delete project's milestone ")]
    public async Task<ResponseDTO<bool>> DeleteProjectMilestone([FromRoute] Guid milestoneId)
    {
      var result = await _projectMilestoneService.DeleteProjectMilestone(milestoneId, User.GetEmail());
      return result.FormatAsResponseDTO(200);
    }
  }
}
