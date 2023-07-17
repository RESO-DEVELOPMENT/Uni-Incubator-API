using Application.Domain.Models;
using Application.DTOs.ProjectReport;
using Application.Helpers;
using Application.QueryParams.ProjectReport;
using Microsoft.AspNetCore.Mvc;

namespace Application.Services
{
    public interface IProjectReportService
    {
        Task<Guid> CreateProjectReport(Guid projectId, ProjectReportCreateDTO dto, string requesterEmail);
        Task<ProjectReportEstimateDTO> EstimateProjectReportReward(Guid projectReportId, string requesterEmail, bool isAdmin = false);
        Task<ProjectReportEstimateDTO> EstimateProjectReportReward(ProjectReportEstimateCreateDTO dto, string requesterEmail, bool isAdmin = false);

        Task<ProjectReportWithTasksDTO> GetProjectReport(Guid reportId, string requesterEmail, bool isAdmin = false);
        Task<List<ProjectReportWithTasksDTO>> GetProjectReports(Guid projectId, ProjectReportQueryParams queryParams,
            string requesterEmail);
        Task<List<ProjectReportWithProjectAndSalaryCycleDTO>> GetProjectsReports(ProjectsReportQueryParams queryParams, string requesterEmail, bool isAdmin = false);

        Task<bool> UpdateProjectReport(ProjectReportUpdateDTO dto, string requesterEmail);
        Task<bool> UpdateProjectReportStatus(ProjectReportStatusUpdateDTO dto, string requesterEmail, bool isAdmin = false);

        Task<FileStreamResult> GetReportTemplate(ProjectReportTemplateQueryDTO dto, string requesterEmail);
    }
}