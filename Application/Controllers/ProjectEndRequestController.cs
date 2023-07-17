using Application.DTOs;
using Application.DTOs.Project;
using Application.DTOs.ProjectEndRequest;
using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectMemberRequest;
using Application.Helpers;
using Application.QueryParams.ProjectEndRequest;
using Application.QueryParams.ProjectMemberRequest;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    [Route("v1/projects")]
    public class ProjectEndRequestController : BaseApiController
    {
        private readonly IProjectEndRequestService _projectEndRequestService;

        public ProjectEndRequestController(
                                  IProjectEndRequestService projectEndRequestService)
        {
            _projectEndRequestService = projectEndRequestService;
        }

        [Authorize]
        [HttpGet("{projectId}/end-requests")]
        [SwaggerOperation("[Admin/PM] View End Requests of Project")]
        public async Task<ResponseDTO<List<ProjectEndRequestDTO>>> ViewProjectEndRequests(
          [FromRoute] Guid projectId, [FromQuery] ProjectEndRequestQueryParams queryParams)
        {
            var result = await _projectEndRequestService.GetAllRequestToEndFromProject(projectId, queryParams, User.GetEmail(), User.IsAdmin());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("end-requests")]
        [SwaggerOperation("[Admin] View End Requests of Project / [PM] View End Requests of Your Managed Project")]
        public async Task<ResponseDTO<List<ProjectEndRequestDTO>>> ViewProjectsEndRequests(
           [FromQuery] ProjectEndRequestQueryParams queryParams)
        {
            var result = await _projectEndRequestService.GetAllRequestToEnd(queryParams, User.GetEmail(), User.IsAdmin());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("end-requests/{requestId}")]
        [SwaggerOperation("[Admin] View End Requests of Project / [PM] View End Requests of Your Managed Project")]
        public async Task<ResponseDTO<ProjectEndRequestDTO>> ViewProjectEndRequestsById(
            [FromRoute] Guid requestId)
        {
            var result = await _projectEndRequestService.GetRequestToEnd(requestId, User.GetEmail(), User.IsAdmin());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPost("{projectId}/end-requests")]
        [SwaggerOperation("[PM] Request to end project")]
        public async Task<ResponseDTO<Guid>> RequestToEnd(
    [FromRoute] Guid projectId, [FromBody] ProjectEndRequestCreateDTO dto)
        {
            var result = await _projectEndRequestService.RequestToEnd(projectId, dto, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize(Roles ="ADMIN")]
        [HttpPut("end-requests")]
        [SwaggerOperation("[Admin] Review to end project")]
        public async Task<ResponseDTO<bool>> ReviewRequestToEnd([FromBody] ProjectEndRequestReviewDTO dto)
        {
            var result = await _projectEndRequestService.ReviewRequestToEnd(dto, User.GetEmail(), User.IsAdmin());
            return result.FormatAsResponseDTO(200);
        }
    }
}
