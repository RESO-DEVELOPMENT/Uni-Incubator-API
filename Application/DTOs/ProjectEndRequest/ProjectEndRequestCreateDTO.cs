using Application.Domain;
using Application.Domain.Enums.ProjectEndRequest;
using Application.DTOs.Project;

namespace Application.DTOs.ProjectEndRequest
{
    public class ProjectEndRequestCreateDTO
    {
        public ProjectEndRequestPointAction PointAction { get; set; }
        public string? Note { get; set; }
    }
}
