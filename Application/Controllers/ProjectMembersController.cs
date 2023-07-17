using Application.DTOs;
using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectMemberRequest;
using Application.Helpers;
using Application.QueryParams.ProjectMemberRequest;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    [Route("v1/projects")]
    public class ProjectMembersController : BaseApiController
    {
        private readonly IProjectMemberService _projectMemberService;
        private readonly IMapper _mapper;

        public ProjectMembersController(
                                  IProjectMemberService projectMemberService,
                                  IMapper mapper)
        {
            _projectMemberService = projectMemberService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("{projectId}/members/requests")]
        [SwaggerOperation("[PM] View requests to join of project")]
        public async Task<ResponseDTO<List<ProjectMemberRequestDTO>>> ViewProjectRequests(
          [FromRoute] Guid projectId, [FromQuery] ProjectMemberRequestQueryParams queryParams)
        {
            var result = await _projectMemberService.GetProjectMemberRequests(projectId, queryParams, User.GetEmail());

            HttpContext.Response.AddPaginationHeader(result);
            var mappedPers = _mapper.Map<List<ProjectMemberRequestDTO>>(result);

            return mappedPers.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{projectId}/members")]
        [SwaggerOperation("[Admin/MemberInProject] View project members detailed information")]
        public async Task<ResponseDTO<List<ProjectMemberDetailedDTO>>> ViewProjectMembersDetailed([FromRoute] Guid projectId)
        {
            var result = await _projectMemberService.GetProjectMembersDetailed(projectId, User.GetEmail(), User.IsAdmin());

            // Pagination.AddPaginationHeader(HttpContext.Response, result);
            var mappedPms = _mapper.Map<List<ProjectMemberDetailedDTO>>(result);

            return mappedPms.FormatAsResponseDTO(200);
        }


        [Authorize]
        [HttpGet("members/{projectMemberId}")]
        [SwaggerOperation("[Admin/MemberInProject] View a project member detailed information")]
        public async Task<ResponseDTO<ProjectMemberDetailedDTO>> ViewProjectMemberDetailed([FromRoute] Guid projectMemberId)
        {
            var result = await _projectMemberService.GetProjectMemberDetailed(projectMemberId, User.GetEmail(), User.IsAdmin());

            // Pagination.AddPaginationHeader(HttpContext.Response, result);
            var mappedPms = _mapper.Map<ProjectMemberDetailedDTO>(result);

            return mappedPms.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPut("members/status")]
        [SwaggerOperation("[PM] Update project member status")]
        public async Task<ResponseDTO<bool>> UpdateProjectMemberStatus([FromBody] ProjectMemberUpdateStatusDTO dto)
        {
            var result = await _projectMemberService.UpdateProjectMemberStatus(dto, User.GetEmail());
            return result.FormatAsResponseDTO<bool>(200);
        }

        [Authorize]
        [HttpPost("{projectId}/members/requests")]
        [SwaggerOperation("Send request to join project")]
        public async Task<ResponseDTO<Guid>> ChangeProjectPM(
          [FromRoute] Guid projectId, ProjectMemberRequestCreateDTO dto)
        {
            var result = await _projectMemberService.RequestToJoin(projectId, dto, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("{projectId}/members/pm")]
        [SwaggerOperation("[ADMIN] Change Project Manager")]
        public async Task<ResponseDTO<bool>> ChangeProjectManager(
            [FromRoute] Guid projectId, ProjectMemberChangePMDTO dto)
        {
            var result = await _projectMemberService.ChangeProjectManager(projectId, dto, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPut("members/requests")]
        [SwaggerOperation("[PM] Review / [USER] Cancel request to join project")]
        public async Task<ResponseDTO<bool>> ReviewRequestToJoinProject(ProjectMemberRequestReviewDTO dto)
        {
            var result = await _projectMemberService.ReviewRequestToJoin(dto.RequestId, dto, User.GetEmail());

            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPut("members")]
        [SwaggerOperation("[PM] Update project member attributes")]
        public async Task<ResponseDTO<bool>> UpdateProjectMember(ProjectMemberUpdateDTO dto)
        {
            var result = await _projectMemberService.UpdateProjectMember(dto, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }
    }
}
