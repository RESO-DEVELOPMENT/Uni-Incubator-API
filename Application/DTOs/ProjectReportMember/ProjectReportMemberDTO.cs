using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectReportMemberTask;

namespace Application.DTOs.ProjectReportMember
{
    public class ProjectReportMemberDTO
    {
        public ProjectMemberDTO ProjectMember { get; set; } = null!;

        public List<ProjectReportMemberTaskDTO> Tasks { get; set; } = new List<ProjectReportMemberTaskDTO>();
    }
}