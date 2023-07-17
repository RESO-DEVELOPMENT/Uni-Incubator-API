using Application.Domain.Enums.Project;
using Application.Domain.Enums.ProjectMember;

namespace Application.DTOs.Member
{
    public class MemberProjectsDTO_ProjectCompactDTO
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;

        public ProjectStatus ProjectStatus { get; set; }
        public ProjectType ProjectType { get; set; }
        public ProjectVisibility ProjectVisibility { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ProjectMemberRole Role { get; set; }
    }

    public class MemberProjectsDTO
    {
        public int TotalProjects { get; set; }
        public int TotalManagedProjects { get; set; }
        public List<MemberProjectsDTO_ProjectCompactDTO> Projects { get; set; } = new List<MemberProjectsDTO_ProjectCompactDTO>();
    }
}