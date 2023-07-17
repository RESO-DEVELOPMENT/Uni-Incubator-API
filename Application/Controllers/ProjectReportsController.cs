using Application.DTOs;
using Application.DTOs.ProjectReport;
using Application.Helpers;
using Application.QueryParams.ProjectReport;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Application.Controllers
{
    [Route("v1/projects")]
    public class ProjectReportsController : BaseApiController
    {
        private readonly IProjectReportService _projectReportService;
        private readonly IMapper _mapper;

        public ProjectReportsController(IProjectReportService projectReportService,
                                  IMapper mapper)
        {
            _projectReportService = projectReportService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("{projectId}/reports")]
        [SwaggerOperation("Get all reports for your project")]
        public async Task<ResponseDTO<List<ProjectReportWithTasksDTO>>> GetProjectReport([FromRoute] Guid projectId, [FromQuery] ProjectReportQueryParams queryParams)
        {
            var result = await _projectReportService.GetProjectReports(projectId, queryParams, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("reports/template")]
        [SwaggerOperation("Get Project Report Template")]
        public async Task<IActionResult> GetProjectReportTemplate([FromQuery] ProjectReportTemplateQueryDTO dto)
        {
            var result = await _projectReportService.GetReportTemplate(dto, User.GetEmail());
            return File(result.FileStream, MediaTypeNames.Application.Octet, result.FileDownloadName);
        }

        [Authorize]
        [HttpGet("reports")]
        [SwaggerOperation("[ADMIN] Get all project reports / [PM] Get all project report where you are PM")]
        public async Task<ResponseDTO<List<ProjectReportWithProjectAndSalaryCycleDTO>>> GetProjectReports([FromQuery] ProjectsReportQueryParams queryParams)
        {
            var result = await _projectReportService.GetProjectsReports(queryParams, User.GetEmail(), User.IsAdmin());
            return result.FormatAsResponseDTO(200);
        }

        [HttpGet("reports/{reportId}")]
        [SwaggerOperation("Get project report detailed")]
        public async Task<ResponseDTO<ProjectReportWithTasksDTO>> GetProjectReportDetailed([FromRoute] Guid reportId)
        {
            var result = await _projectReportService.GetProjectReport(reportId, User.GetEmail(), User.IsAdmin());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPost("reports/{projectReportId}/estimate")]
        [SwaggerOperation("[PM] Estimate member's earning based on report")]
        public async Task<ResponseDTO<ProjectReportEstimateDTO>> EstimateProjectReport(
          [FromRoute] Guid projectReportId)
        {
            var result = await _projectReportService.EstimateProjectReportReward(projectReportId, User.GetEmail(), User.IsAdmin());

            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPost("reports/estimate")]
        [SwaggerOperation("[PM] Estimate member's earning based on new report")]
        public async Task<ResponseDTO<ProjectReportEstimateDTO>> EstimateNewProjectReport(
          [FromBody] ProjectReportEstimateCreateDTO dto)
        {
            var result = await _projectReportService.EstimateProjectReportReward(dto, User.GetEmail(), User.IsAdmin());

            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPost("{projectId}/reports")]
        [SwaggerOperation("[PM] Send project report to admin")]
        public async Task<ResponseDTO<Guid>> CreateProjectReport(
          [FromRoute] Guid projectId, ProjectReportCreateDTO dto)
        {
            var result = await _projectReportService.CreateProjectReport(projectId, dto, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPut("reports")]
        [SwaggerOperation("[PM] Update project report")]
        public async Task<ResponseDTO<bool>> UpdateProjectReport(
        [FromBody] ProjectReportUpdateDTO dto)
        {
            var result = await _projectReportService.UpdateProjectReport(dto, User.GetEmail());

            return result.FormatAsResponseDTO(200);
        }


        [Authorize]
        [HttpPut("reports/status")]
        [SwaggerOperation("[Admin/PM] Update Project Report Status")]
        public async Task<ResponseDTO<bool>> ReviewProjectReport([FromBody] ProjectReportStatusUpdateDTO dto)
        {
            var result = await _projectReportService.UpdateProjectReportStatus(dto, User.GetEmail(), User.IsAdmin());

            // var mapped = _mapper.Map<bool>(result);
            return result.FormatAsResponseDTO(200);
        }
    }
}
