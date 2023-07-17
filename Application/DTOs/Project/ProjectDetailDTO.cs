using Application.Domain.Enums.Project;
using Application.DTOs.ProjectFile;
using Application.DTOs.ProjectMember;

namespace Application.DTOs.Project
{
  public class ProjectDetailDTO
  {
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public string ProjectShortName { get; set; } = null!;
    public string ProjectShortDescription { get; set; } = null!;
    public string ProjectLongDescription { get; set; } = null!;

    public List<ProjectMemberDTO> Members { get; set; } = new List<ProjectMemberDTO>();

    public double Budget { get; set; }
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