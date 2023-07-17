using Application.DTOs;
using Application.DTOs.Voucher;
using Application.Helpers;
using Application.QueryParams.Voucher;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
  public class VouchersController : BaseApiController
  {
    private readonly IMapper _mapper;
    private readonly IVoucherService _voucherService;
    private readonly IMemberVoucherService _memberVoucherService;

    public VouchersController(
                          IVoucherService vService,
                          IMemberVoucherService mvService,
                          IMapper mapper)
    {
      _voucherService = vService;
      _memberVoucherService = mvService;
      _mapper = mapper;
    }

    [Authorize]
    [SwaggerOperation("Get All Vouchers")]
    [HttpGet("")]
    public async Task<ActionResult<ResponseDTO<List<VoucherDTO>>>> GetAllVouchers([FromQuery] VoucherQueryParams queryParams)
    {
      var result = await _voucherService.GetAllVoucher(queryParams);

      Response.AddPaginationHeader(result);
      var mappedVouchers = _mapper.Map<List<VoucherDTO>>(result);
      return mappedVouchers.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Create new voucher")]
    [HttpPost("")]
    public async Task<ActionResult<ResponseDTO<VoucherDTO>>> CreateVoucher([FromBody] VoucherCreateDTO dto)
    {
      var levels = await _voucherService.CreateNewVoucher(dto);
      var mappedVoucher = _mapper.Map<VoucherDTO>(levels);

      return mappedVoucher.FormatAsResponseDTO(200);
    }
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation("[ADMIN] Update voucher")]
    [HttpPatch("")]
    public async Task<ActionResult<ResponseDTO<VoucherDTO>>> UpdateVoucher([FromBody] VoucherUpdateDTO dto)
    {
      var levels = await _voucherService.UpdateVoucher(dto);
      var mappedLevel = _mapper.Map<VoucherDTO>(levels);

      return mappedLevel.FormatAsResponseDTO(200);
    }

    [Authorize]
    [SwaggerOperation("Get voucher by id")]
    [HttpGet("{voucherId}")]
    public async Task<ActionResult<ResponseDTO<VoucherDTO>>> GetVoucherById([FromRoute] Guid voucherId)
    {
      var voucherDTO = await _voucherService.GetVoucherById(voucherId);
      return voucherDTO.FormatAsResponseDTO<VoucherDTO>(200);
    }

    [Authorize]
    [SwaggerOperation("Buy voucher")]
    [HttpPost("{voucherId}/action")]
    public async Task<ActionResult<ResponseDTO<String>>> BuyVoucher([FromRoute] Guid voucherId, VoucherActionDTO dto)
    {
      if (dto.Action == VoucherAction.Buy)
      {
        var result = await _memberVoucherService.BuyVoucher(voucherId, User.GetEmail(), dto.PinCode);
        return "Your purchase is being processed".FormatAsResponseDTO(200);
      }
      else
      {
        return "Invalid Action!".FormatAsResponseDTO(400);
      }
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
