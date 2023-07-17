using Application.DTOs;
using Application.DTOs.AttributeGroup;
using Application.Helpers;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AttributesController : BaseApiController
    {
        private readonly IAttributeService _attributeService;

        public AttributesController(
            IAttributeService attributeService)
        {
            _attributeService = attributeService;
        }

        [SwaggerOperation("[ADMIN] Get All Attribute Groups")]
        [HttpGet("group")]
        public async Task<ActionResult<ResponseDTO<List<AttributeGroupDTO>>>> GetAttributeGroups()
        {
            var attgs = await _attributeService.GetAllAttributeGroups();

            return attgs.FormatAsResponseDTO(200);
        }

    }
}
