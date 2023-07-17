using Application.Domain.Enums.ProjectSponsor;
using Application.DTOs.Project;
using Application.DTOs.Sponsor;

namespace Application.DTOs.ProjectSponsor
{
  public class ProjectSponsorDTO
  {
    public Guid ProjectSponsorId { get; set; }
    public SponsorDTO Sponsor { get; set; } = null!;
    public ProjectWithFilesDTO Project { get; set; }

    public ProjectSponsorStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public double LastDepositAmount { get; set; }
    public DateTime? LastDepositDate { get; set; }

  }
}