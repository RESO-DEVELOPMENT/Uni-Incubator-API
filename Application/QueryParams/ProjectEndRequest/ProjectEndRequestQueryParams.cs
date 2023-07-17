using Application.Domain.Enums.ProjectEndRequest;
using Application.Helpers;

namespace Application.QueryParams.ProjectEndRequest
{
    public class ProjectEndRequestQueryParams : PaginationParams
    {
        public Guid? ProjectId { get; set; }
        public List<ProjectEndRequestStatus> Status { get; set; } = new List<ProjectEndRequestStatus>();
        public ProjectEndRequestOrderBy OrderBy { get; set; } = ProjectEndRequestOrderBy.CreatedAtDesc;
        public List<ProjectEndRequestPointAction> PointAction { get; set; } = new List<ProjectEndRequestPointAction>();
    }
}
