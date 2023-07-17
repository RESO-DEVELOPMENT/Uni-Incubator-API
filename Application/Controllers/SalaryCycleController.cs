using Application.DTOs;
using Application.DTOs.MemberLevel;
using Application.DTOs.SalaryCycle;
using Application.Helpers;
using Application.QueryParams.SalaryCycle;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
  [Authorize]
  public class SalaryCycleController : BaseApiController
  {
    private readonly IMapper _mapper;
    private readonly ISalaryCycleService _salaryCycleService;

    public SalaryCycleController(ISalaryCycleService salaryCycleService,
                          IMapper mapper)
    {
      _salaryCycleService = salaryCycleService;
      _mapper = mapper;
    }

    [SwaggerOperation("Get all salary cycles")]
    [HttpGet("")]
    public async Task<ActionResult<ResponseDTO<List<SalaryCycleDTO>>>> GetAllSalaryCycle([FromQuery] SalaryCycleQueryParams queryParams)
    {
      var result = await _salaryCycleService.GetAllSalaryCycle(queryParams);

      Response.AddPaginationHeader(result);
      var mappedScs = _mapper.Map<List<SalaryCycleDTO>>(result);
      return mappedScs.FormatAsResponseDTO(200);
    }

    [SwaggerOperation("Get salary cycle by Id")]
    [HttpGet("{scId}")]
    public async Task<ActionResult<ResponseDTO<SalaryCycleWithPayslipDTO>>> GetSalaryCycleById([FromRoute] Guid scId)
    {
      var result = await _salaryCycleService.GetSalaryCycleById(scId);

      var mapped = _mapper.Map<SalaryCycleWithPayslipDTO>(result);
      return mapped.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Create salary cycle")]
    [HttpPost("")]
    public async Task<ActionResult<ResponseDTO<SalaryCycleDTO>>> CreateSalaryCycle([FromBody] SalaryCycleCreateDTO dto)
    {
      var result = await _salaryCycleService.CreateSalaryCycle(dto);

      var mapped = _mapper.Map<SalaryCycleDTO>(result);
      return mapped.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Update salary cycle status")]
    [HttpPut("status")]
    public async Task<ActionResult<ResponseDTO<string>>> UpdateSalaryCycleStatus([FromBody] SalaryCycleUpdateDTO dto)
    {
      var result = await _salaryCycleService.UpdateSalaryCycleStatus(dto, User.GetEmail());
      if (!result) return "There something wrong with your request, please try again!".FormatAsResponseDTO(400);
      return "Your request are being processing, please wait!".FormatAsResponseDTO(200);
    }

    [SwaggerOperation("Get all member who level up in the salary cycle duration")]
    [HttpGet("{salaryCycleId}/members-level-up")]
    public async Task<ActionResult<ResponseDTO<List<MemberLevelWithMemberDTO>>>>
    GetAllMemberLevelUpInSalaryCycle([FromRoute] Guid salaryCycleId, [FromQuery] SalaryCycleMemberLevelUpQueryParams queryParams)
    {
      var result = await _salaryCycleService.GetAllMemberLevelUpInSalaryCycle(salaryCycleId, queryParams);

      Response.AddPaginationHeader(result);
      var mappedScs = _mapper.Map<List<MemberLevelWithMemberDTO>>(result);
      return mappedScs.FormatAsResponseDTO(200);
    }
  }

}