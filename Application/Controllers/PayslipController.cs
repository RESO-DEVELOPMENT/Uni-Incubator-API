using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.DTOs.Payslip;
using Application.Helpers;
using Application.QueryParams.Payslip;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
  public class PayslipController : BaseApiController
  {
    private readonly IMapper _mapper;
    private readonly IPayslipService _payslipService;

    public PayslipController(IMapper mapper, IPayslipService service)
    {
      this._mapper = mapper;
      this._payslipService = service;
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    [SwaggerOperation("[ADMIN] Get payslips")]
    public async Task<ResponseDTO<List<PayslipDTO>>> GetPayslips([FromQuery] PayslipQueryParams queryParam)
    {
      var payslips = await _payslipService.GetPayslips(queryParam);

      Response.AddPaginationHeader(payslips);
      var mappedResult = _mapper.Map<List<PayslipDTO>>(payslips);
      return mappedResult.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("{payslipId}")]
    [SwaggerOperation("[ADMIN] Get payslips by id")]
    public async Task<ResponseDTO<PayslipDTO>> GetPayslipById(Guid payslipId)
    {
      var payslip = await _payslipService.GetPayslipsById(payslipId);

      var mappedResult = _mapper.Map<PayslipDTO>(payslip);
      return mappedResult.FormatAsResponseDTO(200);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("total")]
    [SwaggerOperation("[ADMIN] Get payslips total")]
    public async Task<ResponseDTO<PayslipsTotalDTO>> GetPayslipsInfo([FromQuery] PayslipTotalQueryParams queryParam)
    {
      var payslipsInfo = await _payslipService.GetPayslipsTotal(queryParam);

      return payslipsInfo.FormatAsResponseDTO(200);
    }
  }
}