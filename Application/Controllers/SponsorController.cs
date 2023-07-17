using Application.DTOs;
using Application.DTOs.Project;
using Application.DTOs.Sponsor;
using Application.Helpers;
using Application.QueryParams;
using Application.QueryParams.Project;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
  public class SponsorController : BaseApiController
  {
    private readonly IMapper _mapper;
    private readonly ISponsorService _sponsorService;

    public SponsorController(
        ISponsorService sponsorService,
                          IMapper mapper)
    {
      _sponsorService = sponsorService;
      _mapper = mapper;
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Get All Sponsors")]
    [HttpGet()]
    public async Task<ActionResult<ResponseDTO<List<SponsorDetailedDTO>>>> GetSponsors([FromQuery] SponsorQueryParams queryParams)
    {
      var sponsors = await _sponsorService.GetAllSponsors(queryParams);
      Response.AddPaginationHeader(sponsors);

      var mapped = _mapper.Map<List<SponsorDetailedDTO>>(sponsors);
      return mapped.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Get Sponsor By ID")]
    [HttpGet("{sponsorId}")]
    public async Task<ActionResult<ResponseDTO<SponsorDetailedDTO>>> GetSponsor([FromRoute] Guid sponsorId)
    {
      var sponsor = await _sponsorService.GetSponsorById(sponsorId);

      var mapped = _mapper.Map<SponsorDetailedDTO>(sponsor);
      return mapped.FormatAsResponseDTO<SponsorDetailedDTO>(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Get Sponsor Joined Projects")]
    [HttpGet("{sponsorId}/projects")]
    public async Task<ActionResult<ResponseDTO<List<ProjectDTO>>>> GetSponsorsJoinedProjects([FromRoute] Guid sponsorId, [FromQuery] ProjectForSponsorQueryParams queryParams)
    {
      var projects = await _sponsorService.GetSponsorJoinedProjects(queryParams, sponsorId);
      Response.AddPaginationHeader(projects);

      var mapped = _mapper.Map<List<ProjectDTO>>(projects);
      return mapped.FormatAsResponseDTO<List<ProjectDTO>>(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Create Sponsors")]
    [HttpPost()]
    public async Task<ActionResult<ResponseDTO<Guid>>> CreateSponsor([FromBody] SponsorCreateDTO dto)
    {
      var sponsorId = await _sponsorService.CreateSponsor(dto);
      return sponsorId.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Update Sponsor")]
    [HttpPatch()]
    public async Task<ActionResult<ResponseDTO<bool>>> UpdateSponsor([FromBody] SponsorUpdateDTO dto)
    {
      var result = await _sponsorService.UpdateSponsor(dto);
      return result.FormatAsResponseDTO(200);
    }

  }
}
