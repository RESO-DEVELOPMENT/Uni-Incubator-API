using Application.DTOs;
using Application.DTOs.MemberVoucher;
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
    public class MemberVoucherController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IMemberVoucherService _memberVoucherService;

        public MemberVoucherController(
            IMemberVoucherService mvService,
                              IMapper mapper)
        {
            _memberVoucherService = mvService;
            _mapper = mapper;
        }

        [Authorize(Roles = "ADMIN")]
        [SwaggerOperation("[ADMIN] Get Member Voucher From Code")]
        [HttpGet("{memberVoucherCode}")]
        public async Task<ActionResult<ResponseDTO<MemberVoucherDTO>>> GetMemberVoucherCode([FromRoute] string memberVoucherCode)
        {
            var result = await _memberVoucherService.GetMemberVoucherFromCode(memberVoucherCode);

            var mappedMv = _mapper.Map<MemberVoucherDTO>(result);
            return mappedMv.FormatAsResponseDTO(200);
        }

        [Authorize]
        [SwaggerOperation("[ADMIN/Self] Update Member Voucher")]
        [HttpPut("")]
        public async Task<ActionResult<ResponseDTO<bool>>> UpdateMemberVoucherStatus([FromBody] MemberVoucherUpdateStatusDTO dto)
        {
            var result = await _memberVoucherService.UpdateMemberVoucherStatus(dto, User.GetEmail(), User.IsAdmin());
            return result.FormatAsResponseDTO(200);
        }
    }
}
