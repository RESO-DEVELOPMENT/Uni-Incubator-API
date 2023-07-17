using Application.Domain.Enums.Project;
using Application.DTOs.ProjectFile;

namespace Application.DTOs.Project
{
    public class ProjectWithFilesDTO
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string ProjectShortName { get; set; } = null!;
        public string ProjectShortDescription { get; set; } = null!;

        // public int MemberCount { get; set; }

        public ProjectStatus ProjectStatus { get; set; }
        public ProjectType ProjectType { get; set; }
        public ProjectVisibility ProjectVisibility { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<ProjectFileDTO> ProjectFiles { get; set; } = new List<ProjectFileDTO>();
    }
}