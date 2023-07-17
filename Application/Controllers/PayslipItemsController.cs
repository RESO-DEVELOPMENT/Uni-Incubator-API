using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.DTOs.Payslip;
using Application.DTOs.PayslipItem;
using Application.DTOs.PayslipItems;
using Application.Helpers;
using Application.QueryParams.Payslip;
using Application.QueryParams.PayslipItem;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    public class PayslipItemsController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPayslipItemService _psiService;

        public PayslipItemsController(IMapper mapper, IPayslipItemService service)
        {
            this._mapper = mapper;
            this._psiService = service;
        }

        [Authorize]
        [HttpGet]
        [SwaggerOperation("Get payslips items (Query self if not admin)")]
        public async Task<ResponseDTO<List<PayslipItemDTO>>> GetPayslipItems([FromQuery] PayslipItemQueryParams queryParam)
        {
            var payslips = await _psiService.GetPayslipItems(queryParam, User.GetEmail(), User.IsAdmin());

            Response.AddPaginationHeader(payslips);
            var mappedResult = _mapper.Map<List<PayslipItemDTO>>(payslips);
            return mappedResult.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("total")]
        [SwaggerOperation("Get payslips items total (Query self if not admin)")]
        public async Task<ResponseDTO<PayslipItemsTotalDTO>> GetPayslipItemsTotal([FromQuery] PayslipItemTotalQueryParams queryParam)
        {
            var total = await _psiService.GetPayslipItemsTotal(queryParam, User.GetEmail(), User.IsAdmin());
            return total.FormatAsResponseDTO(200);
        }

    }
}