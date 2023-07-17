using Application.DTOs;
using Application.DTOs.Supplier;
using Application.DTOs.Voucher;
using Application.Helpers;
using Application.QueryParams.Supplier;
using Application.QueryParams.Voucher;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
  public class SupplierController : BaseApiController
  {
    private readonly IMapper _mapper;
    private readonly ISupplierService _supplierService;

    public SupplierController(
                          ISupplierService supplierService,
                          IMapper mapper)
    {
      _supplierService = supplierService;
      _mapper = mapper;
    }

    [Authorize]
    [SwaggerOperation("Get All Suppliers")]
    [HttpGet("")]
    public async Task<ActionResult<ResponseDTO<List<SupplierDTO>>>> GetAllSupplier([FromQuery] SupplierQueryParams queryParams)
    {
      var result = await _supplierService.GetAllSupplier(queryParams);
      return result.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Create new supplier")]
    [HttpPost("")]
    public async Task<ActionResult<ResponseDTO<Guid>>> CreateSupplier([FromBody] SupplierCreateDTO dto)
    {
      var result = await _supplierService.CreateSupplier(dto);
      return result.FormatAsResponseDTO(200);
    }
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Update voucher")]
    [HttpPatch("")]
    public async Task<ActionResult<ResponseDTO<bool>>> UpdateSupplier([FromBody] SupplierUpdateDTO dto)
    {
      var result = await _supplierService.UpdateSupplier(dto);
      return result.FormatAsResponseDTO(200);
    }

    [Authorize]
    [SwaggerOperation("[ADMIN] Get Supplier By Id")]
    [HttpGet("{supId}")]
    public async Task<ActionResult<ResponseDTO<SupplierDTO>>> GetVoucherById([FromRoute] Guid supId)
    {
      var supl = await _supplierService.GetSupplier(supId);
      return supl.FormatAsResponseDTO(200);
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
