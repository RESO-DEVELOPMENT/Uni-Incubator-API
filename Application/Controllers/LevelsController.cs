using Application.DTOs;
using Application.DTOs.Level;
using Application.Helpers;
using Application.QueryParams.Level;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
  public class LevelsController : BaseApiController
  {
    private readonly IMapper _mapper;
    private readonly ILevelService _levelService;

    public LevelsController(
                          ILevelService attributeService,
                          IMapper mapper)
    {
      _levelService = attributeService;
      _mapper = mapper;
    }
    [Authorize]
    [SwaggerOperation("Get All Levels")]
    [HttpGet("")]
    public async Task<ActionResult<ResponseDTO<List<LevelDTO>>>> GetAllLevels([FromQuery] LevelQueryParams queryParams)
    {
      var result = await _levelService.GetAllLevels(queryParams);

      Response.AddPaginationHeader(result);
      var mappedLevels = _mapper.Map<List<LevelDTO>>(result);
      return mappedLevels.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Create new level")]
    [HttpPost("")]
    public async Task<ActionResult<ResponseDTO<LevelDTO>>> CreateLevel([FromBody] LevelCreateDTO dto)
    {
      var level = await _levelService.CreateNewLevel(dto);
      var mappedLevel = _mapper.Map<LevelDTO>(level);

      return mappedLevel.FormatAsResponseDTO(200);
    }


    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Create new levels")] 
    [HttpPost("bulk")]
    public async Task<ActionResult<ResponseDTO<List<LevelDTO>>>> CreateLevels([FromBody] LevelCreateBulkDTO dto)
    {
      var levels = await _levelService.CreateNewLevels(dto);
      var mappedLevels = _mapper.Map<List<LevelDTO>>(levels);

      return mappedLevels.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Update Level")]
    [HttpPut("")]
    public async Task<ActionResult<ResponseDTO<bool>>> UpdateLevel([FromBody] LevelUpdateDTO dto)
    {
      var result = await _levelService.UpdateLevel(dto);

      return result.FormatAsResponseDTO(200);
    }

    // [SwaggerOperation("[ADMIN] Update or create new levels")]
    // [HttpPost("")]
    // public async Task<ActionResult<ResponseDTO<List<LevelDTO>>>> CreateLevel([FromBody] LevelCreateDTO dto)
    // {
    //   var levels = await _levelService.CreateNewLevel(dto);
    //   var mappedLevels = _mapper.Map<List<LevelDTO>>(levels);

    //   return mappedLevels.FormatAsResponseDTO<List<LevelDTO>>(200);
    // }
  }
}
