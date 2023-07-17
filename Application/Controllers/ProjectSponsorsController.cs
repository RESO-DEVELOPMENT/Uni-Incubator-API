using Application.DTOs;
using Application.DTOs.ProjectSponsor;
using Application.DTOs.ProjectSponsorTransaction;
using Application.Helpers;
using Application.QueryParams.ProjectSponsor;
using Application.QueryParams.ProjectSponsorTransaction;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
  [Route("v1/projects")]
  public class ProjectSponsorsController : BaseApiController
  {
    private readonly IProjectSponsorService _projectSponsorService;
    private readonly IMapper _mapper;

    public ProjectSponsorsController(
                              IProjectSponsorService projectSponsorService,
                              IMapper mapper)
    {
      _projectSponsorService = projectSponsorService;
      _mapper = mapper;
    }

    [Authorize]
    [HttpGet("{projectId}/sponsors")]
    [SwaggerOperation("[ADMIN] [USER (PM)] Get project sponsors")]
    public async Task<ResponseDTO<List<ProjectSponsorDetailedDTO>>> GetProjectSponsors([FromRoute] Guid projectId, [FromQuery] ProjectSponsorQueryParams queryParams)
    {
      var result = await _projectSponsorService.GetAllSponsorsInProject(projectId, queryParams, User.GetEmail(), User.IsAdmin());
      Pagination.AddPaginationHeader(Response, result);
      var mappedSponsors = _mapper.Map<List<ProjectSponsorDetailedDTO>>(result);
      return mappedSponsors.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("sponsors/transactions")]
    [SwaggerOperation("[ADMIN/PM] Get projects sponsors transactions")]
    public async Task<ResponseDTO<List<ProjectSponsorTransactionDTO>>> GetProjectSponsorsTranasctions([FromQuery] ProjectSponsorTransactionsQueryParams queryParams)
    {
      var result = await _projectSponsorService.GetAllSponsorTransactions(queryParams);
      Pagination.AddPaginationHeader(Response, result);
      var mappedSponsors = _mapper.Map<List<ProjectSponsorTransactionDTO>>(result);
      return mappedSponsors.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("{projectId}/sponsors")]
    [SwaggerOperation("[ADMIN] Add sponsor to project")]
    public async Task<ResponseDTO<ProjectSponsorDTO>> AddProjectSponsor([FromRoute] Guid projectId, [FromBody] ProjectSponsorCreateDTO dto)
    {
      var result = await _projectSponsorService.AddSponsorToProject(projectId, dto);

      var mapped = _mapper.Map<ProjectSponsorDTO>(result);

      return mapped.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("sponsors/status")]
    [SwaggerOperation("[ADMIN] Update sponsor status in project")]
    public async Task<ResponseDTO<ProjectSponsorDTO>> UpdateProjectSponsor([FromBody] ProjectSponsorUpdateDTO dto)
    {
      var result = await _projectSponsorService.UpdateSponsorStatusInProject(dto);

      var mapped = _mapper.Map<ProjectSponsorDTO>(result);

      return mapped.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("sponsors/{projectSponsorId}/transactions")]
    [SwaggerOperation("[ADMIN] Get sponsors transactions")]
    public async Task<ResponseDTO<List<ProjectSponsorTransactionDTO>>> GetProjectSponsorTransaction([FromRoute] Guid projectSponsorId, [FromQuery] ProjectSponsorTransactionsQueryParams queryParams)
    {
      var result = await _projectSponsorService.GetProjectSponsorTransaction(projectSponsorId, queryParams);
      Pagination.AddPaginationHeader(Response, result);
      
      var mapped = _mapper.Map<List<ProjectSponsorTransactionDTO>>(result);

      return mapped.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("sponsors/deposit")]
    [SwaggerOperation("[ADMIN] Project Sponsor Deposit To Project")]
    public async Task<ResponseDTO<bool>> ProjectSponsorDeposit([FromBody] ProjectSponsorDepositDTO dto)
    {
      var result = await _projectSponsorService.ProjectSponsorDeposit(dto);

      var mapped = _mapper.Map<bool>(result);

      return mapped.FormatAsResponseDTO(200);
    }
  }
}
