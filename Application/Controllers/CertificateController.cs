using Application.DTOs;
using Application.DTOs.AttributeGroup;
using Application.DTOs.MemberExport;
using Application.Helpers;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.IO;
using System.Net.Mime;
using System.Text;

namespace Application.Controllers
{
    [Route("v1/certificate")]
    public class CertificateController : BaseApiController
    {
        private readonly ICertificateService _certificateService;

        public CertificateController(
            ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        [Authorize]
        [SwaggerOperation("[Member] Get Self Certificate Link")]
        [HttpGet("me/link")]
        public async Task<ActionResult<ResponseDTO<MemberExportLinkDTO>>> GetCertLink()
        {
            var result = await _certificateService.GetCertLink(User.GetEmail());

            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [SwaggerOperation("[Member] Get Self Certificate File")]
        [HttpGet("me")]
        public async Task<IActionResult> GetSelfCert()
        {
            var result = await _certificateService.GetMemberCert(User.GetEmail());
            return File(result.FileStream, MediaTypeNames.Application.Octet, result.FileDownloadName);
        }


        [SwaggerOperation("Get Member's Certificate File")]
        [HttpGet()]
        public async Task<IActionResult> GetMemberCert([FromQuery] Guid memberId, [FromQuery] string code)
        {
            var result = await _certificateService.GetMemberCert(memberId, code);
            return File(result.FileStream, MediaTypeNames.Application.Octet, result.FileDownloadName);
        }
    }
}
